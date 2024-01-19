using Microsoft.Extensions.DependencyInjection;
using System;

class ServiceConfigurator
{
    static void Main(string[] args)
    {
        // Setup DI
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ILoggingDataAccess, LoggingDataAccess>(serviceProvider =>
                new LoggingDataAccess("Server=localhost;Database=master;User Id=Parth;Password=1762;TrustServerCertificate=true"))

            .AddSingleton<ILoggingDataAccess, LoggingDataAccess>()
            .AddSingleton<ILogger, Logger>()
            .BuildServiceProvider();

        // Retrieve the logger instance
        var logger = serviceProvider.GetService<ILogger>();
    }
}