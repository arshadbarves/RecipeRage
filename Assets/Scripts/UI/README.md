# RecipeRage UI System

This directory contains the UI system for RecipeRage, built using Unity's UI Toolkit.

## Overview

The UI system is designed to be modular, reusable, and easy to extend. It consists of:

- **UIManager**: Singleton that manages all UI screens in the game
- **UIScreen**: Base class for all UI screens
- **UIAnimationSystem**: System for animating UI elements
- **UIExtensions**: Extension methods for UI Toolkit elements

## Directory Structure

- **Animation/**: Contains the animation system for UI elements
- **Screens/**: Contains all UI screen implementations
- **UXML/**: Contains UI layout definitions
- **USS/**: Contains UI style sheets

## Setup

To set up the UI system:

1. Run the "RecipeRage/Setup UI Resources" menu item to create placeholder images
2. Run the "RecipeRage/Create UI Prefabs" menu item to create UI prefabs

## Usage

### Creating a New Screen

1. Create a new UXML file in the `Assets/UI/UXML` directory
2. Create a new USS file in the `Assets/UI/USS` directory
3. Create a new C# class that inherits from `UIScreen` in the `Assets/Scripts/UI/Screens` directory
4. Implement the `InitializeScreen` method to set up your UI elements
5. Register the screen with the `UIManager` in the `UIInitializer`

### Showing and Hiding Screens

```csharp
// Show a screen
UIManager.Instance.ShowScreen<MyScreen>(true);

// Hide a screen
UIManager.Instance.GetScreen<MyScreen>()?.Hide(true);

// Toggle a screen
UIManager.Instance.GetScreen<MyScreen>()?.Toggle(true);
```

### Animating UI Elements

```csharp
// Animate a single element
UIAnimationSystem.Instance.Animate(
    element,
    UIAnimationSystem.AnimationType.FadeIn,
    0.5f,
    0f,
    UIAnimationSystem.EasingType.EaseOutCubic,
    () => Debug.Log("Animation complete")
);

// Animate a sequence of elements
UIAnimationSystem.Instance.AnimateSequence(
    elements,
    UIAnimationSystem.AnimationType.ScaleIn,
    0.3f,
    0.05f,
    UIAnimationSystem.EasingType.EaseOutBack,
    () => Debug.Log("All animations complete")
);

// Chain multiple animations on a single element
UIAnimationSystem.Instance.ChainAnimations(
    element,
    new List<UIAnimationSystem.AnimationType> {
        UIAnimationSystem.AnimationType.FadeIn,
        UIAnimationSystem.AnimationType.Bounce
    },
    new List<float> { 0.5f, 0.5f },
    new List<float> { 0f, 0f },
    new List<UIAnimationSystem.EasingType> {
        UIAnimationSystem.EasingType.EaseOutCubic,
        UIAnimationSystem.EasingType.EaseOutElastic
    },
    () => Debug.Log("Chain complete")
);
```

### Using UI Extensions

```csharp
// Set background image
element.SetBackgroundImage(sprite);

// Set tint color
element.SetTintColor(color);

// Add click handler
element.AddClickHandler(() => Debug.Log("Clicked"));

// Make draggable
element.MakeDraggable();

// Add hover effect
element.AddHoverEffect();

// Add press effect
element.AddPressEffect();

// Make circular
element.MakeCircular();

// Set rounded corners
element.SetRoundedCorners(10f);

// Add shadow
element.AddShadow(Color.black, new Vector2(2, 2), 5f);
```

## Available Screens

- **SplashScreen**: Shown when the game starts
- **MainMenuScreen**: Main menu and lobby screen
- **CharacterSelectionScreen**: Character selection screen
- **GameModeSelectionScreen**: Game mode selection screen
- **SettingsScreen**: Settings screen

## Animation Types

- FadeIn
- FadeOut
- SlideInFromLeft
- SlideInFromRight
- SlideInFromTop
- SlideInFromBottom
- SlideOutToLeft
- SlideOutToRight
- SlideOutToTop
- SlideOutToBottom
- ScaleIn
- ScaleOut
- Bounce
- Pulse
- Shake
- RotateIn
- RotateOut

## Easing Types

- Linear
- EaseInQuad
- EaseOutQuad
- EaseInOutQuad
- EaseInCubic
- EaseOutCubic
- EaseInOutCubic
- EaseInElastic
- EaseOutElastic
- EaseInOutElastic
