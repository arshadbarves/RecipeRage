# Mobile Console Gesture Guide

## 📱 How to Open the Console on Mobile

### The Gesture
```
┌─────────────────────────────┐
│                             │
│         👆  👆  👆          │  Hold 3 fingers
│                             │  for 1 second
│                             │
│      YOUR GAME SCREEN       │
│                             │
│                             │
└─────────────────────────────┘
```

### Step-by-Step
1. **Place 3 fingers** anywhere on the screen
2. **Hold for 1 second** (don't move them)
3. **Console appears!**
4. **Repeat gesture** or tap ✕ to close

## ⚙️ Customizing the Gesture

### In Unity Inspector
Select the `DebugConsole` GameObject and adjust:

```
DebugConsoleUI Component
├── Touch Count: 3        ← Number of fingers
└── Touch Hold Time: 1.0  ← Seconds to hold
```

### Preset Configurations

#### Quick Access (for frequent testing)
```
Touch Count: 3
Touch Hold Time: 0.5
```
**Use when**: Testing frequently, need quick access

#### Safe Mode (prevent accidents)
```
Touch Count: 4
Touch Hold Time: 1.5
```
**Use when**: Playing normally, don't want accidental triggers

#### Easy Mode (for QA team)
```
Touch Count: 2
Touch Hold Time: 0.5
```
**Use when**: QA needs easy access for bug reporting

## 🎮 Platform Differences

### iOS
- Works on iPhone and iPad
- Supports all screen sizes
- Gesture detection is instant
- No conflicts with system gestures

### Android
- Works on all Android devices
- Supports tablets and phones
- Compatible with all screen sizes
- No conflicts with navigation gestures

## ⚠️ Important Notes

### Development Builds Only
```
✅ Development Build → Console available
❌ Release Build → Console removed
```

The console is **completely removed** from release builds!

### Build Settings
```
Unity → File → Build Settings
☑ Development Build  ← Check this!
```

### Verification
After building, test on device:
1. Launch game
2. Try 3-finger gesture
3. If console appears → ✅ Working!
4. If nothing happens → Check build settings

## 🔧 Troubleshooting

### Gesture Not Working

#### Check 1: Development Build?
```
Build Settings → Development Build ☑
```

#### Check 2: Correct Finger Count?
```
Inspector → Touch Count: 3
Try: 2 or 4 fingers
```

#### Check 3: Holding Long Enough?
```
Inspector → Touch Hold Time: 1.0
Try: 0.5 seconds
```

#### Check 4: DebugConsole in Scene?
```
Hierarchy → DebugConsole GameObject
Should be present and active
```

### Gesture Triggers Accidentally

#### Solution 1: More Fingers
```
Touch Count: 4 (instead of 3)
```

#### Solution 2: Longer Hold
```
Touch Hold Time: 1.5 (instead of 1.0)
```

#### Solution 3: Disable During Gameplay
```csharp
// In your game code
var console = FindObjectOfType<DebugConsoleUI>();
console.enabled = false; // Disable during gameplay
console.enabled = true;  // Enable in pause menu
```

## 📊 Visual Feedback

### When Gesture is Detected
```
Touch detected → Hold timer starts → Console appears!
     ↓                  ↓                    ↓
  0.0 sec           1.0 sec              Toggle
```

### If You Release Early
```
Touch detected → Hold timer starts → Release → Reset
     ↓                  ↓                ↓        ↓
  0.0 sec           0.5 sec          Release   Try again
```

## 🎯 Best Practices

### For Development
- Use **3 fingers, 0.5 seconds** for quick access
- Keep console open while testing
- Export logs frequently

### For QA Testing
- Use **3 fingers, 1.0 seconds** (default)
- Train QA team on gesture
- Have them export logs with bug reports

### For Beta Testing
- Use **4 fingers, 1.5 seconds** to prevent accidents
- Include gesture instructions in beta notes
- Request log exports with feedback

## 📱 Device-Specific Tips

### Small Phones
- Use 2 fingers instead of 3
- Easier to perform on small screens
- Less hand strain

### Tablets
- 3 or 4 fingers work well
- More screen space available
- Console is more readable

### Landscape Mode
- Gesture works in any orientation
- Console adapts to screen size
- Same finger count/hold time

## 🚀 Quick Reference

```
┌─────────────────────────────────────────┐
│ MOBILE CONSOLE QUICK REFERENCE          │
├─────────────────────────────────────────┤
│ Open Console:                           │
│   Hold 3 fingers for 1 second          │
│                                         │
│ Close Console:                          │
│   Repeat gesture or tap ✕              │
│                                         │
│ Export Logs:                            │
│   Tap "Export" button in console       │
│                                         │
│ Customize Gesture:                      │
│   Inspector → DebugConsoleUI component │
│                                         │
│ Requirements:                           │
│   Development Build must be enabled    │
└─────────────────────────────────────────┘
```

## 📚 Related Documentation

- **LOGGING_MOBILE_GUIDE.md** - Complete mobile documentation
- **LOGGING_QUICK_START.md** - Quick setup guide
- **LOGGING_MOBILE_UPDATE.md** - Mobile feature summary

---

**Remember**: The console only works in Development Builds!
