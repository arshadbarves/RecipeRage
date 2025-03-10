# RecipeRage Leaderboards Module

The Leaderboards module provides a unified system for tracking and displaying player rankings across different leaderboards. It integrates with Epic Online Services (EOS) to enable global and friend-specific leaderboard functionality.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Basic Usage](#basic-usage)
- [Advanced Features](#advanced-features)
- [Epic Online Services Integration](#epic-online-services-integration)
- [Caching and Performance](#caching-and-performance)
- [Thread Safety](#thread-safety)
- [Examples](#examples)
- [License](#license)

## Overview

The Leaderboards module allows game developers to easily implement competitive features in their games, enabling players to compare their performance with friends and global players.

### Key Features

- **Global Leaderboards**: Display rankings of all players
- **Friend Leaderboards**: Filter leaderboards to show only friends
- **Multiple Stats**: Create different leaderboards based on different stats
- **Score Submission**: Submit scores to leaderboards with optional metadata
- **Paging**: Retrieve leaderboard entries in customizable page sizes
- **Player Lookup**: Find a specific player's ranking
- **Current User Tracking**: Easily identify the current user's position
- **Stat-Based Auto-Submission**: Automatically submit scores when stats change
- **Event System**: Receive notifications for leaderboard operations
- **Error Handling**: Robust error handling and reporting
- **Caching**: Local caching for faster access and offline functionality

## Architecture

The Leaderboards module follows a provider-based architecture, allowing integration with different leaderboard services (e.g., EOS, Steam, etc.).

### Directory Structure

```
Assets/Scripts/Modules/Leaderboards/
│
├── Interfaces/                    # Interface definitions
│   ├── ILeaderboardsService.cs    # Main service interface
│   └── ILeaderboardsProvider.cs   # Provider interface
│
├── Data/                          # Data models
│   ├── LeaderboardEntry.cs        # Player entry on leaderboard
│   ├── LeaderboardDefinition.cs   # Leaderboard metadata
│   └── LeaderboardStatInfo.cs     # Stat info for leaderboard
│
├── Core/                          # Core implementations
│   └── LeaderboardsService.cs     # Main service implementation
│
├── Providers/                     # Provider implementations
│   └── EOS/
│       └── EOSLeaderboardsProvider.cs # EOS implementation
│
├── Utils/                         # Utility classes
│   └── LeaderboardsFormatter.cs   # Formatting utilities
│
└── LeaderboardsHelper.cs          # Static helper for easy access
```

### Components Overview

- **LeaderboardsHelper**: Static helper class for easy access to leaderboard functionality
- **LeaderboardsService**: Core service that manages providers and leaderboard operations
- **EOSLeaderboardsProvider**: Provider implementation for Epic Online Services
- **LeaderboardDefinition**: Metadata about a leaderboard, including ordering, display types, etc.
- **LeaderboardEntry**: Representation of a player's position and score on a leaderboard
- **LeaderboardStatInfo**: Defines relationship between a stat and a leaderboard
- **LeaderboardsFormatter**: Utilities for formatting leaderboard data (ranks, scores, timestamps)

## Getting Started

### Prerequisites

- Epic Online Services SDK (integrated with the project)
- Authentication module initialized and signed in user
- Unity 2019.4 or newer

### Initialization

Initialize the Leaderboards module early in your game:

```csharp
// Initialize the leaderboards system
LeaderboardsHelper.Initialize(success => 
{
    if (success)
    {
        Debug.Log("Leaderboards system initialized successfully");
    }
    else
    {
        Debug.LogError("Failed to initialize leaderboards system");
    }
});
```

## Basic Usage

### Querying Leaderboards

Retrieve entries from a leaderboard:

```csharp
// Get top 10 entries from a leaderboard
LeaderboardsHelper.GetLeaderboardEntries("reciperage_highscores", 1, 10, entries =>
{
    Debug.Log($"Retrieved {entries.Count} entries");
    foreach (var entry in entries)
    {
        Debug.Log($"Rank {entry.Rank}: {entry.DisplayName} - {entry.Score}");
    }
});
```

### Querying Friend Leaderboards

Retrieve entries from a friend-filtered leaderboard:

```csharp
// Get entries from a leaderboard filtered to friends
LeaderboardsHelper.GetLeaderboardEntriesForFriends("reciperage_highscores", entries =>
{
    Debug.Log($"Retrieved {entries.Count} friend entries");
    foreach (var entry in entries)
    {
        Debug.Log($"Rank {entry.Rank}: {entry.DisplayName} - {entry.Score}");
    }
});
```

### Submitting Scores

Submit a score to a leaderboard:

```csharp
// Submit a score to a leaderboard
LeaderboardsHelper.SubmitScore("reciperage_highscores", 1000, success =>
{
    if (success)
    {
        Debug.Log("Score submitted successfully");
    }
    else
    {
        Debug.LogError("Failed to submit score");
    }
});
```

### Getting Current User's Position

Find the current user's position on a leaderboard:

```csharp
// Get the current user's entry on a leaderboard
LeaderboardsHelper.GetCurrentUserLeaderboardEntry("reciperage_highscores", entry =>
{
    if (entry != null)
    {
        Debug.Log($"You are ranked {entry.Rank} with score {entry.Score}");
    }
    else
    {
        Debug.Log("You are not on this leaderboard yet");
    }
});
```

## Advanced Features

### Automatic Score Submission from Stats

Register a stat to automatically update a leaderboard when the stat changes:

```csharp
// Register a stat for automatic leaderboard updates
LeaderboardsHelper.RegisterStatForLeaderboard(
    "player_kills",                           // Stat name
    "reciperage_kills_leaderboard",           // Leaderboard ID
    true,                                     // Auto-submit
    LeaderboardStatTransformType.Multiply,    // Transform function
    100);                                     // Transform value

// Now when you update the stat, it will automatically submit to the leaderboard
LeaderboardsHelper.UpdateStat("player_kills", 10); // Will submit 1000 to the leaderboard
```

### Event Handling

Subscribe to leaderboard events:

```csharp
// Register for leaderboard query events
LeaderboardsHelper.RegisterOnLeaderboardQueried((leaderboardId, entries) =>
{
    Debug.Log($"Leaderboard {leaderboardId} queried with {entries.Count} entries");
});

// Register for score submission events
LeaderboardsHelper.RegisterOnScoreSubmitted((leaderboardId, score, success) =>
{
    Debug.Log($"Score {score} submitted to {leaderboardId}: {success}");
});

// Register for score submission failure events
LeaderboardsHelper.RegisterOnScoreSubmissionFailed((leaderboardId, score, error) =>
{
    Debug.LogError($"Score {score} failed to submit to {leaderboardId}: {error}");
});
```

Remember to unregister when you're done:

```csharp
// Unregister from events
private void OnDisable()
{
    LeaderboardsHelper.UnregisterOnLeaderboardQueried(OnLeaderboardQueried);
    LeaderboardsHelper.UnregisterOnScoreSubmitted(OnScoreSubmitted);
    LeaderboardsHelper.UnregisterOnScoreSubmissionFailed(OnScoreSubmissionFailed);
}
```

## Epic Online Services Integration

The Leaderboards module integrates with EOS through the `EOSLeaderboardsProvider`. It uses both the EOS Leaderboards and Stats interfaces to provide leaderboard functionality.

### EOS Leaderboards Setup

1. Create leaderboards in the EOS Developer Portal
2. Specify the stat that will drive each leaderboard
3. Configure ordering (ascending/descending) in EOS

### EOS Limitations and Workarounds

- EOS doesn't support direct progress updates, only full completions
- Friend leaderboards require friends list integration
- EOS doesn't provide built-in UI for leaderboards, you must build your own

## Caching and Performance

The Leaderboards module implements several caching mechanisms to improve performance:

- Leaderboard definitions are cached after the first query
- Recent leaderboard entries are cached for faster access
- Mappings between stats and leaderboards are cached for auto-submission

### Optimization Tips

- Query leaderboard definitions once at startup, not repeatedly
- Use reasonable page sizes (10-25 entries) for leaderboard queries
- Cache frequently accessed leaderboards and refresh periodically
- Consider using the stat-based auto-submission only for important stats

## Thread Safety

The Leaderboards module is designed for use from the main thread only. All callbacks from asynchronous operations are delivered on the main thread.

## Examples

An example implementation is provided in `Assets/Scripts/Examples/LeaderboardsExample.cs`. This example demonstrates:

- Initialization
- Querying leaderboards
- Submitting scores
- Displaying leaderboard entries
- Event handling

## License

Copyright © 2023 RecipeRage. All rights reserved.

---

# Quick Reference

## Initialization
```csharp
LeaderboardsHelper.Initialize(success => { /* handle result */ });
```

## Querying Leaderboards
```csharp
// Get global leaderboard
LeaderboardsHelper.GetLeaderboardEntries(leaderboardId, startRank, count, entries => { /* handle entries */ });

// Get friend leaderboard
LeaderboardsHelper.GetLeaderboardEntriesForFriends(leaderboardId, entries => { /* handle entries */ });

// Get current user entry
LeaderboardsHelper.GetCurrentUserLeaderboardEntry(leaderboardId, entry => { /* handle entry */ });
```

## Submitting Scores
```csharp
// Submit score
LeaderboardsHelper.SubmitScore(leaderboardId, score, success => { /* handle result */ });

// Register stat for auto-submission
LeaderboardsHelper.RegisterStatForLeaderboard(statName, leaderboardId, autoSubmit, transformType, transformValue);

// Update stat (potentially submits to leaderboard)
LeaderboardsHelper.UpdateStat(statName, statValue);
```

## Event Handling
```csharp
// Register for events
LeaderboardsHelper.RegisterOnLeaderboardQueried(OnLeaderboardQueried);
LeaderboardsHelper.RegisterOnScoreSubmitted(OnScoreSubmitted);
LeaderboardsHelper.RegisterOnScoreSubmissionFailed(OnScoreSubmissionFailed);

// Unregister from events
LeaderboardsHelper.UnregisterOnLeaderboardQueried(OnLeaderboardQueried);
LeaderboardsHelper.UnregisterOnScoreSubmitted(OnScoreSubmitted);
LeaderboardsHelper.UnregisterOnScoreSubmissionFailed(OnScoreSubmissionFailed);
``` 