# Localization Manager - Professional Documentation

## Overview
A professional-grade localization system for Unity mobile games with support for multiple languages, pluralization, missing key tracking, and hot-reload capabilities.

## Features

### ✅ Core Features
- **CSV-Based Translation Storage** - Easy to edit and version control
- **Robust CSV Parser** - Handles quotes, commas, and special characters
- **Auto Language Detection** - Detects system language on mobile devices
- **Optimized Performance** - Dictionary caching for fast lookups
- **Missing Key Tracking** - Automatic detection and reporting of missing translations
- **Fallback Chain** - Graceful degradation to default language
- **String Formatting** - Supports `{0}`, `{1}` placeholders
- **Pluralization Support** - Smart handling of zero, one, and many cases
- **Hot-Reload** - Refresh translations without restarting
- **Event System** - Subscribe to language change events
- **Validation Tools** - Check for incomplete translations

### ✅ Unity Editor Integration
- **Visual Editor Window** - Manage translations directly in Unity
- **Add New Keys** - Add translations without leaving Unity
- **Validation** - One-click validation of all translations
- **Quick Open CSV** - Open CSV file in external editor

## Usage Examples

### Basic Text Retrieval

```csharp
using Modules.Localization;

public class UIController : MonoBehaviour
{
    private ILocalizationManager _localization;

    public void Construct(ILocalizationManager localization)
    {
        _localization = localization;
    }

    void Start()
    {
        // Simple text retrieval
        string loadingText = _localization.GetText("splash_loading");
        // Returns: "Loading..." (English) or "Cargando..." (Spanish)
    }
}
```

### String Formatting

```csharp
// In CSV:
// player_score,Score: {0},Puntuación: {0}

string scoreText = _localization.GetText("player_score", 1250);
// Returns: "Score: 1250" or "Puntuación: 1250"
```

### Pluralization

```csharp
// In CSV:
// item_count_zero,No items,Sin artículos
// item_count_one,{0} item,{0} artículo
// item_count_other,{0} items,{0} artículos

string text = _localization.GetTextPlural("item_count", 0);
// Returns: "No items" or "Sin artículos"

text = _localization.GetTextPlural("item_count", 1);
// Returns: "1 item" or "1 artículo"

text = _localization.GetTextPlural("item_count", 5);
// Returns: "5 items" or "5 artículos"
```

### Language Switching

```csharp
// Get available languages
foreach (string lang in _localization.AvailableLanguages)
{
    Debug.Log($"Available: {lang}");
}

// Change language
_localization.SetLanguage("Spanish");

// Subscribe to language changes
_localization.OnLanguageChanged += OnLanguageChanged;

void OnLanguageChanged()
{
    // Refresh all UI text
    RefreshAllText();
}
```

### Missing Key Detection

```csharp
// Check if key exists before using
if (_localization.HasKey("new_feature_text"))
{
    string text = _localization.GetText("new_feature_text");
}

// Get missing keys report
string report = _localization.GetMissingKeysReport();
Debug.Log(report);

// Clear tracking (useful for testing)
_localization.ClearMissingKeys();
```

### Validation (Editor or Runtime)

```csharp
// Validate all translations
var missingTranslations = _localization.ValidateAllKeys();

foreach (var kvp in missingTranslations)
{
    string key = kvp.Key;
    List<string> missingLanguages = kvp.Value;
    Debug.LogWarning($"Key '{key}' missing in: {string.Join(", ", missingLanguages)}");
}
```

### Hot-Reload (Development)

```csharp
#if UNITY_EDITOR
// Reload translations after CSV changes
_localization.Reload();
#endif
```

### Statistics

```csharp
string stats = _localization.GetStatistics();
Debug.Log(stats);
// Output:
// Localization Statistics:
// - Total Keys: 20
// - Available Languages: 2
// - Current Language: English
// - Cached Languages: 1
// - Missing Keys: 0
```

## CSV File Format

### Location
```
Assets/Resources/Data/Localization.csv
```

### Format
```csv
Key,English,Spanish
splash_loading,Loading...,Cargando...
login_guest,Guest Login,Invitado
player_welcome,"Welcome, {0}!","¡Bienvenido, {0}!"
```

### Rules
1. **First column** is always the key (unique identifier)
2. **Header row** defines language names
3. **Quotes** are used for values containing commas
4. **Double quotes** (`""`) escape quotes inside values
5. **Format placeholders** use C# string formatting: `{0}`, `{1}`, etc.

### Pluralization Naming Convention
```csv
item_count_zero,No items,Sin artículos
item_count_one,{0} item,{0} artículo
item_count_other,{0} items,{0} artículos
```

## Unity Editor Window

### Access
```
Tools → Localization Manager
```

### Features
- **Validate All Keys** - Check for missing translations
- **Add New Key** - Add keys without editing CSV manually
- **Open CSV** - Quick access to external editor
- **Refresh Database** - Force Unity to reload assets

## Architecture

### Interface-Based Design
```csharp
public interface ILocalizationManager
{
    string CurrentLanguage { get; }
    IReadOnlyCollection<string> AvailableLanguages { get; }
    IReadOnlyCollection<string> MissingKeys { get; }

    void Initialize();
    void SetLanguage(string languageCode);
    string GetText(string key);
    string GetText(string key, params object[] args);
    bool HasKey(string key);
    string GetTextPlural(string keyPrefix, int count);
    // ... more methods
}
```

### Dependency Injection
Uses VContainer for dependency injection:
```csharp
builder.Register<LocalizationManager>(Lifetime.Singleton).AsImplementedInterfaces();
```

### Initialization (SOLID Pattern)
LocalizationManager follows the **Single Responsibility Principle** by initializing itself:

```csharp
public class LocalizationManager : ILocalizationManager
{
    // Constructor automatically initializes when created by DI container
    public LocalizationManager()
    {
        Initialize();
    }
}
```

**Benefits:**
- ✅ Self-contained initialization (SRP)
- ✅ Ready before any UI is shown
- ✅ No manual initialization needed
- ✅ Clean dependency injection
- ✅ All text translated from frame 1

**Timeline:**
1. VContainer creates LocalizationManager (Singleton)
2. Constructor runs → Initialize() called automatically
3. CSV loaded & language auto-detected
4. All ViewModels receive initialized instance
5. UI shows translated text immediately ✅

## Performance Considerations

### Caching
- **First Access**: Parses CSV and builds cache (~10ms for 1000 keys)
- **Subsequent Access**: O(1) dictionary lookup (~0.01ms)
- **Memory**: ~50KB per language for 1000 keys

### Mobile Optimization
- No asset bundle loading (uses Resources)
- No localized images/audio (text only)
- Minimal garbage allocation
- Cached per language

## Best Practices

### 1. Key Naming Convention
```
[category]_[feature]_[element]
```
Examples:
- `login_button_submit`
- `shop_item_buy`
- `settings_audio_music`

### 2. Always Use Keys, Never Hardcode Text
❌ Bad:
```csharp
buttonText.text = "Start Game";
```

✅ Good:
```csharp
buttonText.text = _localization.GetText("menu_button_start");
```

### 3. Check Keys During Development
```csharp
#if UNITY_EDITOR
if (!_localization.HasKey("new_key"))
{
    Debug.LogWarning("Missing translation key: new_key");
}
#endif
```

### 4. Subscribe to Language Changes
```csharp
void OnEnable()
{
    _localization.OnLanguageChanged += RefreshText;
}

void OnDisable()
{
    _localization.OnLanguageChanged -= RefreshText;
}
```

### 5. Use Pluralization Correctly
Always provide all three plural forms:
- `*_zero` - for 0 items
- `*_one` - for 1 item
- `*_other` - for 2+ items

## Adding New Languages

1. **Add column to CSV**:
```csv
Key,English,Spanish,French
splash_loading,Loading...,Cargando...,Chargement...
```

2. **Update auto-detection mapping**:
```csharp
string detectedLanguage = systemLanguage switch
{
    "Spanish" => "Spanish",
    "English" => "English",
    "French" => "French",  // Add this
    _ => null
};
```

3. **Test in Unity Editor** using `Tools → Localization Manager`

## Troubleshooting

### Missing Translations
Check the console for warnings:
```
[Localization] Missing key: 'unknown_key' in language 'English'
```

### CSV Not Loading
Verify file location:
```
Assets/Resources/Data/Localization.csv
```

### Language Not Detected
Check Unity's SystemLanguage matches your mapping in `AutoDetectLanguage()`.

### Special Characters
Ensure CSV is UTF-8 encoded for special characters (é, ñ, ü, etc.).

## Testing Checklist

- [ ] All languages load without errors
- [ ] Auto-detection works on device
- [ ] Language switching updates all UI
- [ ] Pluralization works correctly
- [ ] String formatting works with parameters
- [ ] Missing keys are logged
- [ ] Validation finds incomplete translations
- [ ] Special characters display correctly
- [ ] Performance is acceptable (cached lookups < 1ms)

## Migration from Old System

If you had a basic system:

1. **Export existing translations** to CSV format
2. **Update all GetText calls** to use interface
3. **Add Initialize call** to bootstrap
4. **Test each screen** for missing keys
5. **Run validation** to find gaps

## Support & Maintenance

### Regular Tasks
- Weekly: Review missing key reports
- Monthly: Add new translations for features
- Per Release: Validate all keys

### QA Checklist
- Test on devices with different system languages
- Verify text fits in UI elements for all languages
- Check special characters render correctly
- Test language switching in settings

---

**Last Updated**: January 2026
**Version**: 1.0 Professional
**Maintainer**: Recipe Rage Team

