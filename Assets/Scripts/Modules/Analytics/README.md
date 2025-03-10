# Analytics System

A modular analytics system for RecipeRage with Firebase Analytics integration.

## Overview

The Analytics system provides a structured way to track user behavior, game events, and performance metrics throughout the application. It supports multiple analytics providers through a common interface, with Firebase Analytics as the primary implementation.

Key features:
- Event tracking with structured parameters
- User property tracking
- Session management
- Revenue reporting
- Custom event definitions
- Privacy controls
- Offline data collection
- Batch processing
- Multiple provider support

## Architecture

The Analytics system follows a modular design with clear separation of concerns:

```
Assets/Scripts/Modules/Analytics/
│
├── Interfaces/           # Interface definitions
│   └── IAnalyticsService.cs  # Main analytics interface
│
├── Core/                 # Core implementations
│   ├── AnalyticsService.cs   # Base analytics service
│   └── FirebaseAnalyticsService.cs  # Firebase implementation
│
├── Data/                 # Data models and constants
│   ├── AnalyticsEventTypes.cs  # Standard event definitions
│   └── AnalyticsParameters.cs  # Standard parameter definitions
│
├── Providers/            # Provider-specific implementations
│   └── FirebaseProvider/     # Firebase specific code
│
├── Utils/                # Utility classes
│   └── AnalyticsConsent.cs   # User consent management
│
└── AnalyticsHelper.cs    # Static helper for easy access
```

## Getting Started

### Prerequisites

1. Set up Firebase for your project:
   - Create a Firebase project at [firebase.google.com](https://firebase.google.com)
   - Add your app to the Firebase project
   - Download the `google-services.json` (Android) or `GoogleService-Info.plist` (iOS) file
   - Place it in the appropriate location in your Unity project
   
2. Import the Firebase SDK:
   - Add the Firebase Analytics package to your project
   - Configure Firebase in Unity

### Initialization

Initialize the analytics system early in your application:

```csharp
// Initialize analytics with default settings
AnalyticsHelper.Initialize(consentRequired: true, onComplete: success => 
{
    if (success)
    {
        Debug.Log("Analytics system initialized!");
    }
});

// If consent is required, set consent status when obtained
AnalyticsHelper.SetUserConsent(AnalyticsConsentType.AnalyticsStorage, true);
```

### Basic Usage

Use the AnalyticsHelper to track events:

```csharp
// Track a simple event
AnalyticsHelper.LogEvent(AnalyticsEventTypes.GAME_START);

// Track an event with parameters
AnalyticsHelper.LogEvent(AnalyticsEventTypes.LEVEL_START, new Dictionary<string, object>
{
    { AnalyticsParameters.LEVEL_NAME, "Forest" },
    { AnalyticsParameters.LEVEL_NUMBER, 3 },
    { AnalyticsParameters.DIFFICULTY, "Normal" }
});

// Track revenue
AnalyticsHelper.LogPurchase("gems_pack_1", "USD", 4.99, "iap");

// Set user properties
AnalyticsHelper.SetUserProperty("preferred_character", "Chef_Mario");
```

## Event Tracking Best Practices

### Standardized Events

Use predefined event constants for consistency:

```csharp
// Define standard events in AnalyticsEventTypes.cs
public static class AnalyticsEventTypes
{
    public const string GAME_START = "game_start";
    public const string GAME_END = "game_end";
    public const string LEVEL_START = "level_start";
    public const string LEVEL_COMPLETE = "level_complete";
    public const string LEVEL_FAIL = "level_fail";
    public const string TUTORIAL_BEGIN = "tutorial_begin";
    public const string TUTORIAL_COMPLETE = "tutorial_complete";
    // ... and so on
}

// Define standard parameters in AnalyticsParameters.cs
public static class AnalyticsParameters
{
    public const string LEVEL_NAME = "level_name";
    public const string LEVEL_NUMBER = "level_number";
    public const string SCORE = "score";
    public const string SUCCESS = "success";
    public const string DIFFICULTY = "difficulty";
    // ... and so on
}
```

### Game-Specific Events

Track game-specific events that are meaningful for your analytics:

```csharp
// Track recipe creation
AnalyticsHelper.LogEvent("recipe_created", new Dictionary<string, object>
{
    { "recipe_type", "pasta" },
    { "ingredients_count", 5 },
    { "cooking_time", 180 }
});

// Track competitive match results
AnalyticsHelper.LogEvent("match_completed", new Dictionary<string, object>
{
    { "match_type", "ranked" },
    { "duration", 345 },
    { "position", 2 },
    { "players", 4 }
});
```

## Firebase Integration

The Analytics system uses Firebase Analytics as its primary provider, but is designed to support multiple providers.

### Configuration

Firebase-specific configuration is handled in the `FirebaseAnalyticsService` class:

```csharp
// In FirebaseAnalyticsService.cs
private void InitializeFirebase(bool consentRequired, Action<bool> onComplete)
{
    // Configure Firebase
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    {
        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            // Initialize Firebase Analytics
            _firebaseAnalytics = FirebaseAnalytics.GetInstance(FirebaseApp.DefaultInstance);
            
            // Set consent mode if required
            if (consentRequired)
            {
                _firebaseAnalytics.SetAnalyticsCollectionEnabled(false);
            }
            else
            {
                _firebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            
            // Register automatic app lifecycle tracking
            RegisterAppLifecycleTracking();
            
            onComplete?.Invoke(true);
        }
        else
        {
            LogHelper.Error("Analytics", $"Firebase dependency error: {dependencyStatus}");
            onComplete?.Invoke(false);
        }
    });
}
```

### Event Mapping

Events are mapped to Firebase Analytics events:

```csharp
// In FirebaseAnalyticsService.cs
public override void LogEvent(string eventName, Dictionary<string, object> parameters = null)
{
    if (!IsInitialized || !IsEnabled)
    {
        return;
    }
    
    try
    {
        if (parameters == null || parameters.Count == 0)
        {
            _firebaseAnalytics.LogEvent(eventName);
        }
        else
        {
            // Convert parameters to Firebase format
            var firebaseParams = new Parameter[parameters.Count];
            int i = 0;
            
            foreach (var param in parameters)
            {
                firebaseParams[i++] = GetFirebaseParameter(param.Key, param.Value);
            }
            
            _firebaseAnalytics.LogEvent(eventName, firebaseParams);
        }
        
        LogHelper.Debug("Analytics", $"Logged event: {eventName}");
    }
    catch (Exception ex)
    {
        LogHelper.Exception("Analytics", ex, $"Failed to log event: {eventName}");
    }
}

private Parameter GetFirebaseParameter(string key, object value)
{
    // Convert parameter value to appropriate Firebase Parameter type
    if (value is string stringValue)
    {
        return new Parameter(key, stringValue);
    }
    else if (value is long || value is int || value is short || value is byte)
    {
        return new Parameter(key, Convert.ToInt64(value));
    }
    else if (value is double || value is float || value is decimal)
    {
        return new Parameter(key, Convert.ToDouble(value));
    }
    else if (value is bool boolValue)
    {
        return new Parameter(key, boolValue ? 1 : 0);
    }
    else
    {
        // Default to string representation for other types
        return new Parameter(key, value.ToString());
    }
}
```

## Privacy and Consent

The Analytics system includes built-in support for user consent management:

```csharp
// Check if analytics requires consent
bool requiresConsent = AnalyticsHelper.RequiresConsent();

// If consent is required, show consent UI
if (requiresConsent)
{
    ShowConsentDialog(consentGranted => 
    {
        AnalyticsHelper.SetUserConsent(AnalyticsConsentType.AnalyticsStorage, consentGranted);
        AnalyticsHelper.SetUserConsent(AnalyticsConsentType.AdStorage, consentGranted);
    });
}
```

## Integration with Logging

While the Analytics system is separate from the Logging system, they can work together:

```csharp
// In FirebaseAnalyticsService.cs
private void SetupLoggingIntegration()
{
    // Subscribe to error logs to track app errors
    LogHelper.OnLogWritten += HandleLogMessage;
}

private void HandleLogMessage(LogMessage logMessage)
{
    // Only track errors and exceptions as analytics events
    if (logMessage.Level == LogLevel.Error)
    {
        // Log as a non-fatal error in Firebase
        LogEvent("app_error", new Dictionary<string, object>
        {
            { "module", logMessage.Module },
            { "message", logMessage.Message.Substring(0, Math.Min(100, logMessage.Message.Length)) },
            { "timestamp", logMessage.Timestamp.ToString("o") }
        });
    }
}
```

## Advanced Usage

### Custom User Identifiers

Set custom user IDs to identify users across devices:

```csharp
// Set a user ID once the user is authenticated
string userId = AuthHelper.GetCurrentUserId();
AnalyticsHelper.SetUserId(userId);
```

### A/B Testing Integration

Use Firebase A/B Testing with Analytics:

```csharp
Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWith(task => 
{
    if (task.IsCompleted)
    {
        // Get experiment variant
        string variant = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
            .GetValue("tutorial_variant").StringValue;
            
        // Log which variant the user is in
        AnalyticsHelper.SetUserProperty("tutorial_variant", variant);
        
        // Apply the variant in the game
        TutorialManager.SetVariant(variant);
    }
});
```

## Performance Considerations

- Events are buffered and sent in batches to minimize network traffic
- Automatic data capture is configured to minimize performance impact
- Heavy analytics operations are performed off the main thread

## Cross-Platform Support

The Analytics system works across all platforms supported by Firebase Analytics:
- iOS
- Android
- Web (via Firebase JS SDK)

## Multiple Providers

The system is designed to support multiple analytics providers simultaneously:

```csharp
// In AnalyticsService.cs
public class AnalyticsService : IAnalyticsService
{
    private List<IAnalyticsProvider> _providers = new List<IAnalyticsProvider>();
    
    public void AddProvider(IAnalyticsProvider provider)
    {
        _providers.Add(provider);
    }
    
    public override void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        foreach (var provider in _providers)
        {
            provider.LogEvent(eventName, parameters);
        }
    }
    
    // Other methods follow the same pattern...
}
```

## Testing Analytics Integration

Test your analytics implementation with Firebase Debug View:

1. Enable debug mode in development builds
2. Use the Firebase console to verify events
3. Use the Firebase Analytics DebugView
4. Create test users for validation

## License

Copyright © 2024 RecipeRage. All rights reserved. 