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
                foreach (var callLog in callLogs)
                {
                    // Check if the entry already exists
                    var existingEntry = await _dbContext.CallLogs.FindAsync(callLog.SegmentId);
                    if (existingEntry == null)
                    {
                        // Add new entry
                        await _dbContext.CallLogs.AddAsync(callLog);
                        _logger.LogInformation("Adding new call log entry with SegmentId: {SegmentId}", callLog.SegmentId);
                    }
                    else
                    {
                        _logger.LogDebug("Call log entry with SegmentId: {SegmentId} already exists", callLog.SegmentId);
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved {Count} call log entries to the database", callLogs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving call logs to the database");
                throw;
            }
        }
    }
}
