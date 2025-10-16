# Core Logging System

## Architecture

```
LoggingService (ILoggingService)
    ├── Captures Unity Debug.Log calls
    ├── Stores logs in memory (thread-safe)
    ├── Provides filtering and export
    └── Integrated with Bootstrap system

DebugConsoleUI (MonoBehaviour)
    ├── UI Toolkit-based in-game console
    ├── Real-time log display
    ├── Search and filter capabilities
    └── Export functionality

GameLogger (Static Helper)
    ├── Easy access from anywhere
    ├── Category-specific helpers
    └── No need to inject service
```

## Files

- **LogLevel.cs** - Log severity enum
- **LogEntry.cs** - Log entry data structure
- **ILoggingService.cs** - Service interface
- **LoggingService.cs** - Core logging implementation
- **GameLogger.cs** - Static helper for easy access

## Integration with Bootstrap

The logging service is initialized first in `GameBootstrap.InitializeCore()`:

```csharp
private void InitializeCore()
{
    // Logging service first (no dependencies)
    _services.RegisterLoggingService(new LoggingService(maxLogEntries: 5000));
    
    // Other services...
}
```

This ensures all other services can use logging during their initialization.

## Usage Patterns

### Pattern 1: Static Helper (Recommended for MonoBehaviours)
```csharp
GameLogger.Log("Message");
GameLogger.Network.Log("Network message");
```

### Pattern 2: Dependency Injection (Recommended for Services)
```csharp
public class MyService
{
    private readonly ILoggingService _logger;
    
    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }
}
```

### Pattern 3: Direct Access
```csharp
var logger = GameBootstrap.Services.LoggingService;
logger.LogInfo("Message");
```

## Thread Safety

The logging service is thread-safe and can be called from any thread:

```csharp
Task.Run(() => 
{
    GameLogger.Log("From background thread");
});
```

## Performance

- **Memory**: ~5000 log entries by default (~1-2 MB)
- **CPU**: Minimal overhead when console is hidden
- **Thread-safe**: Lock-based synchronization
- **Auto-cleanup**: Old logs removed when limit reached

## Extending

### Add Custom Categories
```csharp
public static class GameLogger
{
    public static class MyCategory
    {
        public static void Log(string message) => LogInfo(message, "MyCategory");
        public static void LogWarning(string message) => GameLogger.LogWarning(message, "MyCategory");
        public static void LogError(string message) => GameLogger.LogError(message, "MyCategory");
    }
}
```

### Custom Log Handlers
```csharp
var logger = GameBootstrap.Services.LoggingService;
logger.OnLogAdded += (entry) => 
{
    // Custom handling (e.g., send to analytics)
};
```
