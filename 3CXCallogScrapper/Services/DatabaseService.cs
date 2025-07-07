using _3CXCallogScrapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3CXCallogScrapper.Services
{
    public class DatabaseService
    {
        private readonly CallLogDbContext _dbContext;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(CallLogDbContext dbContext, ILogger<DatabaseService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            try
            {
                var connectionString = _dbContext.Database.GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Connection string is null or empty. Please check your configuration.");
                    throw new InvalidOperationException("Connection string is null or empty");
                }

                _logger.LogInformation("Using connection string: {ConnectionString}",
                    connectionString.Replace("Password=", "Password=********"));
                // Najpierw spróbujmy utworzyć bazę danych, jeśli nie istnieje
                try
                {
                    // Pobierz oryginalny connection string
                    var npgsqlConnectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

                    // Zapisz nazwę bazy danych
                    var databaseName = npgsqlConnectionStringBuilder.Database;

                    // Zmień connection string, aby łączyć się z bazą postgres (systemową)
                    npgsqlConnectionStringBuilder.Database = "postgres";

                    // Utwórz nowe połączenie do bazy postgres
                    using var masterConnection = new Npgsql.NpgsqlConnection(npgsqlConnectionStringBuilder.ConnectionString);
                    await masterConnection.OpenAsync();

                    // Sprawdź, czy baza danych istnieje
                    using var checkCmd = new Npgsql.NpgsqlCommand(
                        $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", masterConnection);
                    var exists = await checkCmd.ExecuteScalarAsync();

                    if (exists == null)
                    {
                        _logger.LogInformation("Database {DatabaseName} does not exist. Creating...", databaseName);

                        // Utwórz bazę danych
                        using var createCmd = new Npgsql.NpgsqlCommand(
                            $"CREATE DATABASE \"{databaseName}\"", masterConnection);
                        await createCmd.ExecuteNonQueryAsync();

                        _logger.LogInformation("Database {DatabaseName} created successfully", databaseName);
                    }
                    else
                    {
                        _logger.LogInformation("Database {DatabaseName} already exists", databaseName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create database. Will try to continue with migrations anyway.");
                }

                await _dbContext.Database.MigrateAsync();
                _logger.LogInformation("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while migrating the database");
                throw;
            }
        }

        public async Task SaveCallLogsAsync(List<CallLogEntry> callLogs)
        {
            try
            {
                if (!callLogs.Any())
                {
                    _logger.LogInformation("No call logs to save");
                    return;
                }
                int newEntries = 0;
                int duplicates = 0;

                // Wyszukaj i zaloguj duplikaty po SegmentId w dostarczonej liście
                var duplicateGroups = callLogs
                    .GroupBy(c => c.SegmentId)
                    .Where(g => g.Count() > 1)
                    .ToList();
                foreach (var group in duplicateGroups)
                {
                    _logger.LogWarning("Znaleziono duplikaty SegmentId={SegmentId}: {Count} wystąpień. Przykład: {Example}",
                        group.Key, group.Count(), System.Text.Json.JsonSerializer.Serialize(group.First()));
                }

                // USUŃ duplikaty z listy callLogs po SegmentId
                callLogs = callLogs
                    .GroupBy(c => c.SegmentId)
                    .Select(g => g.First())
                    .ToList();

                // Znajdź zakres czasowy przychodzących wpisów
                var minStartTime = callLogs.Min(c => c.StartTime);
                var maxStartTime = callLogs.Max(c => c.StartTime);

                // Dodaj 1 dzień na wszelki wypadek (aby uwzględnić strefy czasowe i inne kwestie)
                var searchStartTime = minStartTime.AddDays(-1);
                var searchEndTime = maxStartTime.AddDays(1);

                _logger.LogInformation("Searching for duplicates in time range: {StartTime} to {EndTime}",
                    searchStartTime, searchEndTime);

                // Pobierz listę wszystkich SegmentId, które już istnieją w bazie w danym przedziale czasowym
                var existingSegmentIds = await _dbContext.CallLogs
                    .Where(c => c.StartTime >= searchStartTime && c.StartTime <= searchEndTime)
                    .Where(c => callLogs.Select(l => l.SegmentId).Contains(c.SegmentId))
                    .Select(c => c.SegmentId)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} existing entries in database that match incoming data", existingSegmentIds.Count);

                foreach (var callLog in callLogs)
                {
                    // Sprawdź czy wpis już istnieje
                    if (existingSegmentIds.Contains(callLog.SegmentId))
                    {
                        _logger.LogDebug("Call log entry with SegmentId: {SegmentId} already exists, skipping", callLog.SegmentId);
                        duplicates++;
                        continue;
                    }

                    // Dodaj nowy wpis
                    await _dbContext.CallLogs.AddAsync(callLog);
                    _logger.LogDebug("Adding new call log entry with SegmentId: {SegmentId}", callLog.SegmentId);
                    newEntries++;
                }

                // Zapisz zmiany tylko jeśli są nowe wpisy
                if (newEntries > 0)
                {
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Saved {Count} new call log entries to the database ({Duplicates} duplicates skipped)",
                        newEntries, duplicates);
                }
                else
                {
                    _logger.LogInformation("No new call log entries to save ({Duplicates} duplicates skipped)", duplicates);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving call logs to the database");
                throw;
            }
        }
    }
}
