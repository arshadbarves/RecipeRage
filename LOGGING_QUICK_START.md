# Logging System - Quick Start

## 🚀 Setup (2 minutes)

### Step 1: Add Debug Console to Scene
In Unity Editor:
1. Go to **Tools → Debug Console → Add to Scene**
2. Done! The console is now in your scene.

### Step 2: Test It
**In Editor:**
1. Press **Play**
2. Press **`** (backtick/tilde key) to open the console
3. You should see all Unity logs captured automatically

**On Mobile Device:**
1. Build with **Development Build** enabled
2. Deploy to device
3. Hold **3 fingers** for **1 second** to open console

### Step 3: Test Logging
In Unity Editor:
- Go to **Tools → Debug Console → Test Logging**
- Open the console in Play mode to see the test logs

### ⚠️ Important: Development Builds Only
The console is **automatically disabled** in release builds for performance and security.

## 📝 Basic Usage

### In Any Script
```csharp
using Core.Logging;

public class MyScript : MonoBehaviour
{
    void Start()
    {
        // Simple logging
        GameLogger.Log("Game started!");
        
        // Category-specific
        GameLogger.Network.Log("Connected to server");
        GameLogger.Audio.LogWarning("Volume low");
        GameLogger.Save.LogError("Save failed");
    }
}
```

### In Services (with DI)
```csharp
public class MyService
{
    private readonly ILoggingService _logger;

    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogInfo("Working...", "MyService");
    }
}
```

## 🎮 Console Controls

### Desktop/Editor
| Key | Action |
|-----|--------|
| **`** (backtick) | Toggle console |

### Mobile
| Gesture | Action |
|---------|--------|
| **3 fingers (1 sec)** | Toggle console |

### All Platforms
| Control | Action |
|---------|--------|
| Search field | Filter logs by text |
| Level dropdown | Filter by severity |
| Category dropdown | Filter by category |
| Clear button | Remove all logs |
| Export button | Save logs to file |
| Click/Tap log | Show/hide stack trace |

## 📦 What's Included

✅ **Automatic Unity Log Capture** - All Debug.Log calls are captured  
✅ **In-Game Console** - View logs on device during testing  
✅ **Export Functionality** - Save logs to file for bug reports  
✅ **Filtering** - Search, filter by level/category  
✅ **Bootstrap Integration** - Works with your existing service system  
✅ **Thread-Safe** - Safe to use from any thread  
✅ **Performance Optimized** - Minimal overhead  

## 🔧 Common Tasks

### Export Logs
```csharp
var logger = GameBootstrap.Services.LoggingService;
logger.SaveLogsToFile(Path.Combine(Application.persistentDataPath, "logs.txt"));
```

### Disable Verbose Logs
```csharp
var logger = GameBootstrap.Services.LoggingService;
logger.SetLogLevel(LogLevel.Warning); // Only warnings and above
```

### Disable Category
```csharp
var logger = GameBootstrap.Services.LoggingService;
logger.DisableCategory("Network"); // Mute network logs
```

## 📍 File Locations

- **Scripts**: `Assets/Scripts/Core/Logging/`
- **UI Component**: `Assets/Scripts/UI/DebugConsoleUI.cs`
- **Exported Logs**: `Application.persistentDataPath/GameLogs_[timestamp].txt`

## 🎯 Next Steps

1. Add `GameLogger` calls throughout your codebase
2. Create custom categories for your systems
3. Test the console on device builds
4. Set up log export for bug reports

For detailed documentation, see **LOGGING_SYSTEM_GUIDE.md**
