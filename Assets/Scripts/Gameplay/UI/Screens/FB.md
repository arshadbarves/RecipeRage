# Facebook Login Setup Guide

## ⚠️ Important: OpenID is NOT Supported

Facebook does **NOT** support OpenID Connect for gaming applications. You must use Facebook's official SDK and Access Token method.

## Prerequisites

1. Facebook Developer Account
2. Facebook App created at https://developers.facebook.com
3. Unity project with EOS Plugin installed

## Step-by-Step Setup

### 1. Install Facebook SDK for Unity

**Option A: Via Package Manager (Recommended)**
```
1. Open Package Manager (Window → Package Manager)
2. Click "+" → "Add package from git URL"
3. Enter: https://github.com/facebook/facebook-sdk-for-unity.git
4. Click "Add"
```

**Option B: Via Unity Asset Store**
```
1. Open Asset Store
2. Search for "Facebook SDK"
3. Download and import
```

### 2. Create Facebook App

1. Go to https://developers.facebook.com
2. Click "My Apps" → "Create App"
3. Select "Gaming" as app type
4. Fill in app details:
    - App Name: Your game name
    - Contact Email: Your email
5. Click "Create App"
6. Note your **App ID** (you'll need this)

### 3. Configure Facebook App

**Add Platforms:**

For **Android**:
1. Settings → Basic → Add Platform → Android
2. Enter Package Name (from Unity: Edit → Project Settings → Player → Android → Package Name)
3. Enter Class Name: `com.unity3d.player.UnityPlayerActivity`
4. Enable "Single Sign On"
5. Add Key Hashes (see Android section below)

For **iOS**:
1. Settings → Basic → Add Platform → iOS
2. Enter Bundle ID (from Unity: Edit → Project Settings → Player → iOS → Bundle Identifier)
3. Enable "Single Sign On"

For **Windows/Mac/Linux**:
1. Settings → Basic → Add Platform → Website
2. Enter Site URL: `http://localhost:8080/`
3. This allows testing in Unity Editor

### 4. Configure Unity Facebook Settings

1. In Unity: **Window → Facebook → Edit Settings**
2. Enter your **App ID**
3. Enter your **App Name**
4. Configure platform-specific settings:
    - **Android**: Check "Android Build Facebook Settings"
    - **iOS**: Check "iOS Build Facebook Settings"

### 5. Get Android Key Hash

**For Debug Build:**
```bash
# On Mac/Linux
keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore | openssl sha1 -binary | openssl base64

# On Windows
keytool -exportcert -alias androiddebugkey -keystore %HOMEPATH%\.android\debug.keystore | openssl sha1 -binary | openssl base64

# Default password: android
```

**For Release Build:**
```bash
keytool -exportcert -alias YOUR_RELEASE_KEY_ALIAS -keystore YOUR_RELEASE_KEY_PATH | openssl sha1 -binary | openssl base64
```

Add the generated hash to Facebook App Settings → Android → Key Hashes

### 6. Update LoginScreen.cs

Replace the Facebook login placeholder with actual implementation:

```csharp
using Facebook.Unity;

// In OnInitialize or Start
private void InitializeFacebook()
{
    if (!FB.IsInitialized)
    {
        FB.Init(() =>
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                Debug.Log("[LoginScreen] Facebook SDK initialized");
            }
            else
            {
                Debug.LogError("[LoginScreen] Failed to initialize Facebook SDK");
            }
        });
    }
    else
    {
        FB.ActivateApp();
    }
}

// Replace OnFacebookLoginClicked method
private void OnFacebookLoginClicked()
{
    if (_isLoggingIn) return;
    
    Debug.Log("[LoginScreen] Facebook login clicked");
    UpdateStatus("Connecting to Facebook...");
    
    _isLoggingIn = true;
    UpdateUI();
    
    // Check if Facebook SDK is initialized
    if (!FB.IsInitialized)
    {
        InitializeFacebook();
        UpdateStatus("Initializing Facebook...");
        UIManager.Instance.StartCoroutine(WaitForFacebookInit());
        return;
    }
    
    // Start Facebook login
    var permissions = new List<string>() { "public_profile", "email" };
    
    FB.LogInWithReadPermissions(permissions, (ILoginResult result) =>
    {
        if (result.Error != null)
        {
            Debug.LogError($"[LoginScreen] Facebook login error: {result.Error}");
            UpdateStatus($"Facebook login failed: {result.Error}");
            _isLoggingIn = false;
            UpdateUI();
            OnLoginFailed?.Invoke(result.Error);
        }
        else if (result.Cancelled)
        {
            Debug.Log("[LoginScreen] Facebook login cancelled");
            UpdateStatus("Login cancelled");
            _isLoggingIn = false;
            UpdateUI();
        }
        else if (FB.IsLoggedIn)
        {
            Debug.Log("[LoginScreen] Facebook login successful");
            UIManager.Instance.StartCoroutine(LoginWithFacebookToken());
        }
        else
        {
            Debug.LogError("[LoginScreen] Facebook login failed: Unknown error");
            UpdateStatus("Facebook login failed");
            _isLoggingIn = false;
            UpdateUI();
            OnLoginFailed?.Invoke("Unknown error");
        }
    });
}

private IEnumerator WaitForFacebookInit()
{
    float timeout = 10f;
    float elapsed = 0f;
    
    while (!FB.IsInitialized && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    if (FB.IsInitialized)
    {
        OnFacebookLoginClicked(); // Retry login
    }
    else
    {
        UpdateStatus("Failed to initialize Facebook");
        _isLoggingIn = false;
        UpdateUI();
    }
}

private IEnumerator LoginWithFacebookToken()
{
    UpdateStatus("Logging in with Facebook...");
    
    string token = AccessToken.CurrentAccessToken.TokenString;
    string userId = AccessToken.CurrentAccessToken.UserId;
    string displayName = $"FB_{userId.Substring(0, 8)}";
    
    bool loginCompleted = false;
    bool loginSuccess = false;
    string errorMessage = "";
    
    EOSManager.Instance.StartConnectLoginWithOptions(
        ExternalCredentialType.FacebookAccessToken,
        token,
        displayName,
        (Epic.OnlineServices.Connect.LoginCallbackInfo callbackInfo) =>
        {
            loginCompleted = true;
            
            if (callbackInfo.ResultCode == Result.Success)
            {
                loginSuccess = true;
                Debug.Log("[LoginScreen] EOS Facebook login successful");
            }
            else
            {
                errorMessage = $"EOS login failed: {callbackInfo.ResultCode}";
                Debug.LogError($"[LoginScreen] {errorMessage}");
            }
        }
    );
    
    // Wait for EOS login to complete
    float timeout = 10f;
    float elapsed = 0f;
    
    while (!loginCompleted && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    _isLoggingIn = false;
    UpdateUI();
    
    if (loginSuccess)
    {
        // Save login method for auto-login next time
        PlayerPrefs.SetString(PREF_LAST_LOGIN_METHOD, LOGIN_METHOD_FACEBOOK);
        PlayerPrefs.Save();
        Debug.Log("[LoginScreen] Saved Facebook as last login method");
        
        OnLoginSuccessful();
    }
    else
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            errorMessage = "Login timed out";
        }
        
        UpdateStatus(errorMessage);
        OnLoginFailed?.Invoke(errorMessage);
    }
}

// Update AttemptFacebookAutoLogin method
private IEnumerator AttemptFacebookAutoLogin()
{
    UpdateStatus("Checking Facebook session...");
    
    // Wait for Facebook SDK to initialize
    if (!FB.IsInitialized)
    {
        InitializeFacebook();
        
        float timeout = 5f;
        float elapsed = 0f;
        
        while (!FB.IsInitialized && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    // Check if user is logged in to Facebook
    if (FB.IsLoggedIn)
    {
        Debug.Log("[LoginScreen] Facebook session found, attempting auto-login");
        yield return LoginWithFacebookToken();
    }
    else
    {
        Debug.Log("[LoginScreen] No Facebook session found");
        UpdateStatus("Please login with Facebook");
    }
}
```

### 7. Test Facebook Login

**In Unity Editor:**
1. Play the game
2. Click "Continue with Facebook"
3. Browser window opens with Facebook login
4. Login with your Facebook account
5. Authorize the app
6. Should return to Unity and login successfully

**On Device:**
1. Build and deploy to device
2. Ensure Facebook app is installed (for native login)
3. Click "Continue with Facebook"
4. Facebook native login appears
5. Login and authorize
6. Should login successfully

### 8. Handle Logout

Add logout functionality:

```csharp
public void LogoutFromFacebook()
{
    if (FB.IsLoggedIn)
    {
        FB.LogOut();
        Debug.Log("[LoginScreen] Logged out from Facebook");
    }
    
    // Clear EOS session
    ClearSavedLoginMethod();
    var productUserId = EOSManager.Instance.GetProductUserId();
    if (productUserId != null && productUserId.IsValid())
    {
        EOSManager.Instance.ClearConnectId(productUserId);
    }
}
```

## EOS Configuration

### Facebook as External Credential

In your EOS Developer Portal:

1. Go to Product Settings → Identity Providers
2. Add Facebook as an identity provider
3. Enter your Facebook App ID
4. Enter your Facebook App Secret (from Facebook App Settings → Basic)
5. Save configuration

## Permissions

### Required Permissions

- `public_profile` - Basic profile information
- `email` - User's email address (optional but recommended)

### Requesting Additional Permissions

```csharp
var permissions = new List<string>() 
{ 
    "public_profile", 
    "email",
    "user_friends" // If you need friends list
};
```

## Testing Checklist

- [ ] Facebook SDK initializes successfully
- [ ] Login button opens Facebook login dialog
- [ ] Can login with Facebook account
- [ ] EOS receives Facebook token and logs in
- [ ] PlayerPrefs saves "Facebook" as login method
- [ ] On second launch, auto-login works
- [ ] Can logout and login again
- [ ] Works on target platform (Android/iOS)

## Troubleshooting

### "Invalid Key Hash" Error (Android)

**Problem**: Facebook login fails with key hash error

**Solution**:
1. Generate key hash using keytool command above
2. Add to Facebook App Settings → Android → Key Hashes
3. For release builds, use your release keystore

### "App Not Setup" Error

**Problem**: Facebook login shows "App Not Setup" message

**Solution**:
1. Check Facebook App is in "Development" or "Live" mode
2. Add your Facebook account as a test user (if in Development mode)
3. Verify App ID in Unity matches Facebook App ID

### Facebook SDK Not Initializing

**Problem**: `FB.IsInitialized` is always false

**Solution**:
1. Check Facebook Settings in Unity (Window → Facebook → Edit Settings)
2. Verify App ID is correct
3. Check console for initialization errors
4. Ensure Facebook SDK is properly imported

### Token Expired Error

**Problem**: EOS login fails with expired token

**Solution**:
1. Facebook tokens expire after ~60 days
2. User needs to login again
3. Implement token refresh logic
4. Check Facebook SDK documentation for token management

### Auto-Login Not Working

**Problem**: User has to login every time

**Solution**:
1. Check `FB.IsLoggedIn` returns true on second launch
2. Verify PlayerPrefs is saving correctly
3. Ensure Facebook SDK is initialized before checking login status
4. Check Facebook app permissions haven't been revoked

## Platform-Specific Notes

### Android

- Requires Facebook App installed for native login (fallback to web if not)
- Key hash must match your keystore
- Test with both debug and release builds

### iOS

- Requires Facebook App installed for native login
- Bundle ID must match in Facebook settings
- Add Facebook URL schemes to Info.plist (done automatically by SDK)

### Windows/Mac/Linux

- Uses web-based login
- Requires localhost URL in Facebook settings
- Good for testing in Unity Editor

## Security Best Practices

1. **Never commit App Secret** to version control
2. **Use different Facebook Apps** for development and production
3. **Validate tokens server-side** if you have a backend
4. **Handle token expiration** gracefully
5. **Request minimum permissions** needed
6. **Implement proper logout** to clear tokens

## Additional Resources

- [Facebook SDK for Unity Documentation](https://developers.facebook.com/docs/unity)
- [EOS Connect Interface Documentation](https://dev.epicgames.com/docs/game-services/eos-connect-interface)
- [Facebook Login Best Practices](https://developers.facebook.com/docs/facebook-login/best-practices)
- [EOS Identity Providers](https://dev.epicgames.com/docs/epic-account-services/auth/auth-interface#identity-providers)
