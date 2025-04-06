# RecipeRage UI System

This directory contains the UI system for RecipeRage, built using Unity's UI Toolkit.

## Setup Instructions

1. **Generate Placeholder Images**
   - Go to the Unity Editor menu: `RecipeRage > Generate Placeholder Images`
   - This will create placeholder images for all UI elements in `Assets/Resources/UI/Images`

2. **Create UI Prefabs**
   - Create the necessary directories:
     ```
     mkdir -p Assets/Prefabs/UI
     ```
   - Create prefabs for each UI screen manually or use the UIPrefabCreator script

3. **Set Up UI Scene**
   - Create a new scene for UI
   - Add a UIManager GameObject with the UIInitializer component
   - Add a Main Camera
   - Add an EventSystem

## UI Structure

- **UXML Files**: Define the structure of UI screens
- **USS Files**: Define the styles for UI elements
- **C# Scripts**: Implement the behavior of UI screens

## UI Screens

- **SplashScreen**: Shown when the game starts
- **MainMenuScreen**: Main menu and lobby screen
- **CharacterSelectionScreen**: Character selection screen
- **GameModeSelectionScreen**: Game mode selection screen
- **SettingsScreen**: Settings screen

## UI Components

- **UIManager**: Manages all UI screens
- **UIScreen**: Base class for all UI screens
- **UIAnimationSystem**: Handles UI animations
- **UIExtensions**: Extension methods for UI elements

## Troubleshooting

If you encounter issues with UI Builder not opening UXML files:

1. Make sure the UXML files have the correct XML declaration:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <UXML xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="UnityEngine.UIElements" 
         xsi:noNamespaceSchemaLocation="../../../UIElementsSchema/UIElements.xsd"
         xsi:schemaLocation="UnityEngine.UIElements ../../../UIElementsSchema/UnityEngine.UIElements.xsd">
   ```

2. Check for syntax errors in the UXML files
3. Make sure all referenced USS files exist
4. Try recreating the UXML file from scratch if needed

## Resources

- [UI Toolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- [UI Builder Documentation](https://docs.unity3d.com/Manual/UIBuilder.html)
