# Cache Management on Logout

## Overview

When a user logs out, the system **selectively clears user-specific cache** while preserving device-level settings and static game data. This approach balances security, performance, and user experience.

## What Gets Cleared on Logout

### ✅ User-Specific Data (Cleared)

1. **Cached Player Progress** (`_cachedProgress`)
   - Level progress, unlocks, achievements
   - Cleared from memory, preserved on disk

2. **Cached Player Stats** (`_cachedStats`)
   - Match history, win/loss records, rankings
   - Cleared from memory, preserved on disk

3. **Cloud Storage Cache** (EOS)
   - Cached cloud files from previous user
   - Prevents data leakage between accounts

4. **Sync Status**
   - Cloud sync state for progress and stats
   - Reset to allow fresh sync for next user

5. **Authentication Tokens**
   - Login method cleared
   - EOS session cleared

### ❌ Device-Level Data (Preserved)

1. **Game Settings** (`_cachedSettings`)
   - Audio volume, graphics quality, language
   - Device-specific, not user-specific
   - Stays cached for instant access

2. **Static Game Data**
   - Audio clips, UI templates (UXML/USS)
   - Game modes, character data, recipes
   - Expensive to reload, not user-specific

3. **Addressables Cache**
   - Asset bundles and downloaded content
   - Shared across all users

4. **Storage Providers**
   - Local and cloud storage providers stay initialized
   - Ready for next login without re-initialization

## Implementation

### SaveService.OnUserLoggedOut()

```csharp
public void OnUserLoggedOut()
{
    // Clear user-specific cached data
    _cachedProgress = null;
    _cachedStats = null;
    
    // Keep device settings cached
    // _cachedSettings is preserved
    
    // Clear sync status for user data
    _syncStatus["progress.json"] = new SyncStatus();
    _syncStatus["stats.json"] = new SyncStatus();
    
    // Notify cloud provider
    if (_cloudProvider is EOSCloudStorageProvider eosProvider)
    {
        eosProvider.OnUserLoggedOut();
    }
}
```

### EOSCloudStorageProvider.OnUserLoggedOut()

```csharp
public void OnUserLoggedOut()
{
    // Clear cached file data from EOS storage service
    var cachedData = _eosStorage.GetLocallyCachedData();
    cachedData?.Clear();
    
    // Mark as not initialized
    _isInitialized = false;
}
```

### AuthenticationService.LogoutAsync()

```csharp
public async UniTask LogoutAsync()
{
    // Clear saved login method
    settings.LastLoginMethod = "";
    _saveService.SaveSettings(settings);
    
    // Clear user-specific cache
    _saveService.OnUserLoggedOut();
    
    // Clear all UI screens and history
    _uiService.HideAllScreens(animate: false);
    _uiService.ClearHistory();
    
    // Clear EOS session
    EOSManager.Instance.ClearConnectId(productUserId);
    
    // Trigger logout complete event
    // GameBootstrap.HandleLogout() will perform full reboot
    OnLogoutComplete?.Invoke();
}
```

### GameBootstrap.HandleLogout()

The GameBootstrap performs a **full reboot** on logout:

```csharp
private async void HandleLogout()
{
    // Dispose all services
    Services?.Dispose();
    
    // Mark as uninitialized
    _isInitialized = false;
    
    // Reinitialize everything from scratch
    Services = new ServiceContainer();
    InitializeFoundation();
    InitializeCoreServices();
    
    // Show login screen
    Services.UIService.ShowScreen(UIScreenType.Login);
    Services.EventBus.Subscribe<Events.LoginSuccessEvent>(OnLoginSuccess);
}
```

This ensures a **completely fresh state** for the next login.

## Additional Methods

### ClearUserCache()

For "Clear Cache" button in settings or troubleshooting:

```csharp
public void ClearUserCache()
{
    // Clear cached user data (will reload from disk)
    _cachedProgress = null;
    _cachedStats = null;
    
    // Clear cloud cache
    eosProvider.OnUserLoggedOut();
}
```

### DeleteAllData()

For "Delete Account" or "Reset Progress":

```csharp
public void DeleteAllData()
{
    // Delete files from disk
    _localProvider.Delete("settings.json");
    _localProvider.Delete("progress.json");
    _localProvider.Delete("stats.json");
    
    // Delete from cloud
    _cloudProvider.Delete("progress.json");
    _cloudProvider.Delete("stats.json");
    
    // Reset cached data
    _cachedSettings = new GameSettingsData();
    _cachedProgress = new PlayerProgressData();
    _cachedStats = new PlayerStatsData();
}
```

## Benefits

### Performance
- No expensive reloading of audio, UI, or game data
- Instant logout → login transition
- Fast account switching

### Security
- User-specific data cleared from memory
- No data leakage between accounts
- Authentication tokens properly cleared

### User Experience
- Device settings preserved (volume, graphics)
- No loading screens between logout/login
- Smooth account switching

## Usage Examples

### Normal Logout
```csharp
await authService.LogoutAsync();
// User data cleared, device settings preserved
// Ready for next login
```

### Clear Cache (Troubleshooting)
```csharp
saveService.ClearUserCache();
// Clears cache, data reloads from disk
```

### Delete Account
```csharp
saveService.DeleteAllData();
// Deletes all data from disk and cloud
// Fresh start
```

## Testing

To verify proper cache clearing:

1. Login as User A
2. Check cached data exists
3. Logout
4. Verify user cache cleared
5. Verify settings preserved
6. Login as User B
7. Verify User A's data not accessible

## Notes

- **Never** clear static game data on logout (performance impact)
- **Always** clear user-specific data (security requirement)
- **Preserve** device settings (better UX)
- **Keep** storage providers initialized (faster next login)
