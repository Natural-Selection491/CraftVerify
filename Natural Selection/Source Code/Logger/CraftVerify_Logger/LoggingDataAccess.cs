using MySql.Data.MySqlClient;
using System.Threading.Tasks;

public class LoggingDataAccess : ILoggingDataAccess
{
    private readonly string _connectionString;

    public LoggingDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveLogAsync(LogEntry logEntry)
    {
        var query = "INSERT INTO LogEntries (UserHash, ActionType, LogTime, LogStatus, LogDetail) VALUES (@UserHash, @ActionType, @LogTime, @LogStatus, @LogDetail)";

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(query, connection))
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

    public async Task ArchiveLogsAsync()
    {
        var moveOldEntriesQuery = @"
        INSERT INTO ArchivedLogEntries (LogID, UserHash, ActionType, LogTime, LogStatus, LogDetail)
        SELECT LogID, UserHash, ActionType, LogTime, LogStatus, LogDetail
        FROM LogEntries
        WHERE LogTime < DATE_SUB(NOW(), INTERVAL 1 YEAR);

        DELETE FROM LogEntries WHERE LogTime < DATE_SUB(NOW(), INTERVAL 1 YEAR);";

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(moveOldEntriesQuery, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
