using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LoggerTests
{
    private ILogger _logger;
    private ILoggingDataAccess _loggingDataAccess;

    [TestInitialize]
    public void Initialize()
    {
        // Initialize the Logger and LoggingDataAccess with actual database connection
        // Update the connection string as per your test database
        var connectionString = "Server=localhost;Database=master;User Id=Parth;Password=1762;TrustServerCertificate=true";
        _loggingDataAccess = new LoggingDataAccess(connectionString);
        _logger = new Logger(_loggingDataAccess);
    }

    [TestMethod]
    public async Task LogAsync_OperationCompletesWithinThreeSeconds()
    {
        var logEntry = new LogEntry("UserHash", "ActionType", "LogStatus", "LogDetail");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _logger.LogAsync(logEntry);
        stopwatch.Stop();

        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000, "Logging operation took longer than 3 seconds.");
    }

    [TestMethod]
    public async Task LogAsync_ShouldSaveLogEntry()
    {
        var logEntry = new LogEntry("UserHash", "ActionType", "LogStatus", "LogDetail");
        await _logger.LogAsync(logEntry);

        // Wait for the queue to be processed
        await _logger.WaitForQueueToEmptyAsync();

        var lastEntry = await _loggingDataAccess.GetLastInsertedLogEntryAsync();
        Assert.IsNotNull(lastEntry, "No log entry was retrieved from the database.");

        Assert.AreEqual(logEntry.UserHash.Trim(), lastEntry.UserHash.Trim());
        Assert.AreEqual(logEntry.ActionType, lastEntry.ActionType);
        Assert.AreEqual(logEntry.LogStatus, lastEntry.LogStatus);
        Assert.AreEqual(logEntry.LogDetail, lastEntry.LogDetail);
    }

    [TestMethod]
    public async Task LogAsync_ShouldBeThreadSafe()
    {
        int numberOfThreads = 10;
        var tasks = new Task[numberOfThreads];
        for (int i = 0; i < numberOfThreads; i++)
        {
            tasks[i] = Task.Run(() => _logger.LogAsync(new LogEntry("UserHash", "ActionType", "LogStatus", "LogDetail")));
        }

        await Task.WhenAll(tasks);
    }

    [TestMethod]
    public async Task LogAsync_ShouldLogWithUTCTimestamp()
    {
        var logEntry = new LogEntry("UserHash", "ActionType", "LogStatus", "LogDetail");
        var entryTime = DateTime.UtcNow;
        await _logger.LogAsync(logEntry);

        // Wait for the queue to be processed
        await _logger.WaitForQueueToEmptyAsync();

        var lastEntry = await _loggingDataAccess.GetLastInsertedLogEntryAsync();
        var timeDifference = (lastEntry.LogTime - entryTime).Duration();

        Assert.IsTrue(timeDifference < TimeSpan.FromSeconds(2), "The timestamp is not within the expected range of UTC time.");
    }
}
