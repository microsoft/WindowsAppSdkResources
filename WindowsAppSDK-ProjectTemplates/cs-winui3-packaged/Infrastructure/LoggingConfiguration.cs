using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace BlankApp.Infrastructure;

/// <summary>
/// Configures logging with multiple destinations:
/// - File: All levels, with 4KB rotation
/// - Windows Event Log: Warning and above only
/// 
/// File locations:
/// - Debug: {ProjectFolder}/logs/
/// - Release: {AppData}/BlankApp/logs/
/// </summary>
public static class LoggingConfiguration
{
    private const int MaxLogFileSizeBytes = 4096; // 4KB
    private const int RetainedFileCount = 10;
    private const string AppName = "BlankApp";

    /// <summary>
    /// Configures logging services with file and Windows Event Log providers.
    /// </summary>
    public static IServiceCollection AddAppLogging(this IServiceCollection services)
    {
        // Determine log folder based on build configuration
        var logFolder = GetLogFolder();
        var logFilePath = Path.Combine(logFolder, "app-.log");

        // Ensure log directory exists
        Directory.CreateDirectory(logFolder);

        // Configure Serilog for file logging with rotation
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: MaxLogFileSizeBytes,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: RetainedFileCount,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // Configure Microsoft.Extensions.Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            
            // Set minimum level
            builder.SetMinimumLevel(LogLevel.Trace);

            // Add Serilog for file logging
            builder.AddSerilog(Log.Logger, dispose: true);

            // Add Windows Event Log for Warning and above
            builder.AddEventLog(new EventLogSettings
            {
                SourceName = AppName,
                LogName = "Application",
                Filter = (category, level) => level >= LogLevel.Warning
            });

#if DEBUG
            // Add Debug output in Debug builds
            builder.AddDebug();
#endif
        });

        return services;
    }

    /// <summary>
    /// Gets the log folder path based on build configuration.
    /// </summary>
    public static string GetLogFolder()
    {
#if DEBUG
        // Debug: Use project folder (or current directory)
        return Path.Combine(AppContext.BaseDirectory, "logs");
#else
        // Release: Use AppData folder
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, AppName, "logs");
#endif
    }

    /// <summary>
    /// Ensures Windows Event Log source exists (requires admin on first run).
    /// Call this during app installation or first run with elevated privileges.
    /// </summary>
    public static void EnsureEventLogSourceExists()
    {
        try
        {
            if (!EventLog.SourceExists(AppName))
            {
                EventLog.CreateEventSource(AppName, "Application");
            }
        }
        catch (System.Security.SecurityException)
        {
            // Requires admin privileges - will be created on first write if possible
            Debug.WriteLine($"Cannot create EventLog source '{AppName}' - requires admin privileges");
        }
    }
}
