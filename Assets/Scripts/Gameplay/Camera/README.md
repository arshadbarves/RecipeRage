# Camera System - Brawl Stars Style

Professional camera system for top-down multiplayer gameplay with Cinemachine 3.x integration.

## Architecture

### Design Pattern: State-Scoped System
The camera system is **gameplay-scoped** - it only exists during active gameplay and is disposed when exiting the gameplay state. This approach:
- Avoids polluting ServiceContainer with gameplay-only systems
- Provides clear lifecycle management (created in GameplayState.Enter, disposed in Exit)
- Follows SOLID principles with focused, single-responsibility controllers

### Component Structure

```
ICameraController (Interface)
└── CameraController (Main Implementation)
    ├── CameraFollowController - Player tracking with smooth damping
    ├── CameraBoundsController - Arena constraint with Cinemachine Confiner
    ├── CameraZoomController - Dynamic zoom with DOTween
    ├── CameraShakeController - Impact feedback with Perlin noise
    └── CameraSettings - ScriptableObject configuration
```

## Features

### 1. Top-Down Perspective
- Configurable camera angle (45-60 degrees recommended)
- Orthographic or perspective projection
- Optimized for Brawl Stars-like gameplay

### 2. Smooth Follow
- Cinemachine-powered smooth tracking
- Configurable damping and look-ahead
- Automatic target switching

### 3. Arena Bounds
- Constrains camera within playable area
- Uses Cinemachine Confiner3D
- Configurable padding from edges

### 4. Dynamic Zoom
- Smooth zoom transitions with DOTween
- Min/max zoom constraints
- Combat intensity-based zoom (future)

### 5. Camera Shake
- Impact feedback for abilities and damage
- Perlin noise-based shake
- Configurable intensity and duration

## Usage

### Basic Setup (Automatic)

The camera system is automatically initialized in `GameplayState.Enter()`:

```csharp
// GameplayState.cs
private void InitializeCameraSystem()
{
    var settings = Resources.Load<CameraSettings>("Data/CameraSettings");
    _cameraController = new CameraController(settings);
    _cameraController.Initialize();
    GameplayContext.CameraController = _cameraController;
}
```

### Accessing the Camera

From any gameplay script:

```csharp
using Gameplay;

// Get camera controller
var camera = GameplayContext.CameraController;

// Set follow target (done automatically for local player)
camera.SetFollowTarget(playerTransform);

// Trigger shake effect
camera.Shake(intensity: 0.5f, duration: 0.3f);

// Zoom in/out
camera.SetZoom(zoomLevel: 1.2f, duration: 0.5f);

// Set arena bounds
camera.SetArenaBounds(new Bounds(Vector3.zero, new Vector3(50, 0, 50)));
```

### Player Integration (Automatic)

The local player automatically becomes the camera target:

```csharp
// PlayerController.cs - OnNetworkSpawn()
if (IsLocalPlayer)
{
    SetupCamera(); // Automatically sets this player as camera target
}
```

### Configuration

Create a CameraSettings asset:

1. Right-click in Project window
2. Create → RecipeRage → Camera → Settings
3. Save to `Assets/Resources/Data/CameraSettings.asset`
4. Adjust settings in Inspector

**Recommended Settings for Brawl Stars Style:**
```
Camera Angle: 50°
Orthographic: true
Orthographic Size: 10
Follow Smooth Time: 0.15s
Min Zoom: 0.8x
Max Zoom: 1.5x
Shake Intensity: 0.5
Bounds Padding: 2 units
```

## Advanced Usage

### Camera Shake Examples

```csharp
var camera = GameplayContext.CameraController;

// Light shake (player hit)
camera.Shake(0.2f, 0.2f);

// Medium shake (ability used)
camera.Shake(0.5f, 0.3f);

// Heavy shake (explosion)
camera.Shake(0.8f, 0.5f);
```

### Dynamic Zoom

```csharp
var camera = GameplayContext.CameraController;

// Zoom in for precision
camera.SetZoom(0.8f, 0.3f);

// Zoom out for overview
camera.SetZoom(1.5f, 0.5f);

// Reset to default
camera.SetZoom(1.0f, 0.3f);
```

### Arena Bounds Setup

```csharp
// In your arena/map script
void Start()
{
    var camera = GameplayContext.CameraController;
    if (camera != null)
    {
        // Get arena bounds from collider or manual setup
        Bounds arenaBounds = GetComponent<BoxCollider>().bounds;
        camera.SetArenaBounds(arenaBounds);
    }
}
```

## Integration with Abilities

Example: Dash ability with camera shake

```csharp
public class DashAbility : CharacterAbility
{
    public override void Execute()
    {
        // Perform dash
        player.Dash(direction);
        
        // Camera feedback
        var camera = GameplayContext.CameraController;
        camera?.Shake(0.3f, 0.2f);
    }
}
```

## Lifecycle

```
GameplayState.Enter()
├── InitializeCameraSystem()
│   ├── Load CameraSettings
│   ├── Create CameraController
│   ├── Initialize Cinemachine
│   └── Set GameplayContext.CameraController
│
Player Spawns (Local)
├── PlayerController.OnNetworkSpawn()
└── SetupCamera() → Sets as follow target

Gameplay
├── Camera follows local player
├── Shake/zoom effects as needed
└── Constrained within arena bounds

GameplayState.Exit()
├── Dispose CameraController
├── Destroy camera rig
└── Clear GameplayContext
```

## Performance

- **Update Rate**: Every frame (optimized by Cinemachine)
- **Memory**: ~1-2 MB for camera rig and controllers
- **CPU**: Minimal (Cinemachine handles heavy lifting)
- **Mobile**: Fully optimized for 60 FPS

## Troubleshooting

### Camera not following player
- Check that player is local player (`IsLocalPlayer`)
- Verify `GameplayContext.CameraController` is not null
- Ensure camera initialized before player spawns

### Camera jittering
- Increase `followSmoothTime` in CameraSettings
- Check physics timestep (Fixed Timestep = 0.02)
- Ensure player movement is in FixedUpdate

### Bounds not working
- Verify `enableBounds` is true in CameraSettings
- Check that arena bounds are set correctly
- Ensure bounds collider is on correct layer

## Future Enhancements

- [ ] Spectator mode (follow other players when dead)
- [ ] Combat intensity-based zoom
- [ ] Multiple camera states (menu, gameplay, replay)
- [ ] Camera transitions between states
- [ ] Cinematic camera paths
- [ ] Split-screen support

## Technical Details

**Dependencies:**
- Cinemachine 3.1.4+
- DOTween (via AnimationService)
- Unity Netcode for GameObjects

**Namespace:** `Gameplay.Camera`

**Key Classes:**
- `ICameraController` - Main interface
- `CameraController` - Implementation
- `CameraSettings` - Configuration ScriptableObject
- `GameplayContext` - Static accessor

**SOLID Principles:**
- Single Responsibility: Each controller handles one aspect
- Open/Closed: Extensible via new controllers
- Liskov Substitution: ICameraController interface
- Interface Segregation: Focused interface
- Dependency Inversion: Depends on abstractions
