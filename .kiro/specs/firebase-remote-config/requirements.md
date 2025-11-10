# Requirements Document

## Introduction

This document defines the requirements for implementing a Firebase Remote Config system for RecipeRage. The system will enable dynamic configuration of game settings, shop items, characters, maps, maintenance mode, and force updates without requiring app updates. The configuration will support environment-based conditions (production/development) and provide a flexible, extensible architecture following SOLID principles.

## Glossary

- **RemoteConfigService**: The core service that fetches and manages remote configuration data from Firebase
- **ConfigProvider**: Interface for fetching configuration data from different sources (Firebase, local fallback)
- **ConfigCache**: Local storage mechanism for caching remote configuration to enable offline functionality
- **ConfigModel**: Data structure representing a specific configuration domain (e.g., GameSettings, ShopConfig)
- **Environment**: Deployment context (production, development, staging) used for conditional configuration
- **Rotation**: Time-based cycling of content (e.g., featured shop items, active maps)
- **MaintenanceMode**: System state that prevents gameplay and displays maintenance message
- **ForceUpdate**: Mechanism to require users to update the app before continuing
- **Firebase**: Google's Backend-as-a-Service platform providing Remote Config functionality

## Requirements

### Requirement 1: Core Remote Config Service

**User Story:** As a developer, I want a centralized service to fetch and manage remote configuration, so that I can dynamically update game settings without app updates

#### Acceptance Criteria

1. WHEN THE RemoteConfigService initializes, THE RemoteConfigService SHALL fetch all configuration data from Firebase Remote Config
2. WHEN Firebase Remote Config is unavailable, THE UIService SHALL display "No Internet Connection" popup with retry option
3. THE RemoteConfigService SHALL support manual provider override to use local ScriptableObjects for testing
4. WHERE provider is set to local, THE RemoteConfigService SHALL load configuration from ScriptableObjects instead of Firebase
5. THE RemoteConfigService SHALL expose configuration through type-safe interfaces for each config domain

### Requirement 2: Environment and Platform-Based Configuration

**User Story:** As a developer, I want different configurations for production and development environments per platform, so that I can test features without affecting live users

#### Acceptance Criteria

1. THE RemoteConfigService SHALL detect the current platform (iOS, Android) and environment (production, development, staging)
2. WHEN fetching configuration, THE RemoteConfigService SHALL apply platform and environment-specific conditions
3. THE RemoteConfigService SHALL support Firebase Remote Config conditions based on platform and environment parameters
4. WHERE platform is iOS AND environment is development, THE RemoteConfigService SHALL use iOS development configuration
5. WHERE platform is Android AND environment is production, THE RemoteConfigService SHALL use Android production configuration

### Requirement 3: Game Settings Configuration

**User Story:** As a game designer, I want to configure game settings remotely, so that I can balance gameplay without app updates

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide GameSettingsConfig containing global gameplay parameters
2. THE GameSettingsConfig SHALL include score multipliers, difficulty settings, and global game rules
3. THE GameSettingsConfig SHALL include currency reward rates and progression parameters
4. THE GameSettingsConfig SHALL include feature flags for enabling/disabling game features
5. WHEN GameSettingsConfig changes, THE RemoteConfigService SHALL notify subscribed services

### Requirement 4: Shop Configuration with NTP-Based Rotation

**User Story:** As a product manager, I want to configure shop items and featured rotations remotely, so that I can create dynamic shopping experiences

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide ShopConfig containing all shop items and categories
2. THE ShopConfig SHALL include item pricing, availability, and purchase limits
3. THE ShopConfig SHALL support time-based rotation schedules for featured items using NTP server time
4. WHEN rotation schedule changes based on NTP time, THE ShopConfig SHALL update featured items automatically
5. THE ShopConfig SHALL support special offers with start and end timestamps validated against NTP server time

### Requirement 5: Character Configuration

**User Story:** As a game designer, I want to configure character stats and abilities remotely, so that I can balance characters without app updates

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide CharacterConfig containing all character definitions
2. THE CharacterConfig SHALL include character stats, abilities, and unlock requirements
3. THE CharacterConfig SHALL support enabling/disabling characters dynamically
4. THE CharacterConfig SHALL include character balance parameters (speed, strength, cooldowns)
5. WHEN CharacterConfig changes, THE CharacterService SHALL update character data

### Requirement 6: Map Configuration with Individual Rotation Schedules

**User Story:** As a game designer, I want to configure available maps with individual rotation schedules, so that I can keep the game fresh and engage players

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide MapConfig containing all map definitions
2. THE MapConfig SHALL include map metadata, match duration, trophy rewards, and game mode compatibility
3. THE MapConfig SHALL support individual rotation schedules per map using NTP server time
4. WHEN map rotation period ends based on NTP time, THE MapConfig SHALL activate/deactivate maps according to their individual schedules
5. THE MapConfig SHALL support seasonal or event-based map availability with different rotation durations per map

### Requirement 7: Graceful Maintenance Mode

**User Story:** As an operations engineer, I want to enable maintenance mode remotely with graceful handling, so that I can perform server maintenance without disrupting active matches

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide MaintenanceConfig indicating maintenance status and scheduled start time
2. THE RemoteConfigService SHALL check MaintenanceConfig periodically (every 2 minutes) during gameplay
3. WHEN MaintenanceConfig indicates upcoming maintenance, THE UIService SHALL display countdown notification to players
4. WHEN maintenance is scheduled within 10 minutes, THE GameStateManager SHALL prevent new match creation
5. WHEN maintenance starts AND player is in active match, THE GameStateManager SHALL allow match completion but prevent next match

### Requirement 8: Force Update Mechanism

**User Story:** As a product manager, I want to force users to update the app, so that I can ensure all users have critical updates

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide ForceUpdateConfig containing minimum required version
2. WHEN current app version is below minimum required version, THE GameBootstrap SHALL prevent gameplay
3. WHEN force update is required, THE UIService SHALL display update prompt with store link
4. THE ForceUpdateConfig SHALL support platform-specific version requirements (iOS, Android)
5. THE ForceUpdateConfig SHALL include update message and urgency level

### Requirement 9: NTP Time Synchronization

**User Story:** As a developer, I want reliable time synchronization using NTP server, so that rotation schedules work correctly regardless of device time settings

#### Acceptance Criteria

1. THE RemoteConfigService SHALL synchronize time with Google NTP server on initialization
2. THE RemoteConfigService SHALL use NTP server time for all rotation calculations
3. WHEN NTP synchronization fails, THE RemoteConfigService SHALL retry with exponential backoff
4. THE RemoteConfigService SHALL cache NTP time offset for offline calculations
5. THE RemoteConfigService SHALL re-synchronize NTP time every 6 hours during active gameplay

### Requirement 10: Type-Safe Configuration Models

**User Story:** As a developer, I want type-safe configuration models, so that I can access configuration without runtime errors

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide strongly-typed configuration models for each domain
2. THE configuration models SHALL use C# classes with proper serialization attributes
3. THE RemoteConfigService SHALL validate configuration data against model schemas
4. WHEN configuration data is invalid, THE RemoteConfigService SHALL log errors and use fallback values
5. THE configuration models SHALL support versioning for backward compatibility

### Requirement 11: Configuration Change Notifications

**User Story:** As a developer, I want to be notified when configuration changes, so that I can update game systems dynamically

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide events for configuration change notifications
2. WHEN configuration is fetched successfully, THE RemoteConfigService SHALL publish ConfigUpdated event
3. THE RemoteConfigService SHALL provide domain-specific change events (e.g., ShopConfigChanged)
4. THE services SHALL subscribe to relevant configuration change events
5. WHEN configuration changes, THE subscribed services SHALL update their internal state

### Requirement 12: Extensible Configuration Architecture

**User Story:** As a developer, I want an extensible configuration system, so that I can add new configuration domains easily

#### Acceptance Criteria

1. THE RemoteConfigService SHALL use interface-based design for configuration providers
2. THE configuration system SHALL support adding new config domains without modifying existing code
3. THE RemoteConfigService SHALL use factory pattern for creating config model instances
4. THE configuration models SHALL implement common IConfigModel interface
5. THE RemoteConfigService SHALL support custom configuration parsers for complex data types

### Requirement 13: Skins Configuration Migration

**User Story:** As a developer, I want to migrate existing Skins.json to remote config, so that I can manage skins dynamically

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide SkinsConfig containing all skin definitions
2. THE SkinsConfig SHALL maintain compatibility with existing Skins.json structure
3. THE SkinsService SHALL fetch skin data from RemoteConfigService instead of local JSON
4. THE SkinsConfig SHALL support adding/removing skins without app updates
5. WHEN SkinsConfig is unavailable, THE SkinsService SHALL fallback to embedded Skins.json

### Requirement 14: Shop Items Configuration Migration

**User Story:** As a developer, I want to migrate existing ShopItems.json to remote config, so that I can manage shop items dynamically

#### Acceptance Criteria

1. THE RemoteConfigService SHALL provide ShopItemsConfig containing all shop item definitions
2. THE ShopItemsConfig SHALL maintain compatibility with existing ShopItems.json structure
3. THE shop system SHALL fetch item data from RemoteConfigService instead of local JSON
4. THE ShopItemsConfig SHALL support dynamic pricing and availability changes
5. WHEN ShopItemsConfig is unavailable, THE shop system SHALL fallback to embedded ShopItems.json

### Requirement 15: Configuration Validation and Error Handling

**User Story:** As a developer, I want robust error handling for configuration, so that invalid config doesn't break the game

#### Acceptance Criteria

1. THE RemoteConfigService SHALL validate all fetched configuration data
2. WHEN configuration validation fails, THE RemoteConfigService SHALL log detailed error messages
3. WHEN configuration is invalid, THE RemoteConfigService SHALL use last known good configuration
4. THE RemoteConfigService SHALL provide configuration health status (healthy, degraded, failed)
5. THE RemoteConfigService SHALL expose configuration errors through diagnostic interface

### Requirement 16: ScriptableObject-Based Configuration for Development

**User Story:** As a developer, I want to edit configuration as ScriptableObjects locally, so that I can test and export to Firebase Remote Config

#### Acceptance Criteria

1. THE configuration system SHALL provide ScriptableObject classes for each config domain
2. THE ScriptableObjects SHALL be editable in Unity Inspector for local development
3. THE ScriptableObjects SHALL provide "Export to JSON" functionality for Firebase Remote Config upload
4. WHERE environment is development, THE RemoteConfigService SHALL load configuration from ScriptableObjects
5. THE ScriptableObjects SHALL maintain identical structure to Firebase Remote Config JSON format
