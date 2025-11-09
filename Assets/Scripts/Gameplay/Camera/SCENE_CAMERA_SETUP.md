# Scene Camera Setup Guide

## Problem: Can't See Game Scene in Editor

The gameplay camera is created at runtime, so you can't see the Game scene in the Unity Editor.

## Solution 1: Use Scene View (Recommended)

1. Open Game scene
2. Use **Scene View** to navigate and edit
3. The camera will work automatically in Play Mode

## Solution 2: Add Temporary Editor Camera

If you want to preview the camera angle while editing:

### Steps:

1. **In Game scene, create a temporary camera:**
   - GameObject → Camera
   - Name it: "EditorCamera_TEMP"
   - Tag: Untagged (NOT MainCamera)

2. **Position it for top-down view:**
   - Position: (0, 15, -10)
   - Rotation: (50, 0, 0)
   - Projection: Orthographic
   - Size: 10

3. **Add this script to disable it at runtime:**

```csharp
using UnityEngine;

public class EditorOnlyCamera : MonoBehaviour
{
    void Awake()
    {
        // Disable in Play Mode (runtime camera takes over)
        if (Application.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }
}
```

4. **Attach script to EditorCamera_TEMP**

### Result:
- ✅ You can see the scene in Editor
- ✅ Camera disables automatically in Play Mode
- ✅ Runtime camera takes over during gameplay

## Solution 3: Use Cinemachine Scene View

1. Select any GameObject in Game scene
2. In Scene View, click the Cinemachine icon
3. Enable "Game View Guides"
4. This shows camera framing in Scene View

## Recommended Workflow:

1. **Edit in Scene View** - Navigate freely
2. **Test in Play Mode** - See actual camera behavior
3. **Adjust CameraSettings** - Tweak camera feel
4. **Repeat** - Iterate until perfect

## Important Notes:

- The runtime camera is tagged "MainCamera" automatically
- Don't add a permanent Main Camera to Game scene
- The runtime camera is destroyed when exiting gameplay
- This is intentional - clean lifecycle management

## Testing the Camera:

1. Create CameraSettings asset (if not done)
2. Enter Play Mode
3. Start a game (host or join)
4. Look in Hierarchy → "GameplayCamera" appears
5. Camera follows your player automatically
6. Exit gameplay → Camera disappears

## Troubleshooting:

**Q: I don't see anything in Game View during Play Mode**
A: Check Console for errors. Camera might not be initializing.

**Q: Camera isn't following player**
A: Verify CameraSettings exists in Resources/Data/

**Q: Multiple cameras in scene**
A: Remove any Main Camera from Game scene. Runtime camera handles it.

**Q: Can't edit scene properly**
A: Use Scene View, not Game View, for editing.
