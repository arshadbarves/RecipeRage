# UI System Documentation

## Overview
This UI system provides a complete main menu with tabs for Lobby, Skins, Shop, and Settings. It includes mobile joystick controls similar to PUBG Mobile.

## Components

### Main Menu (MainMenuUI.cs)
- Manages the overall menu structure
- Handles tab navigation
- Initializes all sub-systems
- Manages player info display

### Shop System (ShopUI.cs)
- Multiple categories: Featured, Skins, Weapons, Bundles
- Dynamic item loading based on category
- Purchase system with currency integration
- Shows owned items

### Skins System (SkinsUI.cs)
- Character skin selection
- Preview system
- Equip/Unequip functionality
- Saves equipped skin to PlayerPrefs

### Settings System (SettingsUI.cs)
- Audio controls (Music, SFX)
- Graphics settings (Quality, Fullscreen)
- Language selection (10 languages)
- Mobile controls editor
- Support links:
  - Help
  - Contact Support
  - Privacy Policy
  - Terms & Conditions
  - Parent Guide

### Currency Manager (CurrencyManager.cs)
- Singleton pattern for global access
- Manages coins and gems
- Handles purchases and rewards
- Auto-saves to PlayerPrefs
- Formats large numbers (1.5K, 2.3M)

### Mobile Controls

#### MobileJoystick.cs
- Touch-based joystick control
- Fixed or floating position modes
- Adjustable size, opacity, dead zone
- Sensitivity control
- Similar to PUBG Mobile

#### MobileControlsManager.cs
- Manages movement and aim joysticks
- Action buttons (Jump, Attack, Special, Interact)
- Platform detection (only shows on mobile)
- Show/Hide controls

#### JoystickEditorUI.cs
- Visual editor for joystick customization
- Real-time preview
- Settings:
  - Joystick size
  - Opacity
  - Dead zone
  - Fixed/Floating position
- Save/Reset functionality

## Usage

### Setting Up Main Menu
1. Create a UI Document GameObject
2. Attach MainMenuUI.cs component
3. Assign MainMenuTemplate.uxml as the source
4. The system will auto-initialize all tabs

### Currency System
```csharp
// Get instance
CurrencyManager currency = CurrencyManager.Instance;

// Add currency
currency.AddCoins(500);
currency.AddGems(50);

// Spend currency
if (currency.SpendCoins(100))
{
    // Purchase successful
}

// Check balance
int coins = currency.GetCoins();
int gems = currency.GetGems();
```

### Mobile Controls
```csharp
// Get input from joysticks
MobileControlsManager controls = GetComponent<MobileControlsManager>();
Vector2 movement = controls.MovementInput;
Vector2 aim = controls.AimInput;

// Show/Hide controls
controls.ShowControls();
controls.HideControls();

// Reset joysticks
controls.ResetControls();
```

### Customizing Shop Items
Edit the `GetItemsForCategory()` method in ShopUI.cs:
```csharp
case "featured":
    return new ShopItemData[]
    {
        new ShopItemData("Item Name", 1500, "type"),
        // Add more items...
    };
```

### Adding New Skins
Edit the `PopulateSkins()` method in SkinsUI.cs:
```csharp
string[] skinNames = { 
    "Classic Chef", 
    "Master Chef", 
    "Your New Skin"
};
```

## PlayerPrefs Keys

### Currency
- `PlayerCoins` - Player's coin balance
- `PlayerGems` - Player's gem balance

### Settings
- `MusicVolume` - Music volume (0-1)
- `SFXVolume` - SFX volume (0-1)
- `Fullscreen` - Fullscreen mode (0/1)
- `Quality` - Graphics quality level
- `Language` - Selected language index
- `Sensitivity` - Control sensitivity

### Joystick
- `JoystickSize` - Joystick size multiplier
- `JoystickOpacity` - Joystick opacity (0-1)
- `JoystickDeadZone` - Dead zone threshold
- `JoystickFixed` - Fixed position mode (0/1)

### Player Data
- `EquippedSkin` - Currently equipped skin name
- `Owned_{ItemName}` - Ownership flag for shop items

## Styling

All styles are defined in USS files:
- `MainMenu.uss` - Main menu and tabs
- `Shop.uss` - Shop screen
- `Skins.uss` - Skins screen
- `Settings.uss` - Settings screen
- `JoystickEditor.uss` - Joystick editor

Colors follow the theme:
- Primary: `rgb(245, 240, 235)` - Cream
- Accent: `rgb(255, 100, 100)` - Red
- Success: `rgb(100, 230, 150)` - Green
- Info: `rgb(100, 201, 255)` - Blue
- Dark: `rgb(40, 30, 25)` - Dark brown

## Production Checklist

- [x] All UI screens implemented
- [x] Currency system with save/load
- [x] Shop with purchase functionality
- [x] Skins with equip system
- [x] Settings with all options
- [x] Mobile joystick controls
- [x] Joystick customization editor
- [x] Support links (Privacy, Terms, etc.)
- [x] Language selection
- [x] Error handling
- [x] PlayerPrefs persistence
- [x] Proper initialization flow
- [x] Memory management (singleton pattern)

## Notes

- All URLs in Settings (Privacy, Terms, etc.) should be updated to your actual website
- Shop items are currently hardcoded - consider loading from a JSON file or server
- Mobile controls only activate on Android/iOS builds
- Currency system includes editor-only test functionality (remove for production)
- All UI uses UI Toolkit (not legacy UGUI)

## Support

For issues or questions, refer to Unity UI Toolkit documentation:
https://docs.unity3d.com/Manual/UIElements.html
