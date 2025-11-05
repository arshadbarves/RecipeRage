# Cinemachine 3.x API Notes

## API Changes from Cinemachine 2.x

This camera system is built for **Cinemachine 3.1.4+** (Unity 6 compatible).

### Key API Differences

#### 1. CinemachineFollow Damping
```csharp
// ❌ Cinemachine 2.x (Old)
follow.Damping = new Vector3(0.15f, 0.15f, 0.15f);

// ✅ Cinemachine 3.x (Current)
follow.TrackerSettings.PositionDamping = new Vector3(0.15f, 0.15f, 0.15f);
```

#### 2. CinemachineConfiner3D
```csharp
// ❌ Cinemachine 2.x (Old)
confiner.Damping = 0.5f;

// ✅ Cinemachine 3.x (Current)
// Damping is handled automatically by Cinemachine 3.x
// No manual damping property needed
```

#### 3. Virtual Camera Component
```csharp
// ✅ Cinemachine 3.x uses CinemachineCamera
var vcam = gameObject.AddComponent<CinemachineCamera>();
vcam.Priority.Value = 10; // Note: Priority is now a struct
```

## Implementation Details

### Follow Settings
The camera uses `CinemachineFollow` for smooth tracking:
- **PositionDamping**: Configurable via CameraSettings.followSmoothTime (smooth damped following)
- **FollowOffset**: Top-down offset from target
- **TrackerSettings**: Handles smooth position interpolation

### Confiner Settings
The camera uses `CinemachineConfiner3D` for arena bounds:
- **BoundingVolume**: BoxCollider defining playable area
- **Damping**: Automatic in Cinemachine 3.x
- **Padding**: Configurable via CameraSettings.boundsPadding

### Shake Implementation
Camera shake uses `CinemachineBasicMultiChannelPerlin`:
- **FrequencyGain**: Shake frequency (default: 25)
- **AmplitudeGain**: Shake intensity (animated)
- **Fade**: Linear fade-out over duration

## Compatibility

### Required Versions
- **Unity**: 6000.0.58f2 or later
- **Cinemachine**: 3.1.4 or later
- **DOTween**: Any version (via AnimationService)

### Package Installation
Cinemachine 3.x is included in Unity 6 by default. If missing:
1. Window → Package Manager
2. Unity Registry
3. Search "Cinemachine"
4. Install version 3.1.4+

## Migration Notes

If upgrading from Cinemachine 2.x:
1. Update API calls as shown above
2. Test camera behavior (damping may feel different)
3. Adjust CameraSettings if needed
4. Verify bounds constraint works correctly

## References

- [Cinemachine 3.x Documentation](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/index.html)
- [Unity 6 Cinemachine Guide](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.cinemachine.html)
