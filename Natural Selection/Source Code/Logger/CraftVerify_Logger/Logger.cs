using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;

public class Logger : ILogger
{
    private readonly ILoggingDataAccess _loggingDataAccess;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly string _connectionString = "Server=localhost;Database=master;User Id=Parth;Password=1762;TrustServerCertificate=true";
    private int _pendingOperations = 0; // To keep track of pending log operations

    public Logger(ILoggingDataAccess loggingDataAccess)
    {
        _loggingDataAccess = loggingDataAccess;
        Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
    }

    public Task LogAsync(LogEntry entry)
    {
        Interlocked.Increment(ref _pendingOperations); // Increment for each enqueued log entry
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
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("INSERT INTO LogTable (UserHash, ActionType, LogTime, LogStatus, LogDetail) VALUES (@userHash, @actionType, @logTime, @logStatus, @logDetail)", connection);
                        command.Parameters.AddWithValue("@userHash", logEntry.UserHash);
                        command.Parameters.AddWithValue("@actionType", logEntry.ActionType);
                        command.Parameters.AddWithValue("@logTime", logEntry.LogTime);
                        command.Parameters.AddWithValue("@logStatus", logEntry.LogStatus);
                        command.Parameters.AddWithValue("@logDetail", logEntry.LogDetail);
                        await command.ExecuteNonQueryAsync();
                        Interlocked.Decrement(ref _pendingOperations); // Decrement after processing
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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

    public async Task WaitForQueueToEmptyAsync(CancellationToken cancellationToken = default)
    {
        while (!_logQueue.IsEmpty || _pendingOperations > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            await Task.Delay(100);
        }
    }
}