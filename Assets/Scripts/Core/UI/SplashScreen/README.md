# Splash Screen System

## Overview

The RecipeRage splash screen system provides a professional, polished introduction to the game, similar to what you'd see in Supercell games or PUBG Mobile. It consists of:

1. **Company Splash Screen**: Displays your company logo
2. **Game Logo Splash Screen**: Displays the RecipeRage logo and tagline
3. **Loading Screen**: Shows initialization progress with a progress bar and loading tips

## Features

- **Smooth Transitions**: Fade in/out transitions between screens
- **Progress Tracking**: Real-time progress updates during system initialization
- **Loading Tips**: Rotating tips to engage players during loading
- **Skip Option**: Players can skip splash screens if desired
- **Minimum Display Time**: Ensures loading screen shows for a minimum time for visual consistency

## Integration with GameBootstrap

The splash screen system is fully integrated with GameBootstrap:

```csharp
// In GameBootstrap.cs
private IEnumerator InitializeGameSystems()
{
    // Initialize splash screen manager first
    if (_showSplashScreens)
    {
        yield return StartCoroutine(InitializeSplashScreenManager());

        // Show company splash screen
        yield return _splashScreenManager.ShowCompanySplash().AsCoroutine();

        // Show game logo splash screen
        yield return _splashScreenManager.ShowGameLogoSplash().AsCoroutine();

        // Show loading screen
        _splashScreenManager.ShowLoadingScreen();
    }

    // Initialize other systems with progress updates
    if (_initializeSaveSystem)
    {
        if (_showSplashScreens)
        {
            _splashScreenManager.UpdateLoadingProgress("Initializing Save System...", 0.05f);
        }

        yield return StartCoroutine(InitializeSaveSystem());
    }

    // ... other systems ...

    // Hide loading screen when done
    if (_showSplashScreens)
    {
        yield return _splashScreenManager.HideLoadingScreen().AsCoroutine();
    }
}
```

## Components

### SplashScreenManager

The main controller for all splash screens and the loading screen.

```csharp
public class SplashScreenManager : MonoBehaviourSingleton<SplashScreenManager>
{
    // Show company splash screen
    public async Task ShowCompanySplash();

    // Show game logo splash screen
    public async Task ShowGameLogoSplash();

    // Show loading screen
    public void ShowLoadingScreen();

    // Update loading progress
    public void UpdateLoadingProgress(string status, float progress);

    // Hide loading screen
    public async Task HideLoadingScreen();
}
```

### UI Toolkit Assets

- **CompanySplashScreen.uxml/uss**: Company logo display
- **GameLogoSplashScreen.uxml/uss**: Game logo with tagline
- **LoadingScreen.uxml/uss**: Loading screen with progress bar and tips

### TaskExtensions

Utility for converting async Tasks to Unity coroutines.

```csharp
public static class TaskExtensions
{
    // Convert a Task to a coroutine
    public static IEnumerator AsCoroutine(this Task task);

    // Convert a Task<T> to a coroutine
    public static IEnumerator AsCoroutine<T>(this Task<T> task);
}
```

## Setup

### Required Assets

The following assets are required for the splash screens to display properly:

- `Assets/Textures/UI/company_logo.png`: Your company logo
- `Assets/Textures/UI/recipe_rage_logo.png`: The RecipeRage game logo
- `Assets/Textures/UI/loading_background.png`: Background image for the loading screen

### Creating the Prefabs

Use the following menu items to create the necessary prefabs:

- **RecipeRage > UI > Create Splash Screen Manager Prefab**: Creates only the SplashScreenManager prefab
- **RecipeRage > Create > All Game Assets**: Creates all prefabs including SplashScreenManager

### Setup Wizard

Use the Splash Screen Setup Wizard to quickly set up the splash screen system:

1. Go to **RecipeRage > UI > Splash Screen Setup Wizard**
2. Add your textures and customize text
3. Click "Setup Everything" to create all necessary assets

## Customization

You can customize the splash screen system in the Inspector:

- **Durations**: Adjust how long each splash screen is displayed
- **Transitions**: Adjust fade transition durations
- **Loading Tips**: Add or modify loading tips
- **Skip Option**: Enable/disable the ability to skip splash screens

## Testing

To test the splash screen system:

1. Open the SplashScreenTest scene
2. Press Play to see the splash screens in action
3. The splash screens will automatically play in sequence
