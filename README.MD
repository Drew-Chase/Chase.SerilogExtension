# Chase Serilog Extension

This extension adds a `EnhancedWriteToFile` option to the Serilog logger to allow for additional options when logging to a file like automatically archiving old logs.

## Usage

To use this extension, add the following using statement to your file:

```csharp
using Chase.SerilogExtension;
```

Then, when initializing your logger, use the `EnhancedWriteToFile` method to specify the file path and options:

## EnhancedWriteToFile

```csharp
.EnhancedWriteToFile(
    directory: "logs", // The directory to write the log files to
    archiveOldLogs: true, // Whether or not to archive old logs
    flushToDiskInterval: TimeSpan.FromSeconds(30), // The interval to flush logs to disk, this will also set buffered to true
    maxLogSize: 10485760) // This will automatically archive the log file if it exceeds 10MB
```

## AutoCloseAndFlush

This extension also adds an `AutoCloseAndFlush` method to the logger configuration that will automatically close and flush the logger when the application exits. This is useful for console applications that may not have a clean exit.

```csharp
.AutoCloseAndFlush()
```

## Example

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .EnhancedWriteToFile(
        directory: "logs", // The directory to write the log files to
        archiveOldLogs: true, // Whether or not to archive old logs
        flushToDiskInterval: TimeSpan.FromSeconds(30), // The interval to flush logs to disk, this will also set buffered to true
        maxLogSize: 10485760) // This will automatically archive the log file if it exceeds 10MB

    .AutoCloseAndFlush() // This will automatically close and flush the logger when the application exits
    .CreateLogger();
```

