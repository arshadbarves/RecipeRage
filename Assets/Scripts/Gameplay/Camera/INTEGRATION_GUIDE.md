# Camera System Integration Guide

Quick guide to integrate the camera system into your gameplay.

## Quick Start (5 Minutes)

### Step 1: Create Camera Settings

1. In Unity, right-click in Project window
2. Navigate to: `Create → RecipeRage → Camera → Settings`
3. Name it: `CameraSettings`
4. Move to: `Assets/Resources/Data/CameraSettings.asset`

### Step 2: Configure Settings

Select the CameraSettings asset and adjust:

```
Camera Angle: 50
Use Orthographic: ✓
Orthographic Size: 10
Follow Smooth Time: 0.15
Default Zoom: 1.0
Min Zoom: 0.8
Max Zoom: 1.5
Max Shake Intensity: 0.5
Enable Bounds: ✓
Bounds Padding: 2
```

### Step 3: Setup Arena Bounds (Optional)

In your Game scene, create or modify your arena:

```csharp
using Gameplay;
using UnityEngine;

public class ArenaSetup : MonoBehaviour
{
    [SerializeField] private Vector3 arenaSize = new Vector3(50, 0, 50);
    
    void Start()
    {
        var camera = GameplayContext.CameraController;
        if (camera != null)
        {
            Bounds bounds = new Bounds(Vector3.zero, arenaSize);
            camera.SetArenaBounds(bounds);
        }
    }
}
```

### Step 4: Test

1. Enter Play Mode
2. Start a game (host or join)
3. Camera should automatically follow your player
4. Test movement - camera should smoothly track

## That's It!

The camera system is now fully integrated. The rest happens automatically:
- ✅ Camera initializes when entering gameplay
- ✅ Local player becomes camera target on spawn
- ✅ Camera disposes when exiting gameplay

## Adding Camera Effects

### Shake on Damage

```csharp
public void TakeDamage(int amount)
{
    health -= amount;
    
    // Camera shake feedback
    var camera = GameplayContext.CameraController;
    camera?.Shake(0.3f, 0.2f);
}
```

### Zoom on Ability

```csharp
public void UseUltimate()
{
    // Zoom out for ultimate
    var camera = GameplayContext.CameraController;
    camera?.SetZoom(1.3f, 0.5f);
    
    // Execute ability
    ExecuteUltimate();
    
    // Zoom back after delay
    StartCoroutine(ResetZoomAfterDelay(2f));
}

IEnumerator ResetZoomAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    GameplayContext.CameraController?.SetZoom(1.0f, 0.5f);
}
```

## Common Patterns

### Pattern 1: Impact Feedback
```csharp
// Light impact
GameplayContext.CameraController?.Shake(0.2f, 0.15f);

// Medium impact
GameplayContext.CameraController?.Shake(0.5f, 0.3f);

// Heavy impact
GameplayContext.CameraController?.Shake(0.8f, 0.5f);
```

### Pattern 2: Zoom States
```csharp
// Normal gameplay
camera.SetZoom(1.0f, 0.3f);

// Aiming/precision
camera.SetZoom(0.8f, 0.2f);

// Overview/tactical
camera.SetZoom(1.5f, 0.5f);
```

### Pattern 3: Null-Safe Access
```csharp
// Always check for null (camera only exists during gameplay)
var camera = GameplayContext.CameraController;
if (camera != null && camera.IsInitialized)
{
    camera.Shake(0.5f, 0.3f);
}
```

## Debugging

### Enable Cinemachine Debug View

1. Select Main Camera in Hierarchy
2. In CinemachineBrain component
3. Enable "Show Debug Text"
4. Shows active virtual camera and blend info

### Check Camera Status

```csharp
var camera = GameplayContext.CameraController;
if (camera != null)
{
    Debug.Log($"Camera Initialized: {camera.IsInitialized}");
    Debug.Log($"Main Camera: {camera.MainCamera?.name}");
}
```

## Performance Tips

1. **Shake Sparingly**: Don't shake every frame
2. **Zoom Transitions**: Use reasonable durations (0.2-0.5s)
3. **Bounds**: Keep arena size reasonable for mobile
4. **Update Rate**: Camera updates automatically, don't force updates

## Next Steps

- Read full documentation: `README.md`
- Customize CameraSettings for your game feel
- Add camera effects to abilities and impacts
- Test on mobile devices for performance

## Support

If camera isn't working:
1. Check GameplayState.cs has camera initialization
2. Verify CameraSettings exists in Resources/Data/
3. Ensure Cinemachine package is installed
4. Check console for errors during initialization
