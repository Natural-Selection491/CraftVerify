public interface ILogger
{
    Task LogAsync(LogEntry entry);
    Task ArchiveLogsAsync();

    Task WaitForQueueToEmptyAsync(CancellationToken cancellationToken = default);
}