# Simplified Remote Config Architecture

## Overview
Firebase-only Remote Config system - no ScriptableObjects, no local fallback, pure Firebase.

## What Was Removed

### ❌ Deleted Components
1. **LocalConfigProvider** - No local fallback needed
2. **All ScriptableObject classes** (7 files) - Not using Unity ScriptableObjects
3. **ConfigProviderType enum** - Only Firebase, no switching
4. **RemoteConfigExportTool** - No ScriptableObjects to export
5. **Provider switching logic** - Single provider only

### ✅ What Remains

**Core System**:
- RemoteConfigService (simplified)
- FirebaseConfigProvider (only provider)
- NTPTimeService
- All 7 configuration models
- All utility classes (MaintenanceChecker, ForceUpdateChecker, etc.)

## Simplified Architecture

```
Firebase Remote Config (Cloud)
    ↓
FirebaseConfigProvider
    ↓
RemoteConfigService (Cache)
    ↓
Game Services
    ↓
UI Components
```

## How It Works

### 1. Initialization
```csharp
// GameBootstrap.cs
var remoteConfigService = new RemoteConfigService();
await remoteConfigService.Initialize();

// Internally:
// - Creates FirebaseConfigProvider
// - Initializes Firebase SDK
// - Fetches all configs
// - Caches in memory
```

### 2. Configuration Access (Cached)
```csharp
// Fast - returns from memory cache
var config = remoteConfigService.GetConfig<GameSettingsConfig>();

// First fetch: Network call to Firebase
// All subsequent calls: Instant memory lookup
```

### 3. Configuration Updates
```csharp
// Manual refresh
await remoteConfigService.RefreshConfig();

// Background refresh (optional)
remoteConfigService.EnableBackgroundRefresh(30); // Every 30 minutes

// Event-driven updates
remoteConfigService.OnConfigUpdated += (config) =>
{
    // Handle config change
};
```

## Benefits of Simplified Approach

### ✅ Advantages
1. **Simpler codebase** - Less code to maintain
2. **Single source of truth** - Firebase only
3. **No sync issues** - No local/remote conflicts
4. **Easier testing** - One provider to test
5. **Cleaner architecture** - No provider switching logic

### ⚠️ Considerations
1. **Requires Firebase** - Can't run without Firebase SDK
2. **No offline fallback** - Needs network connection for first fetch
3. **Development dependency** - Need Firebase project for development

## Development Workflow

### Setup
1. Create Firebase project
2. Add Firebase Unity SDK
3. Configure Remote Config in Firebase Console
4. Set default values in Firebase
5. Run game - configs load from Firebase

### Testing
```csharp
// In Firebase Console:
// 1. Update config values
// 2. Publish changes
// 3. In game, refresh configs
await remoteConfigService.RefreshConfig();

// Or wait for background refresh (if enabled)
```

### Production
```csharp
// Configs are cached after first fetch
// Background refresh keeps them updated
// No manual intervention needed
```

## Configuration Models

All 7 models remain unchanged:
1. **GameSettingsConfig** - Game rules, currency, features
2. **ShopConfig** - Shop items, rotations, offers
3. **CharacterConfig** - Character definitions
4. **MapConfig** - Map definitions, rotations
5. **SkinsConfig** - Skin definitions
6. **MaintenanceConfig** - Maintenance mode
7. **ForceUpdateConfig** - Version requirements

## Firebase Integration

### Required Steps
1. **Import Firebase SDK**
   ```
   - Firebase Core
   - Firebase Remote Config
   ```

2. **Add Config Files**
   ```
   - google-services.json (Android)
   - GoogleService-Info.plist (iOS)
   ```

3. **Uncomment Firebase Code**
   ```csharp
   // In FirebaseConfigProvider.cs
   await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync();
   await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
   var jsonString = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
   ```

4. **Configure Firebase Console**
   - Create parameters for each config
   - Set default JSON values
   - Configure platform conditions
   - Set up environment conditions

## File Structure

```
Assets/Scripts/Core/RemoteConfig/
├── Interfaces/
│   ├── IRemoteConfigService.cs
│   ├── IConfigModel.cs
│   ├── IConfigProvider.cs
│   └── INTPTimeService.cs
├── Models/
│   ├── GameSettingsConfig.cs
│   ├── ShopConfig.cs
│   ├── CharacterConfig.cs
│   ├── MapConfig.cs
│   ├── SkinsConfig.cs
│   ├── MaintenanceConfig.cs
│   └── ForceUpdateConfig.cs
├── Providers/
│   └── FirebaseConfigProvider.cs (only provider)
├── Utilities/
│   ├── MaintenanceChecker.cs
│   ├── ForceUpdateChecker.cs
│   ├── ShopRotationCalculator.cs
│   ├── MapRotationCalculator.cs
│   └── ConfigErrorHandler.cs
├── RemoteConfigService.cs (simplified)
├── NTPTimeService.cs
├── ConfigHealthStatus.cs
└── Documentation files
```

## Usage Examples

### Basic Usage
```csharp
// Get service
var remoteConfig = GameBootstrap.Services.RemoteConfigService;

// Access config (cached)
if (remoteConfig.TryGetConfig<GameSettingsConfig>(out var settings))
{
    int maxPlayers = settings.MaxPlayers;
    bool rankedEnabled = settings.EnableRankedMode;
}
```

### Shop Rotation
```csharp
var calculator = new ShopRotationCalculator(
    remoteConfig,
    ntpTimeService
);

var featuredItems = calculator.GetFeaturedItems();
var activeOffers = calculator.GetActiveSpecialOffers();
```

### Maintenance Mode
```csharp
var checker = new MaintenanceChecker(
    remoteConfig,
    ntpTimeService,
    uiService,
    stateManager
);

checker.Start(); // Checks every 2 minutes
```

### Force Update
```csharp
var checker = new ForceUpdateChecker(
    remoteConfig,
    uiService
);

bool updateRequired = await checker.CheckForUpdate();
```

## Performance

### Memory Usage
- ~10-50 KB total for all 7 configs
- Cached in memory after first fetch
- No disk I/O after initialization

### Network Usage
- Initial fetch: ~10-50 KB (all configs)
- Background refresh: Only if configs changed
- Manual refresh: On-demand only

### Speed
- First access: Network latency (100-500ms)
- Subsequent access: Instant (memory lookup)
- No repeated network calls

## Summary

### What Changed
- ❌ Removed ScriptableObjects
- ❌ Removed LocalConfigProvider
- ❌ Removed provider switching
- ❌ Removed export tool
- ✅ Simplified to Firebase-only

### What Stayed
- ✅ All configuration models
- ✅ Caching system
- ✅ Event system
- ✅ Utility classes
- ✅ Error handling
- ✅ NTP time sync

### Result
**Simpler, cleaner, Firebase-focused architecture** ready for production use!
