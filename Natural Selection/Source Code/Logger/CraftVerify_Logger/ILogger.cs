public interface ILogger
{
    Task LogAsync(LogEntry entry);
    Task ArchiveLogsAsync();
}