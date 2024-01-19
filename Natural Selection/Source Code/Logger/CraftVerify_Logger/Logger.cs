using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class Logger : ILogger
{
    private readonly ILoggingDataAccess _loggingDataAccess;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public Logger(ILoggingDataAccess loggingDataAccess)
    {
        _loggingDataAccess = loggingDataAccess;
        Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
    }

    public Task LogAsync(LogEntry entry)
    {
        _logQueue.Enqueue(entry);
        return Task.CompletedTask;
    }

    private async Task ProcessLogQueue(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_logQueue.TryDequeue(out LogEntry logEntry) && logEntry != null)
            {
                await _semaphore.WaitAsync();
                try
                {
                    await _loggingDataAccess.SaveLogAsync(logEntry);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            else
            {
                // If the queue is empty, wait a bit before trying again to avoid spinning.
                await Task.Delay(100);
            }
        }
    }

    public async Task ArchiveLogsAsync()
    {
        await _loggingDataAccess.ArchiveLogsAsync();
    }

    public void StopLogging()
    {
        _cancellationTokenSource.Cancel();
    }
}
