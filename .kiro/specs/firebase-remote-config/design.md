# Firebase Remote Config System Design

## Overview

The Firebase Remote Config system provides a centralized, cloud-based configuration management solution for RecipeRage. It enables dynamic updates to game settings, shop items, characters, maps, and operational controls (maintenance, force update) without requiring app updates. The system is built on SOLID principles with a provider-based architecture that supports Firebase as the primary source and local ScriptableObjects for development testing.

### Key Design Goals

1. **Online-First Architecture**: Firebase Remote Config as the primary source with graceful error handling
2. **Type Safety**: Strongly-typed configuration models with compile-time validation
3. **Extensibility**: Easy addition of new configuration domains without modifying existing code
4. **Platform Awareness**: iOS/Android specific configurations with environment conditions
5. **Time Reliability**: NTP-based time synchronization for rotation schedules
6. **Developer Experience**: ScriptableObject-based editing with JSON export for Firebase

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      GameBootstrap                          │
│                  (Initialization Layer)                     │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  RemoteConfigService                        │
│              (Core Configuration Manager)                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  - Initialize()                                       │  │
│  │  - GetConfig<T>()                                     │  │
│  │  - RefreshConfig()                                    │  │
│  │  - OnConfigUpdated event                             │  │
│  └──────────────────────────────────────────────────────┘  │
└────────┬────────────────────────────────────────┬───────────┘
         │                                        │
         ▼                                        ▼
┌──────────────────────┐              ┌──────────────────────┐
│  IConfigProvider     │              │   NTPTimeService     │
│   (Strategy)         │              │  (Time Sync)         │
├──────────────────────┤              ├──────────────────────┤
│ - FetchConfig()      │              │ - GetServerTime()    │
│ - GetConfig<T>()     │              │ - SyncTime()         │
└──────────────────────┘              └──────────────────────┘
         │
         ├─────────────────┐
         ▼                 ▼
┌──────────────┐  ┌──────────────┐
│   Firebase   │  │    Local     │
│   Provider   │  │   Provider   │
└──────────────┘  └──────────────┘
         │                 │
         ▼                 ▼
┌──────────────┐  ┌──────────────┐
│   Firebase   │  │ Scriptable   │
│   Remote     │  │   Objects    │
│   Config     │  │              │
└──────────────┘  └──────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Configuration Models                      │
│  ┌──────────────┬──────────────┬──────────────┬─────────┐  │
│  │ GameSettings │  ShopConfig  │ CharacterCfg │ MapCfg  │  │
│  │   Config     │              │              │         │  │
│  ├──────────────┼──────────────┼──────────────┼─────────┤  │
│  │ SkinsConfig  │ Maintenance  │ ForceUpdate  │ etc...  │  │
│  │              │   Config     │   Config     │         │  │
│  └──────────────┴──────────────┴──────────────┴─────────┘  │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Game Services                             │
│  ┌──────────────┬──────────────┬──────────────┬─────────┐  │
│  │   UIService  │  ShopService │ CharacterSvc │ MapSvc  │  │
│  ├──────────────┼──────────────┼──────────────┼─────────┤  │
│  │  SkinsService│ GameStateMgr │ CurrencySvc  │ etc...  │  │
│  └──────────────┴──────────────┴──────────────┴─────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

#### RemoteConfigService (Core)
- **Single Responsibility**: Manage configuration lifecycle and distribution
- **Dependencies**: IConfigProvider, NTPTimeService, IEventBus
- **Responsibilities**:
  - Initialize configuration system on game startup
  - Fetch configuration from active provider
  - Distribute configuration to game services
  - Handle configuration updates and notifications
  - Manage provider selection (Firebase/Local/Cache)

#### IConfigProvider (Strategy Pattern)
- **Single Responsibility**: Abstract configuration data source
- **Implementations**:
  - **FirebaseConfigProvider**: Fetches from Firebase Remote Config
  - **LocalConfigProvider**: Loads from ScriptableObjects
- **Interface**:
  ```csharp
  public interface IConfigProvider
  {
      UniTask<bool> Initialize();
      UniTask<T> FetchConfig<T>(string key) where T : IConfigModel;
      bool IsAvailable();
      string ProviderName { get; }
  }
  ```

#### NTPTimeService
- **Single Responsibility**: Provide reliable server time
- **Responsibilities**:
  - Synchronize with Google NTP server
  - Calculate time offset from device time
  - Provide server time for rotation calculations
  - Re-sync periodically (every 6 hours)

#### Configuration Models (Data Transfer Objects)
- **Single Responsibility**: Define configuration data structure
- **Characteristics**:
  - Immutable data classes
  - JSON serializable
  - Implement IConfigModel interface
  - Include validation logic

## Components and Interfaces

### Core Interfaces

#### IRemoteConfigService
```csharp
public interface IRemoteConfigService
{
    // Initialization
    UniTask<bool> Initialize();
    
    // Configuration Access
    T GetConfig<T>() where T : class, IConfigModel;
    bool TryGetConfig<T>(out T config) where T : class, IConfigModel;
    
    // Configuration Refresh
    UniTask<bool> RefreshConfig();
    UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel;
    
    // Provider Management
    void SetProvider(ConfigProviderType providerType);
    ConfigProviderType CurrentProvider { get; }
    
    // Status
    ConfigHealthStatus HealthStatus { get; }
    DateTime LastUpdateTime { get; }
    
    // Events
    event Action<IConfigModel> OnConfigUpdated;
    event Action<Type, IConfigModel> OnSpecificConfigUpdated;
    event Action<ConfigHealthStatus> OnHealthStatusChanged;
}
```

#### IConfigModel
```csharp
public interface IConfigModel
{
    string ConfigKey { get; }
    string Version { get; }
    bool Validate();
    DateTime LastModified { get; }
}
```

#### IConfigProvider
```csharp
public interface IConfigProvider
{
    string ProviderName { get; }
    bool IsAvailable();
    
    UniTask<bool> Initialize();
    UniTask<T> FetchConfig<T>(string key) where T : IConfigModel;
    UniTask<Dictionary<string, IConfigModel>> FetchAllConfigs();
}
```

#### INTPTimeService
```csharp
public interface INTPTimeService
{
    UniTask<bool> SyncTime();
    DateTime GetServerTime();
    TimeSpan GetTimeOffset();
    bool IsSynced { get; }
    DateTime LastSyncTime { get; }
}
```

### Configuration Models

#### GameSettingsConfig
```csharp
[Serializable]
public class GameSettingsConfig : IConfigModel
{
    public string ConfigKey => "GameSettings";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    // Global Game Rules
    public float GlobalScoreMultiplier { get; set; }
    public float GlobalDifficultyMultiplier { get; set; }
    public bool EnablePowerUps { get; set; }
    public bool EnableSpecialAbilities { get; set; }
    
    // Currency & Progression
    public int BaseCoinsPerMatch { get; set; }
    public int BaseGemsPerMatch { get; set; }
    public float WinBonusMultiplier { get; set; }
    public int ExperiencePerMatch { get; set; }
    
    // Feature Flags
    public bool EnableSeasonPass { get; set; }
    public bool EnableDailyRewards { get; set; }
    public bool EnableLeaderboards { get; set; }
    public bool EnableClanSystem { get; set; }
    
    // Matchmaking
    public int MinPlayersToStart { get; set; }
    public int MaxPlayersPerMatch { get; set; }
    public float MatchmakingTimeout { get; set; }
    
    public bool Validate()
    {
        return GlobalScoreMultiplier > 0 
            && GlobalDifficultyMultiplier > 0
            && MinPlayersToStart > 0
            && MaxPlayersPerMatch >= MinPlayersToStart;
    }
}
```

#### ShopConfig
```csharp
[Serializable]
public class ShopConfig : IConfigModel
{
    public string ConfigKey => "ShopConfig";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    public List<ShopCategory> Categories { get; set; }
    public ShopRotationSchedule RotationSchedule { get; set; }
    public List<SpecialOffer> SpecialOffers { get; set; }
    
    public bool Validate()
    {
        return Categories != null && Categories.Count > 0;
    }
}

[Serializable]
public class ShopCategory
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<ShopItem> Items { get; set; }
    public int DisplayOrder { get; set; }
}

[Serializable]
public class ShopItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public CurrencyType Currency { get; set; }
    public ShopItemType Type { get; set; }
    public string IconAddress { get; set; }
    public bool IsAvailable { get; set; }
    public int PurchaseLimit { get; set; } // 0 = unlimited
}

[Serializable]
public class ShopRotationSchedule
{
    public List<RotationPeriod> Periods { get; set; }
}

[Serializable]
public class RotationPeriod
{
    public long StartTimestamp { get; set; } // Unix timestamp
    public long EndTimestamp { get; set; }
    public List<string> FeaturedItemIds { get; set; }
}

[Serializable]
public class SpecialOffer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int OriginalPrice { get; set; }
    public int DiscountedPrice { get; set; }
    public CurrencyType Currency { get; set; }
    public long StartTimestamp { get; set; }
    public long EndTimestamp { get; set; }
    public List<string> ItemIds { get; set; }
}
```

#### CharacterConfig
```csharp
[Serializable]
public class CharacterConfig : IConfigModel
{
    public string ConfigKey => "CharacterConfig";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    public List<CharacterDefinition> Characters { get; set; }
    
    public bool Validate()
    {
        return Characters != null && Characters.Count > 0;
    }
}

[Serializable]
public class CharacterDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }
    public bool UnlockedByDefault { get; set; }
    
    // Unlock Requirements
    public int UnlockCost { get; set; }
    public int UnlockLevel { get; set; }
    
    // Stats
    public float MovementSpeedModifier { get; set; }
    public float InteractionSpeedModifier { get; set; }
    public float CarryingCapacityModifier { get; set; }
    
    // Ability
    public string AbilityType { get; set; }
    public float AbilityCooldown { get; set; }
    public float AbilityDuration { get; set; }
    public string AbilityParameters { get; set; } // JSON
    
    // Assets
    public string PrefabAddress { get; set; }
    public string IconAddress { get; set; }
}
```

#### MapConfig
```csharp
[Serializable]
public class MapConfig : IConfigModel
{
    public string ConfigKey => "MapConfig";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    public List<MapDefinition> Maps { get; set; }
    public MapRotationSchedule RotationSchedule { get; set; }
    
    public bool Validate()
    {
        return Maps != null && Maps.Count > 0;
    }
}

[Serializable]
public class MapDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }
    
    // Match Settings (moved from GameSettings)
    public float MatchDuration { get; set; } // seconds
    public int TrophyReward { get; set; }
    public int TrophyLoss { get; set; }
    
    // Gameplay
    public int MaxPlayers { get; set; }
    public List<string> SupportedGameModes { get; set; }
    public float RecipeDifficultyMultiplier { get; set; }
    public float OrderFrequency { get; set; }
    public int MaxActiveOrders { get; set; }
    
    // Rotation
    public MapRotationConfig Rotation { get; set; }
    
    // Assets
    public string SceneAddress { get; set; }
    public string ThumbnailAddress { get; set; }
}

[Serializable]
public class MapRotationConfig
{
    public bool IsRotating { get; set; }
    public long RotationDuration { get; set; } // seconds
    public long NextRotationTimestamp { get; set; } // Unix timestamp
}

[Serializable]
public class MapRotationSchedule
{
    public List<MapRotationPeriod> Periods { get; set; }
}

[Serializable]
public class MapRotationPeriod
{
    public long StartTimestamp { get; set; }
    public long EndTimestamp { get; set; }
    public List<string> ActiveMapIds { get; set; }
}
```

#### SkinsConfig
```csharp
[Serializable]
public class SkinsConfig : IConfigModel
{
    public string ConfigKey => "SkinsConfig";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    public List<SkinDefinition> Skins { get; set; }
    
    public bool Validate()
    {
        return Skins != null && Skins.Count > 0;
    }
}

[Serializable]
public class SkinDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public string Rarity { get; set; }
    public bool UnlockedByDefault { get; set; }
    public int UnlockCost { get; set; }
    public string UnlockType { get; set; }
    public string PrefabAddress { get; set; }
    public string IconAddress { get; set; }
    public List<string> Tags { get; set; }
}
```

#### MaintenanceConfig
```csharp
[Serializable]
public class MaintenanceConfig : IConfigModel
{
    public string ConfigKey => "MaintenanceConfig";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    public bool IsMaintenanceActive { get; set; }
    public long MaintenanceStartTimestamp { get; set; }
    public long MaintenanceEndTimestamp { get; set; }
    public string MaintenanceMessage { get; set; }
    public string MaintenanceTitle { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    
    public bool Validate()
    {
        if (IsMaintenanceActive)
        {
            return MaintenanceStartTimestamp > 0 
                && MaintenanceEndTimestamp > MaintenanceStartTimestamp
                && !string.IsNullOrEmpty(MaintenanceMessage);
        }
        return true;
    }
}
```

#### ForceUpdateConfig
```csharp
[Serializable]
public class ForceUpdateConfig : IConfigModel
{
    public string ConfigKey => "ForceUpdateConfig";
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    
    public bool IsUpdateRequired { get; set; }
    public PlatformVersionRequirement iOS { get; set; }
    public PlatformVersionRequirement Android { get; set; }
    public string UpdateMessage { get; set; }
    public string UpdateTitle { get; set; }
    public UpdateUrgency Urgency { get; set; }
    
    public bool Validate()
    {
        return iOS != null && Android != null;
    }
}

[Serializable]
public class PlatformVersionRequirement
{
    public string MinimumVersion { get; set; }
    public string RecommendedVersion { get; set; }
    public string StoreUrl { get; set; }
}

public enum UpdateUrgency
{
    Optional,
    Recommended,
    Required
}
```

## Data Models

### Enums

```csharp
public enum ConfigProviderType
{
    Firebase,
    Local
}

public enum ConfigHealthStatus
{
    Healthy,
    Degraded,
    Failed
}

public enum CurrencyType
{
    Coins,
    Gems
}

public enum ShopItemType
{
    Skin,
    Weapon,
    Bundle,
    PowerUp,
    Currency
}
```

### Firebase Remote Config Conditions

Firebase conditions will be set up as follows:

```
Condition Name: iOS_Production
Expression: app.platform == 'ios' && app.environment == 'production'

Condition Name: iOS_Development
Expression: app.platform == 'ios' && app.environment == 'development'

Condition Name: Android_Production
Expression: app.platform == 'android' && app.environment == 'production'

Condition Name: Android_Development
Expression: app.platform == 'android' && app.environment == 'development'
```

## Error Handling

### Error Handling Strategy

```csharp
public class ConfigErrorHandler
{
    private readonly ILoggingService _logger;
    private readonly IEventBus _eventBus;
    
    public void HandleFetchError(Exception ex, string configKey)
    {
        _logger.LogError($"Failed to fetch config '{configKey}': {ex.Message}");
        _eventBus.Publish(new ConfigFetchFailedEvent(configKey, ex));
        
        // Show user-friendly error
        ShowNoInternetPopup();
    }
    
    public void HandleValidationError(IConfigModel config, List<string> errors)
    {
        _logger.LogError($"Config validation failed for '{config.ConfigKey}': {string.Join(", ", errors)}");
        _eventBus.Publish(new ConfigValidationFailedEvent(config.ConfigKey, errors));
        
        // Use fallback config
        UseFallbackConfig(config.ConfigKey);
    }
    
    private void ShowNoInternetPopup()
    {
        var uiService = GameBootstrap.Services.UIService;
        uiService.ShowPopup("NoInternetPopup", new NoInternetPopupData
        {
            Title = "No Internet Connection",
            Message = "Please check your connection and try again.",
            OnRetry = () => RetryFetch()
        });
    }
}
```

### Fallback Chain

```
1. Firebase Remote Config (Primary)
   ↓ (on failure - show "No Internet" popup)
2. Embedded ScriptableObjects (Manual override only)
   ↓ (on failure)
3. Hardcoded Defaults (Last Resort)
```

## Testing Strategy

### Unit Tests

```csharp
[TestFixture]
public class RemoteConfigServiceTests
{
    [Test]
    public async Task Initialize_WithFirebaseProvider_FetchesConfig()
    {
        // Arrange
        var mockProvider = new MockFirebaseProvider();
        var service = new RemoteConfigService(mockProvider, ...);
        
        // Act
        var result = await service.Initialize();
        
        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(service.GetConfig<GameSettingsConfig>());
    }
    
    [Test]
    public async Task GetConfig_WhenNotInitialized_ReturnsNull()
    {
        // Arrange
        var service = new RemoteConfigService(...);
        
        // Act
        var config = service.GetConfig<GameSettingsConfig>();
        
        // Assert
        Assert.IsNull(config);
    }
    
    [Test]
    public void SetProvider_ChangesActiveProvider()
    {
        // Arrange
        var service = new RemoteConfigService(...);
        
        // Act
        service.SetProvider(ConfigProviderType.Local);
        
        // Assert
        Assert.AreEqual(ConfigProviderType.Local, service.CurrentProvider);
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class RemoteConfigIntegrationTests
{
    [Test]
    public async Task EndToEnd_FetchAndUseConfig()
    {
        // Arrange
        var service = CreateRealService();
        
        // Act
        await service.Initialize();
        var gameSettings = service.GetConfig<GameSettingsConfig>();
        
        // Assert
        Assert.IsNotNull(gameSettings);
        Assert.IsTrue(gameSettings.Validate());
    }
    
    [Test]
    public async Task Rotation_CalculatesCorrectly()
    {
        // Arrange
        var service = CreateRealService();
        var ntpService = new NTPTimeService();
        await ntpService.SyncTime();
        
        // Act
        var shopConfig = service.GetConfig<ShopConfig>();
        var currentRotation = GetCurrentRotation(shopConfig, ntpService.GetServerTime());
        
        // Assert
        Assert.IsNotNull(currentRotation);
        Assert.IsTrue(currentRotation.FeaturedItemIds.Count > 0);
    }
}
```

### Manual Testing Checklist

1. **Firebase Connection**
   - [ ] Successful fetch on first launch
   - [ ] No internet popup appears when offline
   - [ ] Retry button works correctly
   
2. **Provider Switching**
   - [ ] Manual switch to local provider works
   - [ ] Local ScriptableObjects load correctly
   - [ ] Switch back to Firebase works
   
3. **Configuration Updates**
   - [ ] Config changes in Firebase reflect in app
   - [ ] Services receive update notifications
   - [ ] UI updates with new config
   
4. **Rotation Schedules**
   - [ ] Shop rotation changes at correct time
   - [ ] Map rotation changes at correct time
   - [ ] NTP time used (not device time)
   
5. **Maintenance Mode**
   - [ ] Maintenance warning appears 10 min before
   - [ ] Active matches can complete
   - [ ] New matches blocked during maintenance
   - [ ] Maintenance message displays correctly
   
6. **Force Update**
   - [ ] Update prompt appears for old versions
   - [ ] Store link opens correctly
   - [ ] Platform-specific versions checked
   
7. **Platform Conditions**
   - [ ] iOS gets iOS-specific config
   - [ ] Android gets Android-specific config
   - [ ] Environment conditions work (prod/dev)

## Implementation Notes

### Firebase Setup

1. **Create Firebase Project**
   - Add iOS and Android apps
   - Download google-services.json (Android) and GoogleService-Info.plist (iOS)
   
2. **Configure Remote Config Parameters**
   - Create parameters for each config domain
   - Set up conditions for platform/environment
   - Set default values
   
3. **Unity Firebase SDK**
   - Import Firebase Unity SDK
   - Configure Firebase settings
   - Initialize Firebase in GameBootstrap

### ScriptableObject Structure

```
Assets/
└── Resources/
    └── RemoteConfig/
        ├── GameSettings.asset
        ├── ShopConfig.asset
        ├── CharacterConfig.asset
        ├── MapConfig.asset
        ├── SkinsConfig.asset
        ├── MaintenanceConfig.asset
        └── ForceUpdateConfig.asset
```

### JSON Export Tool

Create editor tool to export ScriptableObjects to JSON:

```csharp
[MenuItem("Tools/Remote Config/Export to JSON")]
public static void ExportToJSON()
{
    var configs = Resources.LoadAll<ScriptableObject>("RemoteConfig");
    foreach (var config in configs)
    {
        string json = JsonUtility.ToJson(config, true);
        string path = $"Exports/{config.name}.json";
        File.WriteAllText(path, json);
        Debug.Log($"Exported {config.name} to {path}");
    }
}
```

### NTP Implementation

Use Google NTP server: `time.google.com`

```csharp
public class NTPTimeService : INTPTimeService
{
    private const string NTP_SERVER = "time.google.com";
    private TimeSpan _timeOffset;
    private DateTime _lastSyncTime;
    private bool _isSynced;
    
    public async UniTask<bool> SyncTime()
    {
        try
        {
            var ntpTime = await FetchNTPTime(NTP_SERVER);
            _timeOffset = ntpTime - DateTime.UtcNow;
            _lastSyncTime = DateTime.UtcNow;
            _isSynced = true;
            return true;
        }
        catch (Exception ex)
        {
            GameLogger.LogError($"NTP sync failed: {ex.Message}");
            return false;
        }
    }
    
    public DateTime GetServerTime()
    {
        if (!_isSynced)
        {
            GameLogger.LogWarning("NTP not synced, using device time");
            return DateTime.UtcNow;
        }
        return DateTime.UtcNow + _timeOffset;
    }
}
```

### Maintenance Check Implementation

```csharp
public class MaintenanceChecker
{
    private const float CHECK_INTERVAL = 120f; // 2 minutes
    private float _timeSinceLastCheck;
    private bool _maintenanceWarningShown;
    
    public void Update(float deltaTime)
    {
        _timeSinceLastCheck += deltaTime;
        
        if (_timeSinceLastCheck >= CHECK_INTERVAL)
        {
            _timeSinceLastCheck = 0f;
            CheckMaintenance();
        }
    }
    
    private async void CheckMaintenance()
    {
        await _remoteConfigService.RefreshConfig<MaintenanceConfig>();
        var config = _remoteConfigService.GetConfig<MaintenanceConfig>();
        
        if (config.IsMaintenanceActive)
        {
            var serverTime = _ntpService.GetServerTime();
            var maintenanceStart = DateTimeOffset.FromUnixTimeSeconds(config.MaintenanceStartTimestamp).DateTime;
            var timeUntilMaintenance = maintenanceStart - serverTime;
            
            if (timeUntilMaintenance.TotalMinutes <= 10 && !_maintenanceWarningShown)
            {
                ShowMaintenanceWarning(timeUntilMaintenance);
                _maintenanceWarningShown = true;
            }
            
            if (serverTime >= maintenanceStart)
            {
                HandleMaintenanceStart();
            }
        }
    }
}
```

## Performance Considerations

1. **Caching**: Cache all configs locally to minimize Firebase calls
2. **Lazy Loading**: Load configs on-demand rather than all at once
3. **Batch Fetching**: Fetch multiple configs in single Firebase call
4. **Background Refresh**: Refresh configs in background without blocking gameplay
5. **NTP Sync**: Sync NTP time only on startup and every 6 hours
6. **Maintenance Checks**: Check every 2 minutes (not too frequent)

## Security Considerations

1. **Validation**: Always validate config data before use
2. **Sanitization**: Sanitize user-facing strings from config
3. **Version Checking**: Verify config version compatibility
4. **Fallback**: Always have safe fallback values
5. **Rate Limiting**: Limit Firebase fetch frequency to avoid quota issues
