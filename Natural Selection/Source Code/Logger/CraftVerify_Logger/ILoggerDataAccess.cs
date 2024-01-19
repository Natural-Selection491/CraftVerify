public interface ILoggingDataAccess
{
    Task SaveLogAsync(LogEntry logEntry);
    Task ArchiveLogsAsync();
    Task<LogEntry> GetLastInsertedLogEntryAsync();

}