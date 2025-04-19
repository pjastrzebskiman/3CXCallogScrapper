using _3CXCallogScrapper.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace _3CXCallogScrapper.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ThreeCXApiSettings _apiSettings;
        private readonly ILogger<AuthenticationService> _logger;
        private string? _accessToken;
        private DateTime _tokenExpiryTime = DateTime.MinValue;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        public AuthenticationService(
            HttpClient httpClient,
            IOptions<ThreeCXApiSettings> apiSettings,
            ILogger<AuthenticationService> logger)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _logger = logger;

            // Configure retry policy
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Error during authentication (Attempt {RetryCount}). Retrying in {RetryTimeSpan}...",
                            retryCount, timeSpan);
                    });
        }

        public async Task<string> GetAccessTokenAsync()
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                // Check if we have a valid token
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryTime)
                {
                    _logger.LogDebug("Using existing access token");
                    return _accessToken;
                }

                // Get a new token
                _logger.LogInformation("Getting new access token");
                return await _retryPolicy.ExecuteAsync(async () => await FetchNewTokenAsync());
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        private async Task<string> FetchNewTokenAsync()
        {
            if (string.IsNullOrEmpty(_apiSettings.BaseUrl) ||
                string.IsNullOrEmpty(_apiSettings.AuthEndpoint) ||
                string.IsNullOrEmpty(_apiSettings.Username) ||
                string.IsNullOrEmpty(_apiSettings.Password))
            {
                throw new InvalidOperationException("API settings are not properly configured");
            }

            var request = new AuthRequest
            {
                Username = _apiSettings.Username,
                Password = _apiSettings.Password,
                SecurityCode = _apiSettings.SecurityCode ?? string.Empty
            };

            var url = $"{_apiSettings.BaseUrl.TrimEnd('/')}{_apiSettings.AuthEndpoint}";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);

            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token.Access_token))
            {
                throw new InvalidOperationException("Failed to get a valid access token");
            }

            _accessToken = authResponse.Token.Access_token;
            // Set expiry time with a small buffer (5 minutes)
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(authResponse.Token.Expires_in - 300);

            _logger.LogInformation("Successfully obtained new access token, valid until: {ExpiryTime}", _tokenExpiryTime);

            return _accessToken;
        }

        public void SetAuthorizationHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }
    }
}
