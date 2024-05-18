using System.IO.Compression;
using Serilog;
using Serilog.Events;

namespace Chase.SerilogExtension;

/// <summary>
/// Provides extension methods for the <see cref="LoggerConfiguration"/> class.
/// </summary>
public static class Extension
{
    /// <summary>
    /// Enhances the configuration to write log events to files.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="directory">The directory where log files will be stored.</param>
    /// <param name="flushToDiskInterval">The interval at which log events are flushed to disk. If null, log events are not buffered.</param>
    /// <param name="archiveOldLogs">Indicates whether old log files should be archived.</param>
    /// <param name="maxLogSize">The maximum size (in bytes) of a log file before it is archived. Set to 0 to disable.</param>
    /// <returns>The updated logger configuration.</returns>
    public static LoggerConfiguration EnhancedWriteToFile(this LoggerConfiguration loggerConfiguration, string directory, TimeSpan? flushToDiskInterval = null, bool archiveOldLogs = true, long maxLogSize = 10485760 /* 10MB */) => EnhancedWriteToFileAsync(loggerConfiguration, directory, flushToDiskInterval, archiveOldLogs, maxLogSize).Result;

    /// <summary>
    /// Enhances the configuration to write log events to files asynchronously.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="directory">The directory where log files will be stored.</param>
    /// <param name="flushToDiskInterval">The interval at which log events are flushed to disk. If null, log events are not buffered.</param>
    /// <param name="archiveOldLogs">Indicates whether old log files should be archived.</param>
    /// <param name="maxLogSize">The maximum size (in bytes) of a log file before it is archived. Set to 0 to disable.</param>
    /// <returns>A Task representing the asynchronous operation that returns the updated logger configuration.</returns>
    public static async Task<LoggerConfiguration> EnhancedWriteToFileAsync(this LoggerConfiguration loggerConfiguration, string directory, TimeSpan? flushToDiskInterval = null, bool archiveOldLogs = true, long maxLogSize = 10485760 /* 10MB */)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        if (archiveOldLogs)
        {
            await ArchiveOldLogs(directory);
        }

        if (flushToDiskInterval.HasValue)
        {
            loggerConfiguration.WriteTo.File(Path.Combine(directory, "debug.log"), buffered: true, flushToDiskInterval: flushToDiskInterval.Value, restrictedToMinimumLevel: LogEventLevel.Verbose);
            loggerConfiguration.WriteTo.File(Path.Combine(directory, "latest.log"), buffered: true, flushToDiskInterval: flushToDiskInterval.Value, restrictedToMinimumLevel: LogEventLevel.Information);
            loggerConfiguration.WriteTo.File(Path.Combine(directory, "error.log"), buffered: true, flushToDiskInterval: flushToDiskInterval.Value, restrictedToMinimumLevel: LogEventLevel.Error);
        }
        else
        {
            loggerConfiguration.WriteTo.File(Path.Combine(directory, "debug.log"), buffered: false, restrictedToMinimumLevel: LogEventLevel.Verbose);
            loggerConfiguration.WriteTo.File(Path.Combine(directory, "latest.log"), buffered: false, restrictedToMinimumLevel: LogEventLevel.Information);
            loggerConfiguration.WriteTo.File(Path.Combine(directory, "error.log"), buffered: false, restrictedToMinimumLevel: LogEventLevel.Error);
        }

        if (maxLogSize <= 0) return loggerConfiguration;
        FileSystemWatcher watchers = new FileSystemWatcher(directory, "*.log");
        watchers.Changed += async (sender, e) =>
        {
            if (new FileInfo(e.FullPath).Length > maxLogSize)
            {
                await ArchiveOldLogs(directory);
            }
        };

        return loggerConfiguration;
    }

    /// <summary>
    /// Enhances the configuration to automatically close and flush the log on application exit.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <returns>The updated logger configuration.</returns>
    public static LoggerConfiguration AutoCloseAndFlush(this LoggerConfiguration loggerConfiguration)
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => { Log.CloseAndFlush(); };

        return loggerConfiguration;
    }

    /// <summary>
    /// Archives the old log files in the specified directory.
    /// </summary>
    /// <param name="directory">The directory where log files are stored.</param>
    /// <param name="attempt">The number of attempt to archive log files.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task ArchiveOldLogs(string directory, int attempt = 0)
    {
        string[] logs = Directory.GetFiles(directory, "*.log");
        if (logs.Length == 0) return;
        using ZipArchive archive = ZipFile.Open(Path.Combine(directory, $"logs-{DateTime.Now:MM-dd-yyyy-HH-mm-ss.ffff}.zip"), ZipArchiveMode.Create);
        foreach (string log in logs)
        {
            try
            {
                archive.CreateEntryFromFile(log, Path.GetFileName(log));
                File.Delete(log);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to archive log file {log}", log);
                if (attempt < 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await ArchiveOldLogs(directory, attempt + 1);
                }
            }
        }
    }
}