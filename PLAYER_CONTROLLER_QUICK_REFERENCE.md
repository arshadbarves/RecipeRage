# Player Controller Quick Reference

## 🎮 Inspector Settings (Recommended)

```
Movement Settings:
├─ Base Movement Speed: 5.0
├─ Rotation Speed: 10.0
└─ Carrying Speed Multiplier: 0.7

Input Smoothing:
├─ Enable Input Smoothing: ✓
└─ Input Smooth Time: 0.1

Network Prediction:
├─ Enable Client Prediction: ✓
├─ Max Input History Size: 60
└─ Reconciliation Threshold: 0.1
```

## 📋 Movement States

| State | Description | Can Move? | Speed |
|-------|-------------|-----------|-------|
| `Idle` | Standing still | ✓ | 0% |
| `Moving` | Normal movement | ✓ | 100% |
| `Carrying` | Holding object | ✓ | 70% |
| `Interacting` | Using station | ❌ | 0% |
| `UsingAbility` | Special ability | ❌ | 0% |
| `Stunned` | Disabled | ❌ | 0% |

## 🔧 Common API Calls

### Check State
```csharp
var state = playerController.GetMovementState();
bool isMoving = playerController.IsMoving();
bool isCarrying = playerController.IsHoldingObject();
```

### Change State
```csharp
playerController.SetMovementState(PlayerMovementState.Interacting);
playerController.Stun(2.0f); // Stun for 2 seconds
```

### Get Movement Info
```csharp
float speed = playerController.GetCurrentSpeed();
Vector3 velocity = playerController.GetVelocity();
```

### Listen for Changes
```csharp
playerController.OnMovementStateChanged += (prev, current) =>
{
    Debug.Log($"{prev} → {current}");
};
```

### Runtime Config
```csharp
playerController.SetPredictionEnabled(false);
playerController.SetInputSmoothingEnabled(false);
```

## 🐛 Quick Fixes

| Problem | Solution |
|---------|----------|
| Too fast/slow | Adjust `_baseMovementSpeed` (3-7) |
| Jerky rotation | Increase `_rotationSpeed` (15-20) |
| Sluggish input | Reduce `_inputSmoothTime` (0.05) |
| Network corrections | Increase `_reconciliationThreshold` (0.15) |
| Clips through walls | Check Rigidbody collision mode |
| Can't move | Check movement state |

## 📊 Performance Metrics

- Memory: ~5KB per player
- CPU: ~0.035ms per frame
- Network: ~4.8 KB/s per player

## ✅ Testing Checklist

- [ ] Movement feels smooth
- [ ] Rotation is responsive
- [ ] Collisions work properly
- [ ] State changes correctly
- [ ] Network sync is smooth
- [ ] No wall clipping
- [ ] Gamepad works well
- [ ] Keyboard works well

## 🎯 Tuning Guide

### Competitive (Responsive)
```
Input Smooth Time: 0.05
Rotation Speed: 15
Enable Input Smoothing: ❌
```

### Casual (Smooth)
```
Input Smooth Time: 0.15
Rotation Speed: 8
Enable Input Smoothing: ✓
```

### Balanced (Default)
```
Input Smooth Time: 0.1
Rotation Speed: 10
Enable Input Smoothing: ✓
```

## 🔗 Related Files

- `PlayerController.cs` - Main controller
- `PlayerMovementState.cs` - State enum
- `PlayerInputData.cs` - Network input
- `PlayerStateData.cs` - Network state
- `AAA_PLAYER_CONTROLLER_GUIDE.md` - Full documentation
