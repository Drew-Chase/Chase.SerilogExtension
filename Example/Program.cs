using Chase.SerilogExtension;
using Serilog;

namespace Example;

internal static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .EnhancedWriteToFile(
                directory: "logs", // The directory to write the log files to
                archiveOldLogs: true, // Whether to archive old logs
                flushToDiskInterval: TimeSpan.FromSeconds(30), // The interval to flush logs to disk, this will also set buffered to true
                maxLogSize: 10485760) // This will automatically archive the log file if it exceeds 10MB
            .AutoCloseAndFlush() // This will automatically close and flush the logger when the application exits
            .CreateLogger();

        // Writes to debug.log
        Log.Verbose("hello world");
        Log.Debug("hello world");

        // Writes to latest.log
        Log.Information("hello world");
        Log.Warning("hello world");

        // Writes to error.log
        Log.Error("hello world");
        Log.Fatal("hello world");
    }
}