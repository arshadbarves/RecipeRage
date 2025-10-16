# Logging System Guide

## Overview
A powerful logging system integrated with the bootstrap architecture that captures Unity logs, provides filtering, and includes an in-game debug console using UI Toolkit.

## Features
- ✅ Captures all Unity Debug.Log calls automatically
- ✅ Custom log levels (Verbose, Info, Warning, Error, Critical)
- ✅ Category-based filtering
- ✅ In-game debug console with UI Toolkit
- ✅ Export logs to file
- ✅ Search and filter capabilities
- ✅ Stack trace capture for errors
- ✅ Thread-safe logging
- ✅ Configurable log retention (default 5000 entries)

## Setup

### 1. Add Debug Console to Scene
1. Create an empty GameObject in your scene
2. Add the `DebugConsoleUI` component
3. The console will automatically create its UI

### 2. Bootstrap Integration
The logging service is automatically initialized in `GameBootstrap.InitializeCore()` as the first service (no dependencies).

## Usage

### Basic Logging
```csharp
using Core.Logging;
using Core.Bootstrap;

// Direct service access
var logger = GameBootstrap.Services.LoggingService;
logger.LogInfo("Player connected");
logger.LogWarning("Low memory detected");
logger.LogError("Failed to load asset");
```

### Using GameLogger Helper (Recommended)
```csharp
using Core.Logging;

// General logging
GameLogger.Log("Game started");
GameLogger.LogWarning("Performance issue detected");
GameLogger.LogError("Critical error occurred");

// Category-specific logging
GameLogger.Network.Log("Connected to server");
GameLogger.Audio.LogWarning("Audio clip missing");
GameLogger.Save.LogError("Failed to save game");
GameLogger.Auth.Log("User authenticated");
GameLogger.UI.Log("Menu opened");
```

### Custom Categories
```csharp
GameLogger.LogInfo("Custom message", "MyCategory");
```

### Exception Logging
```csharp
try
{
    // Your code
}
catch (Exception ex)
{
    GameLogger.LogException(ex, "MySystem");
}
```

## Debug Console

### Opening the Console
- Press **`** (backtick/tilde key) to toggle the console
- Or call `debugConsoleUI.Show()` from code

### Console Features
- **Search**: Filter logs by text content
- **Level Filter**: Show only specific log levels
- **Category Filter**: Filter by category
- **Clear**: Remove all logs
- **Export**: Save logs to persistent data path
- **Click on log**: Show/hide stack trace (if available)

### Console Controls
```csharp
var console = FindObjectOfType<DebugConsoleUI>();
console.Show();    // Open console
console.Hide();    // Close console
console.Toggle();  // Toggle visibility
```

## Advanced Features

### Set Minimum Log Level
```csharp
// Only log warnings and above
logger.SetLogLevel(LogLevel.Warning);
```

### Disable/Enable Categories
```csharp
// Disable verbose network logs
logger.DisableCategory("Network");

// Re-enable later
logger.EnableCategory("Network");
```

### Export Logs Programmatically
```csharp
// Get logs as string
string logContent = logger.ExportLogs();

// Save to custom location
logger.SaveLogsToFile("/path/to/logs.txt");

// Default export location
string path = Path.Combine(Application.persistentDataPath, "logs.txt");
logger.SaveLogsToFile(path);
```

### Query Logs
```csharp
// Get all logs
LogEntry[] allLogs = logger.GetLogs();

// Get by level
LogEntry[] errors = logger.GetLogsByLevel(LogLevel.Error);

// Get by category
LogEntry[] networkLogs = logger.GetLogsByCategory("Network");
```

## Log Entry Structure
```csharp
public class LogEntry
{
    public string Message;        // Log message
    public LogLevel Level;        // Severity level
    public string Category;       // Category/system
    public string Timestamp;      // When it was logged
    public string StackTrace;     // Stack trace (if available)
    public int FrameCount;        // Unity frame number
}
```

## Integration Examples

### In Your Services
```csharp
public class MyService
{
    private readonly ILoggingService _logger;

    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.LogInfo("Doing something", "MyService");
    }
}
```

### In MonoBehaviours
```csharp
public class MyBehaviour : MonoBehaviour
{
    private void Start()
    {
        GameLogger.Log("MyBehaviour started");
    }

    private void OnError()
    {
        GameLogger.LogError("Something went wrong", "MyBehaviour");
    }
}
```

## Performance Considerations
- Logs are stored in memory (default: 5000 entries)
- Old logs are automatically removed when limit is reached
- Thread-safe for multi-threaded logging
- Minimal performance impact when console is hidden
- Unity logs are captured automatically (no performance overhead)

## Customization

### Change Max Log Entries
```csharp
// In GameBootstrap.InitializeCore()
_services.RegisterLoggingService(new LoggingService(maxLogEntries: 10000));
```

### Change Toggle Key
```csharp
// In DebugConsoleUI inspector
[SerializeField] private KeyCode _toggleKey = KeyCode.F1;
```

### Custom Log Categories
Create your own category helpers in `GameLogger`:
```csharp
public static class Gameplay
{
    public static void Log(string message) => LogInfo(message, "Gameplay");
    public static void LogWarning(string message) => GameLogger.LogWarning(message, "Gameplay");
    public static void LogError(string message) => GameLogger.LogError(message, "Gameplay");
}
```

## Troubleshooting

### Console Not Showing
- Ensure `DebugConsoleUI` component is in the scene
- Check that `UIDocument` component is attached
- Verify logging service is initialized in bootstrap

### Logs Not Appearing
- Check if category is disabled
- Verify log level filter
- Ensure bootstrap is initialized

### Export Not Working
- Check write permissions for persistent data path
- Verify path exists and is writable
- Check console for error messages

## File Locations
- **Exported Logs**: `Application.persistentDataPath/GameLogs_[timestamp].txt`
- **Scripts**: `Assets/Scripts/Core/Logging/`
- **UI**: `Assets/Scripts/UI/DebugConsoleUI.cs`

## Best Practices
1. Use category-specific loggers for better organization
2. Use appropriate log levels (don't log everything as Error)
3. Include context in log messages
4. Use exceptions for critical errors
5. Disable verbose categories in production builds
6. Export logs when users report issues
7. Clear logs periodically in long-running sessions
