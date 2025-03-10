# Achievements & Stats Module

A comprehensive system for tracking player achievements and statistics with Epic Online Services integration.

## Overview

The Achievements & Stats module provides a robust solution for tracking player progress, rewarding accomplishments, and storing gameplay statistics. It integrates with Epic Online Services (EOS) to provide cross-platform achievement tracking and stat synchronization across devices.

Key features:
- Achievement unlocking and progress tracking
- Statistics tracking with various aggregation methods
- Automatic stat-based achievement unlocking
- Local caching with cloud synchronization
- Event-based workflow for UI notifications
- Cross-platform support through EOS
- Support for multiple providers (expandable architecture)

## Architecture

The Achievements & Stats module follows a modular design with clear separation of concerns:

```
Assets/Scripts/Modules/Achievements/
│
├── Interfaces/                    # Interface definitions
│   ├── IAchievementsService.cs    # Main service interface
│   └── IAchievementsProvider.cs   # Provider interface
│
├── Data/                          # Data models
│   ├── Achievement.cs             # Player achievement model
│   ├── AchievementDefinition.cs   # Definition from provider
│   ├── PlayerStat.cs              # Player statistic model
│   └── StatDefinition.cs          # Definition from provider
│
├── Core/                          # Core implementations
│   └── AchievementsService.cs     # Main service implementation
│
├── Providers/                     # Provider implementations
│   └── EOS/
│       └── EOSAchievementsProvider.cs # EOS implementation
│
└── AchievementsHelper.cs          # Static helper for easy access
```

## Getting Started

### Initialization

Initialize the achievements system early in your application:

```csharp
// Initialize achievements
AchievementsHelper.Initialize(success =>
{
    if (success)
    {
        Debug.Log("Achievements initialized successfully");
    }
    else
    {
        Debug.LogError("Failed to initialize achievements");
    }
});
```

### Prerequisites

1. Epic Online Services SDK for Unity
2. User must be authenticated with Epic Online Services
3. The Auth module must be initialized before Achievements

## Basic Usage

### Unlocking Achievements

```csharp
// Unlock an achievement
AchievementsHelper.UnlockAchievement("achievement_id", (success, error) =>
{
    if (success)
    {
        Debug.Log("Achievement unlocked");
    }
    else
    {
        Debug.LogError($"Failed to unlock achievement: {error}");
    }
});

// Update achievement progress
AchievementsHelper.UpdateAchievementProgress("achievement_id", 0.5f, (success, error) =>
{
    if (success)
    {
        Debug.Log("Achievement progress updated to 50%");
    }
    else
    {
        Debug.LogError($"Failed to update achievement progress: {error}");
    }
});

// Check if an achievement is unlocked
bool isUnlocked = AchievementsHelper.IsAchievementUnlocked("achievement_id");

// Get achievement progress
float progress = AchievementsHelper.GetAchievementProgress("achievement_id");
```

### Tracking Statistics

```csharp
// Update a stat
AchievementsHelper.UpdateStat("kills", 10, (success, error) =>
{
    if (success)
    {
        Debug.Log("Stat updated");
    }
    else
    {
        Debug.LogError($"Failed to update stat: {error}");
    }
});

// Increment a stat
AchievementsHelper.IncrementStat("deaths", 1, (success, error) =>
{
    if (success)
    {
        Debug.Log("Stat incremented");
    }
    else
    {
        Debug.LogError($"Failed to increment stat: {error}");
    }
});

// Get stat value
double kills = AchievementsHelper.GetStatValue("kills");
```

### Querying Data

```csharp
// Query achievements
AchievementsHelper.QueryAchievements(forceRefresh: true, (achievements, error) =>
{
    if (achievements != null)
    {
        Debug.Log($"Found {achievements.Count} achievements");
        foreach (var achievement in achievements)
        {
            Debug.Log($"- {achievement.Title}: {achievement.Progress:P0}");
        }
    }
    else
    {
        Debug.LogError($"Failed to query achievements: {error}");
    }
});

// Query stats
AchievementsHelper.QueryStats(forceRefresh: true, (stats, error) =>
{
    if (stats != null)
    {
        Debug.Log($"Found {stats.Count} stats");
        foreach (var stat in stats)
        {
            Debug.Log($"- {stat.Name}: {stat.Value}");
        }
    }
    else
    {
        Debug.LogError($"Failed to query stats: {error}");
    }
});
```

### Synchronization

```csharp
// Sync achievements with cloud
AchievementsHelper.SynchronizeAchievements((success, error) =>
{
    if (success)
    {
        Debug.Log("Achievements synchronized successfully");
    }
    else
    {
        Debug.LogError($"Failed to synchronize achievements: {error}");
    }
});

// Sync stats with cloud
AchievementsHelper.SynchronizeStats((success, error) =>
{
    if (success)
    {
        Debug.Log("Stats synchronized successfully");
    }
    else
    {
        Debug.LogError($"Failed to synchronize stats: {error}");
    }
});
```

## Event Handling

The Achievements module provides events for tracking achievements and stats updates:

```csharp
// Register for achievement unlocked events
AchievementsHelper.RegisterAchievementUnlockedCallback(achievement =>
{
    Debug.Log($"Achievement unlocked: {achievement.Title}");
    // Show UI notification
});

// Register for achievement progress updated events
AchievementsHelper.RegisterAchievementProgressUpdatedCallback((achievement, progress) =>
{
    Debug.Log($"Achievement progress updated: {achievement.Title} - {progress:P0}");
    // Update UI progress bar
});

// Register for stat updated events
AchievementsHelper.RegisterStatUpdatedCallback((stat, value) =>
{
    Debug.Log($"Stat updated: {stat.Name} - {value}");
    // Update UI stats display
});

// Don't forget to unregister callbacks when they're no longer needed
AchievementsHelper.UnregisterAchievementUnlockedCallback(myCallback);
```

## Epic Online Services Integration

The Achievements module integrates with EOS to provide:
- Cross-platform achievement tracking
- Stat synchronization across devices
- Automatic stat-based achievement unlocking
- Achievement definitions from EOS Developer Portal

### EOS Achievement Setup

1. Set up achievements on the EOS Developer Portal
2. Define stat thresholds for each achievement
3. Run the game to sync definitions
4. Use EOS interface to view achievement stats and progress

### Working with EOS Stats

EOS uses a system where achievements are unlocked based on stat thresholds. The module automatically handles this by:
1. Tracking stat updates
2. Checking for threshold-based achievements
3. Automatically unlocking achievements when thresholds are met

## Advanced Usage

### Implementing Custom Providers

You can implement your own achievements provider by implementing the `IAchievementsProvider` interface:

```csharp
public class CustomAchievementsProvider : IAchievementsProvider
{
    public void Initialize(Action<bool> onComplete = null)
    {
        // Initialize your provider
        onComplete?.Invoke(true);
    }
    
    public void QueryAchievements(Action<List<Achievement>, string> onComplete)
    {
        // Implement achievements query
    }
    
    // Implement other required methods...
    
    public string GetProviderName()
    {
        return "CustomProvider";
    }
    
    public bool IsAvailable()
    {
        // Check if the provider is available
        return true;
    }
}

// Add your custom provider
AchievementsHelper.AddProvider(new CustomAchievementsProvider());
```

### Handling Offline Mode

The Achievements module automatically handles offline mode:
- Achievements and stats are cached locally when unlocked/updated
- Data is synchronized when the user goes online
- Events are fired for both local and synchronized achievements

### Statistical Aggregation

Stats can be aggregated in various ways depending on their nature:
- **Sum**: Values are added together (e.g., total kills)
- **Max**: The highest value is kept (e.g., highest score)
- **Min**: The lowest value is kept (e.g., fastest time)
- **Average**: Values are averaged (e.g., average accuracy)
- **Latest**: Only the latest value is kept (e.g., last login time)

## Performance Considerations

- Local cache is used to minimize network operations
- Stats and achievements are batched for efficient updates
- Queries are cached to reduce API calls
- Event handlers are only called when necessary
- Automatic stat-based achievement checks are optimized

## Thread Safety

The Achievements module is designed to be used from the main thread only.

## Error Handling

All methods that perform network operations take a callback with an error parameter:

```csharp
AchievementsHelper.UnlockAchievement("achievement_id", (success, error) =>
{
    if (!success)
    {
        // Handle error
        Debug.LogError($"Error unlocking achievement: {error}");
        
        if (error.Contains("offline"))
        {
            // Handle offline case
            ShowOfflineNotification();
        }
        else if (error.Contains("not found"))
        {
            // Handle achievement not found
            LogInvalidAchievement();
        }
    }
});
```

## Examples

See the `AchievementsExample.cs` class for a complete example of using the Achievements module.

## License

Copyright © 2024 RecipeRage. All rights reserved. 