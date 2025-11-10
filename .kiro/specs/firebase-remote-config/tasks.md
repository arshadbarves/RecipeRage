# Implementation Plan

- [x] 1. Set up core infrastructure and interfaces
  - Create IRemoteConfigService interface with initialization, config access, and event methods
  - Create IConfigModel interface with ConfigKey, Version, Validate, and LastModified properties
  - Create IConfigProvider interface with Initialize, FetchConfig, and IsAvailable methods
  - Create INTPTimeService interface with SyncTime, GetServerTime, and time offset methods
  - Create ConfigProviderType enum (Firebase, Local)
  - Create ConfigHealthStatus enum (Healthy, Degraded, Failed)
  - _Requirements: 1.1, 1.5, 10.1, 10.2, 12.1, 12.2_

- [x] 2. Implement NTP time synchronization service
  - Create NTPTimeService class implementing INTPTimeService
  - Implement SyncTime method to connect to Google NTP server (time.google.com)
  - Implement GetServerTime method using calculated time offset
  - Add retry logic with exponential backoff for failed sync attempts
  - Add periodic re-sync every 6 hours during active gameplay
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

- [x] 3. Create configuration model classes
  - [ ] 3.1 Create GameSettingsConfig model
    - Define properties for global game rules, currency rates, and feature flags
    - Implement Validate method for score multipliers and player counts
    - Add JSON serialization attributes
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 10.2, 10.3_
  
  - [x] 3.2 Create ShopConfig model with rotation support
    - Define ShopCategory, ShopItem, ShopRotationSchedule, and RotationPeriod classes
    - Add SpecialOffer class with timestamp-based availability
    - Implement Validate method for categories and items
    - Add NTP timestamp fields for rotation calculations
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_
  
  - [x] 3.3 Create CharacterConfig model
    - Define CharacterDefinition class with stats, abilities, and unlock requirements
    - Add ability parameters as JSON string field
    - Implement Validate method for character list
    - Include asset addresses for prefabs and icons
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [x] 3.4 Create MapConfig model with individual rotation
    - Define MapDefinition class with match duration, trophy rewards, and rotation config
    - Add MapRotationConfig class with per-map rotation duration
    - Add MapRotationSchedule and MapRotationPeriod classes
    - Implement Validate method for map list
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  
  - [x] 3.5 Create SkinsConfig model
    - Define SkinDefinition class matching existing Skins.json structure
    - Add properties for character ID, rarity, unlock cost, and asset addresses
    - Implement Validate method for skins list
    - Ensure backward compatibility with existing SkinsService
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5_
  
  - [x] 3.6 Create MaintenanceConfig model
    - Define properties for maintenance status, timestamps, and messages
    - Implement Validate method checking timestamp consistency
    - Add estimated duration field
    - _Requirements: 7.1, 7.4_
  
  - [x] 3.7 Create ForceUpdateConfig model
    - Define PlatformVersionRequirement class for iOS and Android
    - Add UpdateUrgency enum (Optional, Recommended, Required)
    - Implement Validate method for platform requirements
    - Include store URLs for each platform
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [x] 4. Implement Firebase config provider
  - Create FirebaseConfigProvider class implementing IConfigProvider
  - Implement Initialize method to set up Firebase Remote Config SDK
  - Implement FetchConfig method to retrieve configuration from Firebase
  - Add platform and environment detection for Firebase conditions
  - Implement FetchAllConfigs method for batch fetching
  - Add error handling with detailed logging
  - _Requirements: 1.1, 2.1, 2.2, 2.3, 2.4, 2.5, 15.1, 15.2_

- [x] 5. Implement local ScriptableObject provider
  - Create LocalConfigProvider class implementing IConfigProvider
  - Implement Initialize method to load ScriptableObjects from Resources
  - Implement FetchConfig method to return ScriptableObject data
  - Add validation for ScriptableObject existence
  - _Requirements: 1.3, 1.4, 16.1, 16.2, 16.4, 16.5_

- [x] 6. Create ScriptableObject classes for each config domain
  - Create GameSettingsConfigSO ScriptableObject
  - Create ShopConfigSO ScriptableObject
  - Create CharacterConfigSO ScriptableObject
  - Create MapConfigSO ScriptableObject
  - Create SkinsConfigSO ScriptableObject
  - Create MaintenanceConfigSO ScriptableObject
  - Create ForceUpdateConfigSO ScriptableObject
  - Add CreateAssetMenu attributes for Unity editor
  - _Requirements: 16.1, 16.2, 16.5_

- [x] 7. Implement RemoteConfigService core
  - Create RemoteConfigService class implementing IRemoteConfigService
  - Implement Initialize method with provider selection logic
  - Implement GetConfig and TryGetConfig methods with type safety
  - Add configuration storage dictionary
  - Implement SetProvider method for manual provider override
  - Add health status tracking and events
  - _Requirements: 1.1, 1.3, 1.4, 1.5, 10.1, 10.4, 11.1, 11.2, 15.4_

- [ ] 8. Implement configuration refresh and update notifications
  - Implement RefreshConfig methods (all configs and specific config)
  - Add OnConfigUpdated event publishing
  - Add OnSpecificConfigUpdated event with type parameter
  - Implement configuration change detection
  - Add background refresh capability
  - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

- [ ] 9. Implement configuration validation and error handling
  - Create ConfigErrorHandler class for centralized error handling
  - Implement validation for all config models
  - Add HandleFetchError method with user-friendly error display
  - Add HandleValidationError method with fallback logic
  - Implement "No Internet Connection" popup integration
  - Add configuration health status updates
  - _Requirements: 1.2, 15.1, 15.2, 15.3, 15.4, 15.5_

- [ ] 10. Create JSON export editor tool
  - Create RemoteConfigExportTool editor script
  - Implement ExportToJSON method to convert ScriptableObjects to JSON
  - Add menu item "Tools/Remote Config/Export to JSON"
  - Create export directory structure
  - Add export validation and error reporting
  - _Requirements: 16.3_

- [ ] 11. Integrate RemoteConfigService with GameBootstrap
  - Register RemoteConfigService in ServiceContainer
  - Initialize RemoteConfigService before other services
  - Add NTPTimeService initialization
  - Configure provider based on build settings
  - Add error handling for initialization failures
  - _Requirements: 1.1, 1.2, 1.5_

- [ ] 12. Implement maintenance mode checker
  - Create MaintenanceChecker class with periodic checking
  - Implement Update method with 2-minute check interval
  - Add CheckMaintenance method using NTP time
  - Implement 10-minute warning notification
  - Add HandleMaintenanceStart method to prevent new matches
  - Integrate with GameStateManager for graceful match completion
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ] 13. Implement force update checker
  - Create ForceUpdateChecker class
  - Implement version comparison logic
  - Add platform-specific version checking (iOS/Android)
  - Create force update popup with store link
  - Integrate with GameBootstrap initialization
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [ ] 14. Migrate SkinsService to use RemoteConfigService
  - Update SkinsService to fetch from RemoteConfigService
  - Remove direct JSON file loading
  - Add fallback to embedded Skins.json if config unavailable
  - Subscribe to SkinsConfig update events
  - Update skin data when configuration changes
  - _Requirements: 13.2, 13.3, 13.4, 13.5_

- [ ] 15. Create shop rotation calculator
  - Create ShopRotationCalculator class
  - Implement GetCurrentRotation method using NTP time
  - Add rotation period matching logic
  - Implement featured items filtering
  - Add special offers time validation
  - _Requirements: 4.3, 4.4, 4.5_

- [ ] 16. Create map rotation calculator
  - Create MapRotationCalculator class
  - Implement GetActiveMap method using NTP time
  - Add per-map rotation schedule checking
  - Implement map availability filtering
  - Add rotation period transition handling
  - _Requirements: 6.3, 6.4, 6.5_

- [ ] 17. Update existing services to use RemoteConfigService
  - Update GameModeService to use GameSettingsConfig
  - Update CharacterService to use CharacterConfig
  - Create MapService to use MapConfig
  - Update shop system to use ShopConfig
  - Add configuration change event subscriptions
  - _Requirements: 3.5, 5.5, 11.4, 11.5_

- [ ] 18. Create "No Internet Connection" popup
  - Create NoInternetPopup UI component
  - Add retry button functionality
  - Implement popup display logic in UIService
  - Add popup data model with retry callback
  - Style popup according to design system
  - _Requirements: 1.2_

- [ ] 19. Set up Firebase project and Unity SDK
  - Create Firebase project in Firebase Console
  - Add iOS and Android apps to Firebase project
  - Download google-services.json and GoogleService-Info.plist
  - Import Firebase Unity SDK package
  - Configure Firebase settings in Unity
  - Set up Remote Config parameters in Firebase Console
  - _Requirements: 1.1, 2.1, 2.2_

- [ ] 20. Configure Firebase Remote Config parameters
  - Create parameter for each config domain (GameSettings, ShopConfig, etc.)
  - Set up platform conditions (iOS, Android)
  - Set up environment conditions (production, development, staging)
  - Create combined conditions (iOS_Production, Android_Development, etc.)
  - Set default values for each parameter
  - Test parameter fetching with different conditions
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [ ]* 21. Create unit tests for configuration models
  - Write tests for GameSettingsConfig validation
  - Write tests for ShopConfig validation
  - Write tests for CharacterConfig validation
  - Write tests for MapConfig validation
  - Write tests for MaintenanceConfig validation
  - Write tests for ForceUpdateConfig validation
  - _Requirements: 10.3, 10.4, 15.1_

- [ ]* 22. Create unit tests for RemoteConfigService
  - Write tests for Initialize method
  - Write tests for GetConfig and TryGetConfig methods
  - Write tests for SetProvider method
  - Write tests for RefreshConfig methods
  - Write tests for configuration update events
  - Write tests for error handling
  - _Requirements: 1.1, 1.3, 1.4, 1.5, 15.1, 15.2_

- [ ]* 23. Create integration tests
  - Write end-to-end test for Firebase fetch
  - Write test for local provider fallback
  - Write test for rotation calculations
  - Write test for maintenance mode flow
  - Write test for force update flow
  - Write test for configuration migration
  - _Requirements: 1.1, 1.2, 4.4, 6.4, 7.5, 8.3_

- [ ] 24. Create documentation and examples
  - Write README for RemoteConfig system
  - Create example ScriptableObjects for each config domain
  - Document Firebase setup process
  - Create guide for adding new config domains
  - Document JSON export process
  - Add code examples for accessing configuration
  - _Requirements: 12.3, 12.4, 16.3_
