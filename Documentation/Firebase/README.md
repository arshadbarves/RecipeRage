# Firebase Remote Config Setup Guide

## Overview
This folder contains example JSON files for all 7 configuration domains in RecipeRage. Use these to set up Firebase Remote Config parameters.

## Configuration Files

1. **GameSettings.json** - Game rules, currency rates, feature flags
2. **ShopConfig.json** - Shop items, categories, rotations, special offers
3. **CharacterConfig.json** - Character definitions with stats and abilities
4. **MapConfig.json** - Map definitions with rotation schedules
5. **SkinsConfig.json** - Skin definitions for characters
6. **MaintenanceConfig.json** - Maintenance mode configuration
7. **ForceUpdateConfig.json** - Platform-specific version requirements

## Firebase Console Setup

### Step 1: Create Firebase Project
1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click "Add project"
3. Enter project name: "RecipeRage"
4. Enable Google Analytics (optional)
5. Create project

### Step 2: Add Apps
1. Click "Add app" and select iOS
   - Bundle ID: `com.yourcompany.reciperage`
   - Download `GoogleService-Info.plist`
   - Place in `Assets/` folder

2. Click "Add app" and select Android
   - Package name: `com.yourcompany.reciperage`
   - Download `google-services.json`
   - Place in `Assets/` folder

### Step 3: Configure Remote Config Parameters

For each JSON file, create a parameter in Firebase Remote Config:

#### Parameter: GameSettings
- **Key**: `GameSettings`
- **Data type**: JSON
- **Default value**: Copy content from `GameSettings.json`

#### Parameter: ShopConfig
- **Key**: `ShopConfig`
- **Data type**: JSON
- **Default value**: Copy content from `ShopConfig.json`

#### Parameter: CharacterConfig
- **Key**: `CharacterConfig`
- **Data type**: JSON
- **Default value**: Copy content from `CharacterConfig.json`

#### Parameter: MapConfig
- **Key**: `MapConfig`
- **Data type**: JSON
- **Default value**: Copy content from `MapConfig.json`

#### Parameter: SkinsConfig
- **Key**: `SkinsConfig`
- **Data type**: JSON
- **Default value**: Copy content from `SkinsConfig.json`

#### Parameter: MaintenanceConfig
- **Key**: `MaintenanceConfig`
- **Data type**: JSON
- **Default value**: Copy content from `MaintenanceConfig.json`

#### Parameter: ForceUpdateConfig
- **Key**: `ForceUpdateConfig`
- **Data type**: JSON
- **Default value**: Copy content from `ForceUpdateConfig.json`

### Step 4: Set Up Conditions (Optional)

Create conditions for different platforms and environments:

#### Platform Conditions
1. **iOS_Platform**
   - Condition: `Platform == iOS`

2. **Android_Platform**
   - Condition: `Platform == Android`

#### Environment Conditions
1. **Development**
   - Condition: `App version matches regex: ^0\..*`

2. **Staging**
   - Condition: `App version matches regex: ^1\.0\..*-beta`

3. **Production**
   - Condition: `App version matches regex: ^1\.[1-9]\..*`

#### Combined Conditions
- **iOS_Production**: `iOS_Platform AND Production`
- **Android_Production**: `Android_Platform AND Production`
- **iOS_Development**: `iOS_Platform AND Development`
- **Android_Development**: `Android_Platform AND Development`

### Step 5: Publish Changes
1. Review all parameters
2. Click "Publish changes"
3. Confirm publication

## Unity Integration

### Install Firebase SDK
1. Download [Firebase Unity SDK](https://firebase.google.com/download/unity)
2. Import packages:
   - `FirebaseAnalytics.unitypackage`
   - `FirebaseRemoteConfig.unitypackage`

### Update FirebaseConfigProvider
Uncomment the Firebase code in `Assets/Scripts/Core/RemoteConfig/Providers/FirebaseConfigProvider.cs`:

```csharp
// Initialize Firebase
await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;

// Set config settings
var configSettings = new Firebase.RemoteConfig.ConfigSettings
{
    MinimumFetchInternalInMilliseconds = 3600000 // 1 hour
};
remoteConfig.SetConfigSettingsAsync(configSettings);

// Fetch and activate
await remoteConfig.FetchAsync();
await remoteConfig.ActivateAsync();

// Get config value
var jsonString = remoteConfig.GetValue(key).StringValue;
var config = JsonConvert.DeserializeObject<T>(jsonString);
```

## Testing

### Local Testing (Without Firebase)
The system will automatically fail to initialize Firebase and show errors. For local testing, you need Firebase SDK integrated.

### Firebase Testing
1. Build and run on device/emulator
2. Check logs for "Firebase provider initialized successfully"
3. Verify configs are loaded: "Fetched X configs from Firebase"
4. Test config updates by changing values in Firebase Console

## Maintenance Mode Testing

To test maintenance mode:
1. Update `MaintenanceConfig.json`:
   ```json
   {
     "isMaintenanceActive": true,
     "maintenanceStartTimestamp": 1705622400,
     "maintenanceEndTimestamp": 1705626000,
     "estimatedDurationMinutes": 60
   }
   ```
2. Publish changes in Firebase
3. App will show maintenance screen

## Force Update Testing

To test force update:
1. Update `ForceUpdateConfig.json`:
   ```json
   {
     "platformRequirements": [
       {
         "platform": "iOS",
         "minimumVersion": "2.0.0",
         "updateUrgency": "required"
       }
     ]
   }
   ```
2. Publish changes
3. App with version < 2.0.0 will show update prompt

## Rotation Testing

### Shop Rotation
Update timestamps in `ShopConfig.json` to current time:
```json
{
  "rotationPeriods": [
    {
      "startTimestamp": 1705276800,  // Use current Unix timestamp
      "endTimestamp": 1705881600,    // Use future Unix timestamp
      "featuredItemIds": ["item1", "item2"]
    }
  ]
}
```

### Map Rotation
Update timestamps in `MapConfig.json` similarly.

## Troubleshooting

### Firebase Not Initializing
- Check `google-services.json` and `GoogleService-Info.plist` are in correct location
- Verify package name/bundle ID matches Firebase project
- Check Firebase SDK is imported correctly

### Configs Not Loading
- Verify parameter keys match exactly (case-sensitive)
- Check JSON is valid (use JSON validator)
- Ensure changes are published in Firebase Console
- Check app has internet connection

### Validation Errors
- Each config has a `Validate()` method
- Check logs for validation error details
- Verify all required fields are present
- Check data types match model definitions

## Production Checklist

- [ ] Firebase project created
- [ ] iOS and Android apps added
- [ ] All 7 parameters configured
- [ ] Default values set
- [ ] Conditions created (if needed)
- [ ] Changes published
- [ ] Firebase SDK integrated in Unity
- [ ] FirebaseConfigProvider code uncommented
- [ ] Tested on iOS device
- [ ] Tested on Android device
- [ ] Maintenance mode tested
- [ ] Force update tested
- [ ] Rotation schedules tested

## Support

For issues or questions:
1. Check Unity console logs
2. Check Firebase Console > Remote Config > Fetch logs
3. Review `IMPLEMENTATION_SUMMARY.md` in RemoteConfig folder
4. Review `SIMPLIFIED_ARCHITECTURE.md` for system overview
