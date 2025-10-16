# Logging System - Visual Guide

## 🎨 What It Looks Like

### In-Game Debug Console
```
┌─────────────────────────────────────────────────────────────────┐
│ Debug Console                                              ✕    │
├─────────────────────────────────────────────────────────────────┤
│ Search: [____________] Level: [All ▼] Category: [All ▼]        │
│ [Clear] [Export]                                                │
│ Showing 156 of 156 logs                                         │
├─────────────────────────────────────────────────────────────────┤
│ 2025-10-16 14:23:45.123 [Info]    [General]    Game started    │
│ 2025-10-16 14:23:46.456 [Info]    [Network]    Connected       │
│ 2025-10-16 14:23:47.789 [Warning] [Audio]      Volume low      │
│ 2025-10-16 14:23:48.012 [Error]   [SaveSystem] Save failed     │
│ 2025-10-16 14:23:49.345 [Info]    [UI]         Menu opened     │
│                                                                  │
│ (Click on any log to show/hide stack trace)                    │
└─────────────────────────────────────────────────────────────────┘
```

## 📊 Log Levels (Color Coded)

```
Verbose  → Gray   → Detailed debug information
Info     → White  → General information
Warning  → Yellow → Potential issues
Error    → Orange → Errors that need attention
Critical → Red    → Critical failures
```

## 🎯 Usage Patterns

### Pattern 1: Quick Logging
```csharp
GameLogger.Log("Quick message");
```
**When to use**: Simple logging without categories

### Pattern 2: Category Logging
```csharp
GameLogger.Network.Log("Connected");
GameLogger.Audio.LogWarning("Missing clip");
GameLogger.Save.LogError("Save failed");
```
**When to use**: Organized logging by system

### Pattern 3: Custom Category
```csharp
GameLogger.LogInfo("Player spawned", "Gameplay");
```
**When to use**: Custom categories not in GameLogger

### Pattern 4: Service Injection
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
        _logger.LogInfo("Working", "MyService");
    }
}
```
**When to use**: In services with dependency injection

## 🔄 Workflow

### Development Workflow
```
1. Write code with GameLogger calls
   ↓
2. Run game in editor
   ↓
3. Press ` to open console
   ↓
4. Filter/search logs
   ↓
5. Fix issues
   ↓
6. Repeat
```

### Bug Report Workflow
```
1. User encounters bug
   ↓
2. Press ` to open console
   ↓
3. Click "Export" button
   ↓
4. Send exported file with bug report
   ↓
5. Developer analyzes logs
```

## 📱 On-Device Testing

### Mobile
```
1. Build to device
2. Tap screen with 3 fingers (or custom gesture)
3. Console appears
4. Export logs via share sheet
```

### Console/PC
```
1. Build to platform
2. Press ` (backtick) key
3. Console appears
4. Export to persistent data path
```

## 🎮 Console Controls

### Keyboard Shortcuts
```
`           → Toggle console
Escape      → Close console (optional)
Ctrl+F      → Focus search field (optional)
Ctrl+E      → Export logs (optional)
Ctrl+L      → Clear logs (optional)
```

### Mouse/Touch
```
Click log   → Show/hide stack trace
Scroll      → Navigate logs
Click field → Edit search/filters
```

## 📈 Performance Visualization

### Memory Usage
```
Log Entries: 5000 (default)
Memory:      ~1-2 MB
Growth:      Linear with entries
Cleanup:     Automatic (FIFO)
```

### CPU Usage
```
Logging:     < 0.1ms per entry
UI Update:   Only when visible
Filtering:   < 1ms for 5000 entries
Export:      < 10ms for 5000 entries
```

## 🎨 Customization Examples

### Custom Toggle Key
```csharp
// In DebugConsoleUI inspector
Toggle Key: F1  // Instead of backtick
```

### Custom Max Entries
```csharp
// In GameBootstrap.cs
_services.RegisterLoggingService(
    new LoggingService(maxLogEntries: 10000)
);
```

### Custom Categories
```csharp
// In GameLogger.cs
public static class Gameplay
{
    public static void Log(string msg) => 
        LogInfo(msg, "Gameplay");
}

// Usage
GameLogger.Gameplay.Log("Player died");
```

## 🔍 Filtering Examples

### Search Examples
```
"error"           → Find all logs with "error"
"network connect" → Find network connection logs
"player"          → Find all player-related logs
```

### Level Filter Examples
```
All      → Show everything
Info     → Info and above (Info, Warning, Error, Critical)
Warning  → Warning and above (Warning, Error, Critical)
Error    → Error and above (Error, Critical)
Critical → Only critical logs
```

### Category Filter Examples
```
All          → Show all categories
Network      → Only network logs
Audio        → Only audio logs
SaveSystem   → Only save system logs
Unity        → Only Unity Debug.Log calls
```

## 📊 Export Format

### Exported File Structure
```
=== Game Logs Export ===
Export Time: 2025-10-16 14:30:00
Total Entries: 156
Unity Version: 2022.3.10f1
Platform: WindowsPlayer
Device: DESKTOP-ABC123
========================

[2025-10-16 14:23:45.123] [Info] [General] Game started

[2025-10-16 14:23:46.456] [Info] [Network] Connected to server
Stack Trace:
  at NetworkService.Connect() in NetworkService.cs:45
  at GameBootstrap.Start() in GameBootstrap.cs:23

[2025-10-16 14:23:47.789] [Warning] [Audio] Volume low
...
```

## 🎯 Best Practices Visualization

### ✅ Good Logging
```csharp
// Descriptive and actionable
GameLogger.Network.Log("Connected to server at 192.168.1.1:7777");
GameLogger.Save.LogError("Failed to save: Disk full (0 bytes available)");
GameLogger.Audio.LogWarning("Audio clip 'explosion.wav' not found in Resources");
```

### ❌ Bad Logging
```csharp
// Too vague
GameLogger.Log("Error");
GameLogger.Log("Something happened");
GameLogger.Log("Test");
```

## 🚀 Quick Reference Card

```
┌─────────────────────────────────────────────────┐
│ LOGGING QUICK REFERENCE                         │
├─────────────────────────────────────────────────┤
│ Open Console:    Press `                        │
│ Close Console:   Press ` or ✕                   │
│                                                  │
│ Basic Logging:                                  │
│   GameLogger.Log("message")                     │
│   GameLogger.LogWarning("message")              │
│   GameLogger.LogError("message")                │
│                                                  │
│ Category Logging:                               │
│   GameLogger.Network.Log("message")             │
│   GameLogger.Audio.LogWarning("message")        │
│   GameLogger.Save.LogError("message")           │
│                                                  │
│ Export Logs:                                    │
│   Click "Export" button in console             │
│   Or: logger.SaveLogsToFile(path)              │
│                                                  │
│ Filter Logs:                                    │
│   Use search field                              │
│   Select level dropdown                         │
│   Select category dropdown                      │
└─────────────────────────────────────────────────┘
```

---

**Tip**: Keep the console open during development to catch issues in real-time!
