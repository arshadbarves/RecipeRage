# RecipeRage Authentication System

## Overview
The RecipeRage Authentication System is a robust, modular, and production-ready implementation that supports multiple authentication methods including:
- Guest login (using device ID)
- Facebook login
- Epic Online Services (EOS) device authentication

This system is designed to be separate from gameplay code, making it reusable across different games, with minimal MonoBehaviour dependencies.

## Architecture
The authentication system follows a modular architecture using the following design patterns:
- **Singleton Pattern**: Provides global access to key components without MonoBehaviour dependencies
- **Provider Pattern**: Enables multiple authentication methods
- **Service Locator Pattern**: Manages service dependencies
- **Factory Pattern**: Creates UI components without requiring MonoBehaviour inheritance
- **Static Helper**: Provides Unity-like easy access to auth functionality

### Key Components

```
├── Core/
│   ├── Patterns/                  # Reusable design patterns
│   │   ├── Singleton.cs           # Generic singleton for non-MonoBehaviour classes
│   │   ├── MonoBehaviourSingleton.cs # Generic singleton for MonoBehaviour classes
│   │   └── ServiceLocator.cs      # Service locator for dependency management
│   │
│   ├── Interfaces/                # Core interfaces
│   │   ├── IAuthProvider.cs       # Interface for auth providers
│   │   ├── IAuthProviderUser.cs   # Interface for user data
│   │   └── IAuthService.cs        # Interface for auth service
│   │
│   ├── Services/                  # Core services
│   │   └── LogService.cs          # Production-ready logging service
│   │
│   └── Auth/                      # Core auth implementation
│       └── AuthService.cs         # Main auth service (singleton)
│
├── Auth/
│   ├── Core/                      # Auth core classes
│   │   ├── BaseAuthProvider.cs    # Base auth provider implementation
│   │   └── AuthProviderUser.cs    # User data implementation
│   │
│   ├── Providers/                 # Auth provider implementations
│   │   ├── GuestAuthProvider.cs   # Guest login using device ID
│   │   ├── FacebookAuthProvider.cs # Facebook login
│   │   └── EOSDeviceAuthProvider.cs # EOS device authentication
│   │
│   ├── UI/                        # UI components
│   │   ├── AuthUIFactory.cs       # Factory for creating auth UI without MonoBehaviours
│   │   └── AuthLoginController.cs # Non-MonoBehaviour UI controller
│   │
│   ├── AuthHelper.cs              # Static helper for easy auth access
│   └── AuthBootstrap.cs           # Bootstrap component integrated with GameBootstrap
```

## Technical Specifications

### Authentication Flow
1. `GameBootstrap` initializes all game systems including auth
2. `AuthService` handles the authentication state and provider management
3. On first launch, the login UI is displayed using `AuthUIFactory`
4. User selects a login method (Guest, Facebook, etc.)
5. The selected provider authenticates the user
6. Credentials are saved for auto-login on next launch
7. Auth state changes are broadcasted via events

### Reduced MonoBehaviour Usage
- Only `GameBootstrap` requires MonoBehaviour inheritance
- `AuthService` uses a non-MonoBehaviour Singleton pattern
- UI creation is handled by a factory (`AuthUIFactory`) that dynamically creates UI without requiring attached scripts
- Static `AuthHelper` class provides easy access to auth functionality without getting instances

### Production-Ready Logging
- `LogService` handles both local and remote logging
- Log levels (Verbose, Debug, Info, Warning, Error, Fatal)
- File-based logging with full device and version information
- Remote crash reporting capability
- Static `Logger` class for Unity-like logging without getting instances

### Persistence
- User credentials are saved using `PlayerPrefs` for persistence across app restarts
- The last used provider is remembered for auto-login
- Each provider has its own credential storage implementation

### Security
- Sensitive data like access tokens is only stored locally
- Device ID uses secure hashing to create unique identifiers
- Facebook SDK integration follows Facebook's security guidelines

## Integration Guide

### Prerequisites
- Unity 2020.3 or later
- Epic Online Services Plugin for Unity
- Facebook SDK for Unity (optional, for Facebook login)

### Basic Setup

1. Add the `GameBootstrap` component to a GameObject in your initial scene
2. Configure the bootstrap component in the inspector
3. Access the auth functionality from anywhere:

```csharp
// Check if user is logged in
if (AuthHelper.IsSignedIn())
{
    Logger.Info("MyClass", $"Logged in as: {AuthHelper.CurrentUser.DisplayName}");
}

// Listen to auth state changes
AuthHelper.RegisterAuthStateCallback(user => {
    if (user != null)
    {
        Logger.Info("MyClass", $"User logged in: {user.DisplayName}");
    }
    else
    {
        Logger.Info("MyClass", "User logged out");
    }
});

// Show login UI
AuthHelper.ShowLoginUI(
    onComplete: success => {
        Logger.Info("MyClass", $"Login {(success ? "successful" : "failed")}");
    }
);

// Sign in programmatically
AuthHelper.SignInAsGuest(
    onSuccess: user => {
        Logger.Info("MyClass", $"Login successful: {user.DisplayName}");
    },
    onFailure: error => {
        Logger.Error("MyClass", $"Login failed: {error}");
    }
);

// Sign out
AuthHelper.SignOut();
```

### Advanced Usage

#### Adding Custom Auth Providers
1. Create a new class that extends `BaseAuthProvider`
2. Implement the required methods
3. Register the provider in `GameBootstrap` configuration or at runtime:

```csharp
// Get auth service from service locator
var authService = ServiceLocator.Instance.Get<IAuthService>();

// Register custom provider
authService.RegisterProvider(new MyCustomAuthProvider());
```

## Implementation Notes

### Single Bootstrap
- The system uses a single `GameBootstrap` for the entire game
- All subsystems (Auth, Logging, etc.) are initialized through this bootstrap
- Configuration of different subsystems is handled through inspector properties

### UI Creation without MonoBehaviours
The system uses a factory approach to create UI without requiring MonoBehaviour inheritance:

```csharp
// Create and show login UI using factory
AuthUIFactory.CreateLoginUI(
    parent: transform,
    onComplete: success => {
        // Handle auth completion
    }
);
```

This creates a UI Document and associated controller without needing to attach scripts to GameObjects.

## TODOs
1. Add unit tests for core components
2. Implement more social login options (Google, Apple, etc.)
3. Add offline mode support
4. Enhance security with token encryption
5. Add user profile management
6. Implement account linking between providers

## Dependencies
- Unity UI Toolkit for UI elements
- Epic Online Services Plugin v4.0.0+ for Unity
- Facebook SDK for Unity (optional)

## Known Limitations
- Facebook login requires Facebook SDK integration
- EOS integration requires proper EOS setup and credentials
- Mobile platform authentication may require additional configuration

## License
This code is proprietary and intended for use only in authorized projects.

## Support
For issues or feature requests, please contact the development team. 