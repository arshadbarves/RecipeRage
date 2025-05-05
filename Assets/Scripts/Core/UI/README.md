# RecipeRage UI System

This directory contains the core UI components for the RecipeRage game, including the splash screen and loading screen systems.

## Splash Screen and Loading Screen

The splash screen and loading screen systems have been separated into two distinct components:

1. **SplashScreenManager**: Handles the company splash screen and game logo splash screen.
2. **LoadingScreenManager**: Handles the loading screen with progress bar and loading tips.

### SplashScreenManager

The SplashScreenManager is responsible for displaying the initial splash screens when the game starts:

- Company splash screen (e.g., studio logo)
- Game logo splash screen

It provides smooth transitions between these screens and allows for skipping with mouse clicks or keyboard input.

### LoadingScreenManager

The LoadingScreenManager is responsible for displaying the loading screen during game initialization:

- Shows a progress bar indicating loading progress
- Displays status messages about what's being loaded
- Shows cycling loading tips to entertain the player during loading
- Provides smooth transitions when showing and hiding

## Scene Setup

To set up the RecipeRage scenes with the splash screen and loading screen systems:

1. Open Unity Editor
2. Go to the menu: `RecipeRage > Setup > Setup All Scenes`
3. This will create:
   - Startup.unity (with splash and loading screens)
   - MainMenu.unity
   - Game.unity

To add these scenes to the build settings:

1. Go to the menu: `RecipeRage > Setup > Add Scenes to Build Settings`

## Manual Setup

If you prefer to set up the scenes manually:

1. Create the splash screen and loading screen prefabs:
   - Go to the menu: `RecipeRage > Setup > Create Splash and Loading Screen Prefabs`
   
2. Generate the startup scene:
   - Go to the menu: `RecipeRage > Setup > Generate Startup Scene`

3. Generate the main menu scene:
   - Go to the menu: `RecipeRage > Setup > Generate Main Menu Scene`

4. Generate the game scene:
   - Go to the menu: `RecipeRage > Setup > Generate Game Scene`

## Customization

To customize the splash screens and loading screen:

1. Modify the UI assets in:
   - `Assets/UI/UXML/CompanySplashScreen.uxml`
   - `Assets/UI/UXML/GameLogoSplashScreen.uxml`
   - `Assets/UI/UXML/LoadingScreen.uxml`

2. Modify the styles in:
   - `Assets/UI/USS/CompanySplashScreen.uss`
   - `Assets/UI/USS/GameLogoSplashScreen.uss`
   - `Assets/UI/USS/LoadingScreen.uss`

3. Regenerate the prefabs using the menu items mentioned above

## Integration with GameBootstrap

The GameBootstrap class is responsible for initializing all game systems, including the splash screen and loading screen managers. It shows the splash screens and loading screen during game initialization and updates the loading progress as each system is initialized.

The initialization sequence is:

1. Initialize SplashScreenManager
2. Initialize LoadingScreenManager
3. Show company splash screen
4. Show game logo splash screen
5. Show loading screen
6. Initialize all game systems (with loading progress updates)
7. Hide loading screen
8. Transition to the initial game state (usually MainMenu)
