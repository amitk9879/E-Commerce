using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace SharedKernel.Logging
{
    public static class LoggingExtensions
    {
        public static void AddSharedSerilogLogging(this IHostApplicationBuilder builder, string serviceName)
        {
            var logsPath = GetLogsDirectory(serviceName);
            var logPath = Path.Combine(logsPath, "log-.txt");

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine}{Exception}"
                );

            Log.Logger = loggerConfig.CreateLogger();

            builder.Services.AddSerilog();
        }

        private static string GetLogsDirectory(string serviceName)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "AirmasterCommerce.slnx")))
            {
                directory = directory.Parent;
            }

            var rootPath = directory != null ? directory.FullName : AppContext.BaseDirectory;
            var logsPath = Path.Combine(rootPath, "logs", serviceName);

            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            return logsPath;
        }
    }
}
