using Microsoft.Extensions.DependencyInjection;
using System;

class ServiceConfigurator
{
    static void Main(string[] args)
    {
        // Setup DI
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ILoggingDataAccess, LoggingDataAccess>(serviceProvider =>
                new LoggingDataAccess("Server=hostname;Port=port;Database=your_database;User=user;Password=password;"))

            .AddSingleton<ILoggingDataAccess, LoggingDataAccess>()
            .AddSingleton<ILogger, Logger>()
            .BuildServiceProvider();

        // Retrieve the logger instance
        var logger = serviceProvider.GetService<ILogger>();
    }
}