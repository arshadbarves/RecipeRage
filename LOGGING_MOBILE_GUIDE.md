# Logging System - Mobile Guide

## 📱 Mobile-Specific Features

### Touch Gesture to Open Console
**Default**: Hold **3 fingers** for **1 second** to toggle the console

This gesture works on both iOS and Android devices.

### Why 3 Fingers?
- Won't interfere with normal gameplay (1-2 finger gestures)
- Easy to perform during testing
- Difficult to trigger accidentally
- Works consistently across devices

## 🔧 Customizing the Gesture

In the `DebugConsoleUI` component inspector:

```
Touch Count: 3        (number of fingers required)
Touch Hold Time: 1.0  (seconds to hold)
```

### Recommended Settings

| Use Case | Touch Count | Hold Time |
|----------|-------------|-----------|
| Default | 3 | 1.0s |
| Quick Access | 3 | 0.5s |
| Prevent Accidents | 4 | 1.5s |
| Easy Testing | 2 | 0.5s |

## 🚀 Mobile Setup

### Step 1: Enable Development Build
In Unity Build Settings:
1. File → Build Settings
2. Check **"Development Build"**
3. Build and deploy to device

**Important**: The console is **automatically disabled** in release builds!

### Step 2: Test on Device
1. Launch your game on device
2. Hold 3 fingers on screen for 1 second
3. Console should appear
4. Tap the ✕ button or use gesture again to close

### Step 3: Export Logs from Device

#### iOS
1. Open console with 3-finger gesture
2. Tap "Export" button
3. Logs saved to: `Application.persistentDataPath`
4. Access via Xcode → Window → Devices and Simulators → Download Container

#### Android
1. Open console with 3-finger gesture
2. Tap "Export" button
3. Logs saved to: `/storage/emulated/0/Android/data/[your.package]/files/`
4. Access via Android File Transfer or adb pull

## 📊 Performance on Mobile

### Memory Usage
- **5000 logs**: ~1-2 MB
- **Console UI**: ~500 KB
- **Total**: ~2-3 MB (negligible on modern devices)

### CPU Usage
- **Logging**: < 0.1ms per entry
- **UI Update**: Only when visible
- **Touch Detection**: < 0.01ms per frame

### Battery Impact
- **Minimal** when console is closed
- **Low** when console is open
- **No impact** in release builds (completely removed)

## 🎮 Mobile UI Considerations

### Console Size
The console automatically adapts to screen size:
- **Portrait**: 90% width, 80% height
- **Landscape**: 90% width, 80% height
- **Tablets**: Same responsive sizing

### Touch-Friendly Controls
- Large buttons (minimum 44x44 points)
- Scrollable log view
- Touch-friendly dropdowns
- Easy-to-tap close button

### Orientation Support
The console works in both portrait and landscape modes.

## 🔍 Debugging on Mobile

### Common Scenarios

#### Scenario 1: Performance Issues
```csharp
void Update()
{
    if (Time.deltaTime > 0.033f) // < 30 FPS
    {
        GameLogger.LogWarning($"Low FPS: {1f/Time.deltaTime:F1}", "Performance");
    }
}
```

#### Scenario 2: Network Issues
```csharp
void OnConnectionFailed()
{
    GameLogger.Network.LogError($"Connection failed: {error}");
    // Open console to see logs
}
```

#### Scenario 3: Memory Warnings
```csharp
void OnApplicationLowMemory()
{
    GameLogger.LogWarning($"Low memory warning! Used: {GC.GetTotalMemory(false) / 1024 / 1024} MB", "Memory");
}
```

## 📤 Exporting Logs for Bug Reports

### Method 1: In-Game Export
1. Open console (3-finger gesture)
2. Tap "Export" button
3. Note the file path shown
4. Retrieve file from device

### Method 2: Programmatic Export
```csharp
public void OnBugReportButton()
{
    var logger = GameBootstrap.Services?.LoggingService;
    if (logger != null)
    {
        var path = Path.Combine(Application.persistentDataPath, "bug_report.txt");
        logger.SaveLogsToFile(path);
        
        // Show path to user or trigger share
        Debug.Log($"Bug report saved: {path}");
    }
}
```

### Method 3: Share Sheet (iOS/Android)
```csharp
using System.IO;

public void ShareLogs()
{
    var logger = GameBootstrap.Services?.LoggingService;
    if (logger == null) return;
    
    var path = Path.Combine(Application.persistentDataPath, "logs.txt");
    logger.SaveLogsToFile(path);
    
    // Use native share (requires plugin)
    // NativeShare.Share("Game Logs", path);
}
```

## 🛠️ Development Workflow

### Testing on Device
1. Build with Development Build enabled
2. Deploy to device
3. Play and test features
4. Use 3-finger gesture to check logs
5. Export logs if issues found

### QA Testing
1. Give QA team development builds
2. Train them on 3-finger gesture
3. Have them export logs when bugs occur
4. Logs include timestamps and stack traces

### Beta Testing
1. Provide development builds to beta testers
2. Include instructions for opening console
3. Request log exports with bug reports
4. Analyze logs to fix issues

## ⚠️ Important Notes

### Release Builds
- Console is **completely removed** in release builds
- No performance impact
- No memory overhead
- GameLogger calls are compiled out
- Users cannot access debug console

### Development Builds
- Console is available
- Minimal performance impact
- Useful for testing and debugging
- Can be toggled on/off

### Build Configuration
```csharp
// This code only runs in development builds
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    GameLogger.Log("Debug message");
#endif
```

## 🎯 Best Practices for Mobile

### 1. Log Important Events
```csharp
void OnApplicationPause(bool pause)
{
    GameLogger.Log($"App {(pause ? "paused" : "resumed")}", "Lifecycle");
}

void OnApplicationFocus(bool focus)
{
    GameLogger.Log($"App {(focus ? "focused" : "unfocused")}", "Lifecycle");
}
```

### 2. Log Device Info
```csharp
void Start()
{
    GameLogger.Log($"Device: {SystemInfo.deviceModel}", "System");
    GameLogger.Log($"OS: {SystemInfo.operatingSystem}", "System");
    GameLogger.Log($"Memory: {SystemInfo.systemMemorySize} MB", "System");
    GameLogger.Log($"GPU: {SystemInfo.graphicsDeviceName}", "System");
}
```

### 3. Log Touch Input Issues
```csharp
void Update()
{
    if (Input.touchCount > 0)
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                GameLogger.Log($"Touch at {touch.position}", "Input");
            }
        }
    }
}
```

### 4. Monitor Performance
```csharp
void LateUpdate()
{
    if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
    {
        float fps = 1f / Time.deltaTime;
        if (fps < 30)
        {
            GameLogger.LogWarning($"FPS: {fps:F1}", "Performance");
        }
    }
}
```

## 📱 Platform-Specific Tips

### iOS
- Use Xcode to access exported logs
- Check Console app for Unity logs
- Use Instruments for performance profiling
- TestFlight builds can be development builds

### Android
- Use Android Studio Logcat
- Access files via Android File Transfer
- Use adb to pull log files
- Google Play Internal Testing supports dev builds

## 🔧 Troubleshooting

### Console Not Opening on Mobile
- [ ] Verify it's a Development Build
- [ ] Check touch count setting (try 2 fingers)
- [ ] Reduce hold time (try 0.5s)
- [ ] Check DebugConsoleUI is in scene
- [ ] Verify GameBootstrap is initialized

### Gesture Conflicts
- [ ] Increase touch count to 4 fingers
- [ ] Increase hold time to 1.5s
- [ ] Disable gesture during gameplay
- [ ] Use button in debug menu instead

### Export Not Working
- [ ] Check write permissions
- [ ] Verify persistentDataPath exists
- [ ] Check available storage space
- [ ] Look for error messages in console

## 📚 Additional Resources

- **Main Guide**: LOGGING_SYSTEM_GUIDE.md
- **Quick Start**: LOGGING_QUICK_START.md
- **Setup Checklist**: LOGGING_SETUP_CHECKLIST.md

---

**Remember**: The console is only available in Development Builds!
