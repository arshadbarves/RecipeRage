# Splash Screen and Loading System

This directory contains the implementation of RecipeRage's professional splash screen and loading system.

## Overview

The splash screen system consists of three main screens:

1. **Company Splash Screen**: Displays your company logo (similar to Supercell splash)
2. **Game Logo Splash Screen**: Displays the RecipeRage logo and tagline
3. **Loading Screen**: Shows initialization progress with a progress bar and loading tips

## Components

- **SplashScreenManager**: Main controller for all splash screens and loading screen
- **UI Toolkit Assets**: UXML and USS files for the visual layout
- **TaskExtensions**: Utility for converting async Tasks to Unity coroutines

## Integration with GameBootstrap

The splash screen system is integrated with GameBootstrap to show initialization progress for each system:

1. SplashScreenManager is initialized first
2. Company splash screen is shown
3. Game logo splash screen is shown
4. Loading screen is shown
5. Each system initialization updates the loading progress
6. When all systems are initialized, the loading screen is hidden

## Customization

You can customize the splash screen system in the Inspector:

- **Durations**: Adjust how long each splash screen is displayed
- **Transitions**: Adjust fade transition durations
- **Loading Tips**: Add or modify loading tips
- **Skip Option**: Enable/disable the ability to skip splash screens

## Creating the Prefabs

Use the following menu items to create the necessary prefabs:

- **RecipeRage > UI > Create Splash Screen Manager Prefab**: Creates only the SplashScreenManager prefab
- **RecipeRage > Create > All Game Assets**: Creates all prefabs including SplashScreenManager

## Required Assets

The following assets are required for the splash screens to display properly:

- `Assets/Textures/UI/company_logo.png`: Your company logo
- `Assets/Textures/UI/recipe_rage_logo.png`: The RecipeRage game logo
- `Assets/Textures/UI/loading_background.png`: Background image for the loading screen
