using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

namespace LoggingLib.Tests
{
    [TestClass]
    public class LoggerTests
    {
        private Logger _logger;
        private InMemoryLoggingDataAccess _dataAccess;

        [TestInitialize]
        public void Initialize()
        {
            _dataAccess = new InMemoryLoggingDataAccess();
            _logger = new Logger(_dataAccess);
        }

        [TestMethod]
        public async Task LogAsync_OperationCompletesWithinThreeSeconds()
        {
            var logEntry = new LogEntry("UserHash", "Info", "TestCategory", "Test log message");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _logger.LogAsync(logEntry);

            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000, "Logging operation took longer than 3 seconds.");
        }

        [TestMethod]
        public async Task LogAsync_ShouldSaveLogEntry()
        {
            var logEntry = new LogEntry("UserHash", "Info", "TestCategory", "Test log message");

            await _logger.LogAsync(logEntry);

            // Allow time for the log entry to be processed
            await Task.Delay(500); // Adjust as necessary

            // Assert
            bool logEntryExists = _dataAccess.LogEntries.Contains(logEntry);
            Assert.IsTrue(logEntryExists, "Log entry was not saved correctly.");
        }


        [TestMethod]
        public async Task LogAsync_ShouldBeThreadSafe()
        {
            var tasks = new List<Task>();
            int numberOfParallelLogs = 100;

            for (int i = 0; i < numberOfParallelLogs; i++)
            {
                var logEntry = new LogEntry("UserHash", "Debug", "TestCategory", $"Parallel log {i}");
                tasks.Add(_logger.LogAsync(logEntry));
            }

            await Task.WhenAll(tasks);

            // Allow time for all log entries to be processed
            await Task.Delay(1000); // Adjust the delay as needed

            // Assert
            Assert.AreEqual(numberOfParallelLogs, _dataAccess.LogEntries.Count, "Not all log entries were saved.");
        }


        [TestMethod]
        public async Task LogAsync_ShouldLogWithUTCTimestamp()
        {
            var logEntry = new LogEntry("UserHash", "Debug", "TestCategory", "Test UTC timestamp");

            await _logger.LogAsync(logEntry);

            // Allow time for the log entry to be processed
            await Task.Delay(500); // Adjust as necessary

            var savedLog = _dataAccess.LogEntries.LastOrDefault();
            Assert.IsNotNull(savedLog, "Log entry was not found.");
            Assert.IsTrue(savedLog.LogTime.ToUniversalTime() == savedLog.LogTime, "Log time is not UTC.");
        }


        // Additional test methods...

        private class InMemoryLoggingDataAccess : ILoggingDataAccess
        {
            public List<LogEntry> LogEntries { get; } = new List<LogEntry>();
            public List<LogEntry> ArchivedLogEntries { get; } = new List<LogEntry>();

            public Task SaveLogAsync(LogEntry logEntry)
            {
                LogEntries.Add(logEntry);
                return Task.CompletedTask;
            }

            public Task ArchiveLogsAsync()
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var entriesToArchive = LogEntries.Where(entry => entry.LogTime < cutoffDate).ToList();

                foreach (var entry in entriesToArchive)
                {
                    LogEntries.Remove(entry);
                    ArchivedLogEntries.Add(entry);
                }

                return Task.CompletedTask;
            }
        }

    }
}
