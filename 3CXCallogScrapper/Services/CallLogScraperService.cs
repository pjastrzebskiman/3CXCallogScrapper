using System.Text.Json;
using System.Web;
using _3CXCallogScrapper.Models;
using _3CXCallogScrapper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace _3CXCallLogScraper.Services;

public class CallLogScraperService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticationService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ThreeCXApiSettings _apiSettings;
    private readonly ILogger<CallLogScraperService> _logger;

    public CallLogScraperService(
        IHttpClientFactory httpClientFactory,
        AuthenticationService authService,
        IOptions<ThreeCXApiSettings> apiSettings,
        ILogger<CallLogScraperService> logger,
        IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _apiSettings = apiSettings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Ensure database is set up
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                await dbService.EnsureDatabaseCreatedAsync();
            }

            // Set up polling interval
            var intervalMinutes = _apiSettings.QueryIntervalMinutes;
            _logger.LogInformation("Call log scraper service started. Polling every {IntervalMinutes} minutes", intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ScrapeCallLogsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while scraping call logs");
                }

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Graceful shutdown
            _logger.LogInformation("Call log scraper service is shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in call log scraper service");
            throw;
        }
    }

    private async Task ScrapeCallLogsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting call log scraping process");

        // Calculate time range
        var now = DateTime.UtcNow;
        var periodFrom = now.AddMinutes(-_apiSettings.LookbackMinutes).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var periodTo = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        _logger.LogInformation("Fetching call logs from {PeriodFrom} to {PeriodTo}", periodFrom, periodTo);

        // Build the query URL with parameters
        var queryParams = new Dictionary<string, string>
        {
            { "periodFrom", periodFrom },
            { "periodTo", periodTo },
            { "sourceType", "0" },
            { "sourceFilter", "'" + "'" },
            { "destinationType", "0" },
            { "destinationFilter", "'" + "'" },
            { "callsType", "0" },
            { "callTimeFilterType", "0" },
            { "callTimeFilterFrom", "'0:00:0'" },
            { "callTimeFilterTo", "'0:00:0'" },
            { "hidePcalls", "true" }
        };

        var queryString = string.Join(",", queryParams.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));

        // Lista do przechowywania wszystkich pobranych rekordów
        var allCallLogs = new List<CallLogEntry>();
        int pageSize = 100;
        int skip = 0;
        bool hasMoreRecords = true;

        // Pobieranie danych z paginacją
        while (hasMoreRecords && !cancellationToken.IsCancellationRequested)
        {
            var url = $"{_apiSettings.BaseUrl.TrimEnd('/')}{_apiSettings.CallLogEndpoint}({queryString})?$top={pageSize}&$skip={skip}&$orderby=SegmentId desc";
            _logger.LogInformation("Fetching page with skip={Skip}, top={Top}", skip, pageSize);

            // Get access token and set up the request
            var token = await _authService.GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {token}");

            // Execute the request with retry policy
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Error fetching call logs (Attempt {RetryCount}). Retrying in {RetryTimeSpan}...",
                            retryCount, timeSpan);
                    });

            var httpClient = _httpClientFactory.CreateClient();
            var response = await retryPolicy.ExecuteAsync(async () => await httpClient.SendAsync(request, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to fetch call logs. Status: {StatusCode}, Response: {ErrorContent}",
                    response.StatusCode, errorContent);
                return;
            }

            // Process the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var callLogResponse = JsonSerializer.Deserialize<CallLogResponse>(content);

            if (callLogResponse?.Value == null)
            {
                _logger.LogInformation("No call logs found for the specified period");
                break;
            }

            int recordsInPage = callLogResponse.Value.Count;
            _logger.LogInformation("Retrieved {Count} call log entries on page {Page}", recordsInPage, skip / pageSize + 1);

            // Dodaj pobrane rekordy do całkowitej listy
            allCallLogs.AddRange(callLogResponse.Value);

            // Sprawdź, czy jest więcej rekordów do pobrania
            if (recordsInPage < pageSize)
            {
                hasMoreRecords = false;
                _logger.LogInformation("Reached the end of available records");
            }
            else
            {
                // Przygotuj do pobrania następnej strony
                skip += pageSize;
            }
        }

        _logger.LogInformation("Total number of call log entries retrieved: {Count}", allCallLogs.Count);

        // Save to database
        if (allCallLogs.Any())
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                await dbService.SaveCallLogsAsync(allCallLogs);
            }
        }
    }
}