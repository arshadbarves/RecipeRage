# Firebase Remote Config - Quick Start

## 5-Minute Setup

### 1. Firebase Console (2 minutes)
```
1. Go to console.firebase.google.com
2. Create project "RecipeRage"
3. Add iOS app (download GoogleService-Info.plist)
4. Add Android app (download google-services.json)
```

### 2. Unity Setup (1 minute)
```
1. Place GoogleService-Info.plist in Assets/
2. Place google-services.json in Assets/
3. Import Firebase Unity SDK packages
```

### 3. Configure Parameters (2 minutes)
```
In Firebase Console > Remote Config:

For each JSON file in this folder:
1. Click "Add parameter"
2. Key: [filename without .json]
3. Data type: JSON
4. Default value: [paste JSON content]
5. Click "Save"

Repeat for all 7 files:
- GameSettings
- ShopConfig
- CharacterConfig
- MapConfig
- SkinsConfig
- MaintenanceConfig
- ForceUpdateConfig

6. Click "Publish changes"
```

### 4. Enable Firebase in Code
```csharp
// In FirebaseConfigProvider.cs, uncomment lines 41-44 and 73-75
// That's it!
```

## Test It Works

Build and run. Check logs for:
```
✅ "Firebase provider initialized successfully"
✅ "Fetched 7 configs from Firebase"
✅ "RemoteConfigService initialized successfully"
```

## Common Issues

**"Firebase provider failed to initialize"**
- Check google-services.json and GoogleService-Info.plist are in Assets/
- Verify package name matches Firebase project

**"No configurations fetched"**
- Ensure all 7 parameters are created in Firebase Console
- Click "Publish changes" in Firebase Console
- Check internet connection

**"Config validation failed"**
- Verify JSON is valid (use jsonlint.com)
- Check all required fields are present

## Next Steps

- Update config values in Firebase Console
- Set up platform conditions (iOS/Android)
- Configure rotation schedules with real timestamps
- Test maintenance mode
- Test force update

## Useful Links

- Firebase Console: https://console.firebase.google.com/
- Firebase Unity SDK: https://firebase.google.com/download/unity
- JSON Validator: https://jsonlint.com/
- Unix Timestamp Converter: https://www.unixtimestamp.com/

## Example: Update Shop Items

1. Go to Firebase Console > Remote Config
2. Find "ShopConfig" parameter
3. Click "Edit"
4. Modify JSON (add/remove items)
5. Click "Save"
6. Click "Publish changes"
7. In game, call `remoteConfigService.RefreshConfig()`
8. New items appear!

## Example: Enable Maintenance Mode

1. Edit "MaintenanceConfig" in Firebase
2. Set `"isMaintenanceActive": true`
3. Set timestamps (use current time + 10 minutes)
4. Publish changes
5. Game will show maintenance warning at 10 minutes
6. Game will block at maintenance time

Done! 🎉
