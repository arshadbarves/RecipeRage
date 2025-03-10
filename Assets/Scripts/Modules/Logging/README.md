# Logging System

A modular, flexible logging system for RecipeRage that provides consistent logging across all modules.

## Overview

The Logging system provides a centralized way to log messages, warnings, errors, and exceptions throughout the application. It supports different output destinations and log levels, allowing for flexible logging configurations.

Key features:
- Multiple log levels (Debug, Info, Warning, Error)
- Multiple output destinations (Console, File)
- Structured logging with contextual information
- Timestamp and module tagging
- Thread-safe logging
- Event-based notifications
- Log filtering by level
- Historical log access

## Architecture

The Logging system follows a modular design with clear separation of concerns:

```
Assets/Scripts/Modules/Logging/
│
├── Interfaces/           # Interface definitions
│   └── ILogService.cs    # Main logging interface
│
├── Core/                 # Core implementations
│   └── LogService.cs     # Default log service implementation
│
└── LogHelper.cs          # Static helper for easy access
```

## Getting Started

### Initialization

Initialize the logging system early in your application:

```csharp
// Initialize logging with default settings
LogHelper.SetConsoleOutput(true);
LogHelper.SetFileOutput(true);
LogHelper.SetLogLevel(LogLevel.Debug);
```

### Basic Usage

Use the LogHelper to log messages:

```csharp
// Log messages with different levels
LogHelper.Debug("MyModule", "Debug information");
LogHelper.Info("MyModule", "Information message");
LogHelper.Warning("MyModule", "Warning message");
LogHelper.Error("MyModule", "Error message");

// Log exceptions
try
{
    // Some code that might throw
    int x = 1 / 0;
}
catch (Exception ex)
{
    LogHelper.Exception("MyModule", ex, "Division by zero");
}
```

### Log Levels

The logging system supports four log levels:

- **Debug** - Detailed information, typically useful only for debugging.
- **Info** - General information about system operation.
- **Warning** - Potentially harmful situations that might lead to errors.
- **Error** - Errors and exceptions that might cause features to malfunction.

You can set the minimum log level to display:

```csharp
// Only show warnings and errors
LogHelper.SetLogLevel(LogLevel.Warning);
```

### Output Destinations

Logs can be sent to multiple destinations:

```csharp
// Enable console output
LogHelper.SetConsoleOutput(true);

// Enable file output with custom path
LogHelper.SetFileOutput(true, Application.persistentDataPath + "/Logs/custom.log");
```

### Event-Based Notifications

Subscribe to log events:

```csharp
// Subscribe to log events
LogHelper.OnLogWritten += (logMessage) =>
{
    // Do something with the log message
    if (logMessage.Level == LogLevel.Error)
    {
        SendNotification("An error occurred: " + logMessage.Message);
    }
};
```

### Accessing Log History

Retrieve recent logs:

```csharp
// Get the 50 most recent logs
LogMessage[] recentLogs = LogHelper.GetRecentLogs(50);

// Display logs
foreach (var log in recentLogs)
{
    Debug.Log($"[{log.Timestamp}] [{log.Level}] [{log.Module}] {log.Message}");
}
```

## Advanced Usage

### Custom Log Service

You can implement your own log service by implementing the `ILogService` interface and setting it as the active service:

```csharp
// Create custom log service
MyCustomLogService customLogService = new MyCustomLogService();

// Set as the active service
LogHelper.SetLogService(customLogService);
```

### Contextual Logging

Use module names to provide context for your logs:

```csharp
// Log with module context
LogHelper.Info("AuthSystem", "User authenticated successfully");
LogHelper.Info("GameState", "Level 5 started");
```

### Structured Logging

When logging complex information, consider serializing it to a structured format:

```csharp
// Log structured data
var playerData = new { PlayerId = 12345, Level = 5, Score = 100 };
LogHelper.Info("PlayerSystem", $"Player data: {JsonUtility.ToJson(playerData)}");
```

## Best Practices

1. **Use appropriate log levels**
   - Debug: Detailed debugging information
   - Info: General operational information
   - Warning: Potential issues that aren't errors
   - Error: Actual errors that affect functionality

2. **Always include module names**
   - Makes it easier to filter and find relevant logs
   - Provides context for the log message

3. **Be concise but informative**
   - Include relevant data in logs
   - Avoid overly verbose messages

4. **Include actionable information in error logs**
   - What went wrong
   - Why it went wrong
   - How to fix it (if possible)

5. **Avoid logging sensitive information**
   - Never log passwords, tokens, or personal data
   - Be careful with user identifiers

6. **Use structured logging for complex data**
   - JSON or other structured formats
   - Makes logs easier to parse and analyze

7. **Set appropriate log levels for different environments**
   - Development: Debug or Info
   - Testing: Info or Warning
   - Production: Warning or Error

## Integration with Unity

The logging system integrates with Unity's built-in logging system:

```csharp
// Unity's logs will be captured by the logging system
Debug.Log("This will be captured");
Debug.LogWarning("This will be captured as a warning");
Debug.LogError("This will be captured as an error");
```

## Thread Safety

The logging system is thread-safe and can be used from multiple threads.

## Performance Considerations

- Log messages are buffered and written asynchronously to avoid performance impact
- Logs that are filtered out by level have minimal overhead
- File logging is optimized for performance

## License

Copyright © 2024 RecipeRage. All rights reserved. 