using System.Data.SqlClient; // Replaced MySQL client with SQL Server client
using System.Threading.Tasks;
using System.Collections.Generic;

public class LoggingDataAccess : ILoggingDataAccess
{
    private readonly string _connectionString;

    public LoggingDataAccess(string connectionString)
    {
        // Example SQL Server connection string (update with your server details)
        _connectionString = "Server=localhost;Database=master;User Id=Parth;Password=1762;TrustServerCertificate=true";
    }

    public async Task SaveLogAsync(LogEntry logEntry)
    {
        var query = "INSERT INTO LogTable (UserHash, ActionType, LogTime, LogStatus, LogDetail) VALUES (@UserHash, @ActionType, @LogTime, @LogStatus, @LogDetail)";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserHash", logEntry.UserHash);
                command.Parameters.AddWithValue("@ActionType", logEntry.ActionType);
                command.Parameters.AddWithValue("@LogTime", logEntry.LogTime);
                command.Parameters.AddWithValue("@LogStatus", logEntry.LogStatus);
                command.Parameters.AddWithValue("@LogDetail", logEntry.LogDetail);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<LogEntry> GetLastInsertedLogEntryAsync()
    {
        var query = "SELECT TOP 1 * FROM LogTable ORDER BY LogID DESC"; // Adjust the query as per your table schema

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new LogEntry(
                            reader.GetString(reader.GetOrdinal("UserHash")),
                            reader.GetString(reader.GetOrdinal("ActionType")),
                            reader.GetString(reader.GetOrdinal("LogStatus")),
                            reader.GetString(reader.GetOrdinal("LogDetail"))
                        ) 
                        {
                            LogID = reader.GetInt64(reader.GetOrdinal("LogID")),
                            LogTime = reader.GetDateTime(reader.GetOrdinal("LogTime"))
                        };
                    }
                }
            }
        }

        return null; // or handle as appropriate
    }
    public async Task ArchiveLogsAsync()
    {
        // Example of archiving logs (update query as per your requirement)
        var moveOldEntriesQuery = @"
            INSERT INTO ArchivedLogEntries (LogID, UserHash, ActionType, LogTime, LogStatus, LogDetail)
            SELECT LogID, UserHash, ActionType, LogTime, LogStatus, LogDetail
            FROM LogTable
            WHERE LogTime < DATEADD(YEAR, -1, GETDATE());

            DELETE FROM LogTable WHERE LogTime < DATEADD(YEAR, -1, GETDATE());";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(moveOldEntriesQuery, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}