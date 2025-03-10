# Cloud Storage Module

A modular cross-platform cloud storage system for RecipeRage that enables saving and loading user data across devices.

## Overview

The Cloud Storage module provides a comprehensive solution for game save management across different devices. It integrates with Epic Online Services (EOS) to provide player data storage functionality, allowing players to continue their game progress across multiple devices.

Key features:
- Cross-platform data synchronization
- Transparent local caching with cloud backup
- Support for multiple storage providers (currently EOS)
- Binary, text, and JSON data support
- File metadata and storage quota management
- Event-based workflow for save/load operations
- Conflict resolution with smart sync
- Offline support with automatic sync when online

## Architecture

The Cloud Storage module follows a modular design with clear separation of concerns:

```
Assets/Scripts/Modules/Cloud/
│
├── Interfaces/               # Interface definitions
│   ├── ICloudStorageService.cs   # Main cloud storage interface
│   └── ICloudStorageProvider.cs  # Provider interface
│
├── Data/                     # Data models
│   ├── CloudFileMetadata.cs      # File metadata
│   └── CloudStorageStatus.cs     # Service status
│
├── Core/                     # Core implementations
│   └── CloudStorageService.cs    # Default service implementation
│
├── Providers/                # Provider implementations
│   └── EOS/
│       └── EOSPlayerDataStorageProvider.cs  # EOS implementation
│
└── CloudStorageHelper.cs     # Static helper for easy access
```

## Getting Started

### Initialization

Initialize the cloud storage system early in your application:

```csharp
// Initialize cloud storage
CloudStorageHelper.Initialize(success =>
{
    if (success)
    {
        Debug.Log("Cloud storage initialized successfully");
    }
    else
    {
        Debug.LogError("Failed to initialize cloud storage");
    }
});
```

### Prerequisites

1. Epic Online Services SDK for Unity
2. User must be authenticated with Epic Online Services
3. The Auth module must be initialized before Cloud Storage

## Basic Usage

### Saving Data

```csharp
// Save a text file
CloudStorageHelper.SaveText("player_notes.txt", "This is a player note", (success, error) =>
{
    if (success)
    {
        Debug.Log("Text saved successfully");
    }
    else
    {
        Debug.LogError($"Failed to save text: {error}");
    }
});

// Save a binary file
byte[] data = new byte[] { 0x01, 0x02, 0x03 };
CloudStorageHelper.SaveFile("binary_data.bin", data, (success, error) =>
{
    if (success)
    {
        Debug.Log("Binary data saved successfully");
    }
    else
    {
        Debug.LogError($"Failed to save binary data: {error}");
    }
});

// Save a JSON object
var playerData = new PlayerData
{
    Name = "Player1",
    Level = 10,
    Score = 5000,
    LastPlayed = DateTime.UtcNow
};

CloudStorageHelper.SaveJson("player_data.json", playerData, (success, error) =>
{
    if (success)
    {
        Debug.Log("JSON data saved successfully");
    }
    else
    {
        Debug.LogError($"Failed to save JSON data: {error}");
    }
});
```

### Loading Data

```csharp
// Load a text file
CloudStorageHelper.LoadText("player_notes.txt", (text, error) =>
{
    if (text != null)
    {
        Debug.Log($"Loaded text: {text}");
    }
    else
    {
        Debug.LogError($"Failed to load text: {error}");
    }
});

// Load a binary file
CloudStorageHelper.LoadFile("binary_data.bin", (data, error) =>
{
    if (data != null)
    {
        Debug.Log($"Loaded binary data: {data.Length} bytes");
    }
    else
    {
        Debug.LogError($"Failed to load binary data: {error}");
    }
});

// Load a JSON object
CloudStorageHelper.LoadJson<PlayerData>("player_data.json", (playerData, error) =>
{
    if (playerData != null)
    {
        Debug.Log($"Loaded player data: {playerData.Name}, Level {playerData.Level}");
    }
    else
    {
        Debug.LogError($"Failed to load player data: {error}");
    }
});
```

### File Management

```csharp
// Delete a file
CloudStorageHelper.DeleteFile("outdated_file.txt", (success, error) =>
{
    if (success)
    {
        Debug.Log("File deleted successfully");
    }
    else
    {
        Debug.LogError($"Failed to delete file: {error}");
    }
});

// List all files
CloudStorageHelper.ListFiles((files, error) =>
{
    if (files != null)
    {
        Debug.Log($"Found {files.Count} files:");
        foreach (var file in files)
        {
            Debug.Log($"- {file.FileName} ({file.GetReadableSize()}, modified: {file.LastModified})");
        }
    }
    else
    {
        Debug.LogError($"Failed to list files: {error}");
    }
});

// Check if a file exists
CloudStorageHelper.FileExists("player_data.json", exists =>
{
    Debug.Log($"File exists: {exists}");
});

// Get file metadata
CloudStorageHelper.GetFileMetadata("player_data.json", (metadata, error) =>
{
    if (metadata != null)
    {
        Debug.Log($"File: {metadata.FileName}");
        Debug.Log($"Size: {metadata.GetReadableSize()}");
        Debug.Log($"Last Modified: {metadata.LastModified}");
        Debug.Log($"Provider: {metadata.Provider}");
    }
    else
    {
        Debug.LogError($"Failed to get metadata: {error}");
    }
});
```

### Synchronization

```csharp
// Synchronize local files with cloud storage
CloudStorageHelper.Sync((success, syncCount, error) =>
{
    if (success)
    {
        Debug.Log($"Sync completed successfully: {syncCount} files synced");
    }
    else
    {
        Debug.LogError($"Sync failed: {error}");
    }
});

// Get storage quota information
CloudStorageHelper.GetStorageQuota((used, total, error) =>
{
    if (string.IsNullOrEmpty(error))
    {
        float percentage = total > 0 ? (float)used / total * 100 : 0;
        Debug.Log($"Storage: {used} / {total} bytes ({percentage:F1}%)");
    }
    else
    {
        Debug.LogError($"Failed to get quota: {error}");
    }
});
```

## Event Handling

The Cloud Storage module provides events for tracking storage operations:

```csharp
// Register for file saved events
CloudStorageHelper.RegisterFileSavedCallback((fileName, success) =>
{
    Debug.Log($"File {fileName} saved: {success}");
});

// Register for file loaded events
CloudStorageHelper.RegisterFileLoadedCallback((fileName, success) =>
{
    Debug.Log($"File {fileName} loaded: {success}");
});

// Register for file deleted events
CloudStorageHelper.RegisterFileDeletedCallback((fileName, success) =>
{
    Debug.Log($"File {fileName} deleted: {success}");
});

// Register for sync completed events
CloudStorageHelper.RegisterSyncCompletedCallback((success, syncCount, error) =>
{
    Debug.Log($"Sync completed: {success}, Files synced: {syncCount}, Error: {error}");
});

// Don't forget to unregister callbacks when they're no longer needed
CloudStorageHelper.UnregisterFileSavedCallback(myCallback);
```

## Epic Online Services Integration

The Cloud Storage module integrates with Epic Online Services Player Data Storage, which provides:
- Cross-platform synchronization
- Per-user storage (200 MB per user)
- No additional server setup required
- Automatic conflict resolution
- User authentication through EOS

### Requirements

1. User must be signed in with Epic Online Services
2. The EOS SDK must be properly set up
3. The user must have an internet connection for initial sync

## Advanced Usage

### Implementing Custom Providers

You can implement your own storage provider by implementing the `ICloudStorageProvider` interface:

```csharp
public class CustomStorageProvider : ICloudStorageProvider
{
    public void Initialize(Action<bool> onComplete = null)
    {
        // Initialize your provider
        onComplete?.Invoke(true);
    }
    
    public void SaveFile(string fileName, byte[] data, Action<bool, string> onComplete = null)
    {
        // Implement file saving
    }
    
    public void LoadFile(string fileName, Action<byte[], string> onComplete)
    {
        // Implement file loading
    }
    
    // Implement other required methods...
    
    public string GetProviderName()
    {
        return "CustomStorage";
    }
    
    public bool IsAvailable()
    {
        // Check if the provider is available
        return true;
    }
}

// Add your custom provider
CloudStorageHelper.AddProvider(new CustomStorageProvider());
```

### Handling Offline Mode

The Cloud Storage module automatically handles offline mode:
- Files are saved locally when offline
- Files are synchronized when the user goes online
- Local cache is used for fast loading

### Conflict Resolution

When conflicts occur (both local and cloud versions have been modified):
- By default, the most recent version is used
- Files are automatically backed up before being overwritten
- Custom conflict resolution can be implemented by creating a custom provider

## Performance Considerations

- Local cache is used to minimize network operations
- Files are loaded from local cache when available
- Chunked uploads/downloads are used for large files
- Files are compressed before being sent to the cloud
- Metadata cache is maintained to avoid redundant API calls

## Thread Safety

The Cloud Storage module is designed to be used from the main thread only.

## Error Handling

All methods that perform network operations take a callback with an error parameter:

```csharp
CloudStorageHelper.SaveFile("example.txt", data, (success, error) =>
{
    if (!success)
    {
        // Handle error
        Debug.LogError($"Error saving file: {error}");
        
        if (error.Contains("offline"))
        {
            // Handle offline case
            ShowOfflineNotification();
        }
        else if (error.Contains("quota"))
        {
            // Handle quota exceeded
            ShowQuotaExceededDialog();
        }
    }
});
```

## Examples

See the `CloudStorageExample` class for a complete example of using the Cloud Storage module.

## License

Copyright © 2024 RecipeRage. All rights reserved. 