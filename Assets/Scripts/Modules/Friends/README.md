# Friends System

A modular P2P-based friends system for RecipeRage that works across different authentication providers.

## Overview

The Friends system enables players to connect with each other through friend codes, manage friend relationships, see online status, and more - all without requiring a dedicated server.

Key features:
- Friend code generation and sharing
- Friend requests management
- Presence and status updates
- P2P connections using EOS
- Chat messaging
- Game invites
- Cross-auth support (works with any auth provider)
- Persistent data storage
- UI Toolkit components

## Architecture

The Friends system follows a modular design with clear separation of concerns:

```
Assets/Scripts/Modules/Friends/
│
├── Core/                 # Core implementations
│   ├── FriendsService.cs     # Manages friend relationships
│   ├── IdentityService.cs    # Manages user identity and friend codes
│   ├── PresenceService.cs    # Manages online status and activity
│   ├── ChatService.cs        # Manages chat messages
│   └── FriendsServiceUpdater.cs  # MonoBehaviour for Update events
│
├── Interfaces/           # Interface definitions
│   ├── IFriendsService.cs    # Friends management interface
│   ├── IIdentityService.cs   # Identity management interface
│   ├── IPresenceService.cs   # Presence management interface
│   ├── IChatService.cs       # Chat service interface
│   └── IP2PNetworkService.cs # Network communication interface
│
├── Data/                 # Data models
│   ├── FriendsDataTypes.cs   # Friend, presence, and request data structures
│   └── ChatMessage.cs        # Chat message data structure
│
├── Network/              # Network communication
│   ├── FriendsNetworkProtocol.cs  # P2P message protocol
│   └── EOSP2PNetworkService.cs    # EOS P2P implementation
│
├── UI/                   # UI components
│   ├── Components/           # UI component implementations
│   ├── Templates/            # UI Templates (UXML)
│   ├── Styles/               # UI Styles (USS)
│   └── FriendsUIManager.cs   # Main UI manager
│
├── Utils/                # Utility classes
│   └── FriendCodeGenerator.cs     # Friend code generation
│
├── FriendsHelper.cs      # Static helper for friend functionality
└── ChatHelper.cs         # Static helper for chat functionality
```

## Getting Started

### Initialization

Initialize the friends system early in your game:

```csharp
// Initialize Friends system after authentication
if (AuthHelper.IsSignedIn())
{
    FriendsHelper.Initialize(enableDiscovery: true, onComplete: success =>
    {
        if (success)
        {
            Debug.Log("Friends system initialized!");
            
            // Subscribe to events
            FriendsHelper.RegisterEvents(
                onFriendAdded: HandleFriendAdded,
                onFriendRequestReceived: HandleFriendRequestReceived,
                onFriendPresenceChanged: HandleFriendPresenceChanged
            );
            
            // Update your status
            FriendsHelper.SetStatus(UserStatus.Online);
            FriendsHelper.SetActivity("Playing RecipeRage", joinable: true);
            
            // Initialize chat system
            ChatHelper.Initialize(chatSuccess =>
            {
                if (chatSuccess)
                {
                    Debug.Log("Chat system initialized!");
                }
            });
        }
    });
}
```

### Managing Friends

```csharp
// Get your friend code to share
string myFriendCode = FriendsHelper.MyFriendCode;

// Send a friend request using a friend code
FriendsHelper.SendFriendRequest("ABCD-1234-EFGH", message: "Let's play together!");

// Accept a friend request
FriendsHelper.AcceptFriendRequest(requestId);

// Reject a friend request
FriendsHelper.RejectFriendRequest(requestId);

// Remove a friend
FriendsHelper.RemoveFriend(friendId);

// Get all friends
List<FriendData> friends = FriendsHelper.GetFriends();

// Get online friends
List<FriendData> onlineFriends = FriendsHelper.GetOnlineFriends();

// Get pending friend requests
List<FriendRequest> requests = FriendsHelper.GetPendingFriendRequests();
```

### Presence and Status

```csharp
// Set your status
FriendsHelper.SetStatus(UserStatus.Playing);

// Set your activity
FriendsHelper.SetActivity("In a match", joinable: true, joinData: "match_id:12345");

// Get a friend's presence
PresenceData presence = FriendsHelper.GetFriendPresence(friendId);
if (presence != null && presence.IsOnline)
{
    Debug.Log($"{presence.DisplayName} is {presence.Status} - {presence.Activity}");
    
    if (presence.IsJoinable)
    {
        // Can join this friend's activity
        string joinData = presence.JoinData;
    }
}
```

### Chat Functionality

```csharp
// Send a text message to a friend
ChatHelper.SendTextMessage(friendId, "Hello, how are you?", success =>
{
    if (success)
    {
        Debug.Log("Message sent!");
    }
});

// Send a game invite
string gameData = "{\"gameMode\":\"Standard\",\"mapId\":\"1\",\"difficulty\":\"Normal\"}";
ChatHelper.SendGameInvite(friendId, gameData, "Join my game!");

// Load chat history
ChatHelper.LoadChatHistory(friendId, 50, messages =>
{
    foreach (var message in messages)
    {
        Debug.Log($"{message.SenderName}: {message.Content}");
    }
});

// Get unread messages count
int unreadCount = ChatHelper.GetUnreadCount(friendId);
int totalUnread = ChatHelper.GetTotalUnreadCount();

// Mark messages as read
ChatHelper.MarkAsRead(friendId);

// Register for chat events
ChatHelper.RegisterEventHandlers(
    onMessageReceived: message =>
    {
        Debug.Log($"New message from {message.SenderName}: {message.Content}");
    },
    onMessageSent: message =>
    {
        Debug.Log($"Message sent to {message.ReceiverId}");
    }
);
```

### UI Components

The Friends system comes with a set of UI Toolkit components that provide a complete user interface for interacting with the system.

```csharp
// Get the Friends UI Manager
FriendsUIManager friendsUI = FindObjectOfType<FriendsUIManager>();

// Show the UI
friendsUI.Show();

// Hide the UI
friendsUI.Hide();
```

For more detailed information about the UI components, see the [UI README](UI/README.md).

## Event Handling

The Friends system uses events to notify the application of important changes:

```csharp
// Register for friends system events
FriendsHelper.RegisterEvents(
    onFriendAdded: friend => {
        Debug.Log($"New friend added: {friend.DisplayName}");
    },
    onFriendRemoved: friendId => {
        Debug.Log($"Friend removed: {friendId}");
    },
    onFriendRequestReceived: request => {
        Debug.Log($"Friend request from {request.SenderName}: {request.Message}");
    },
    onFriendPresenceChanged: (friendId, presence) => {
        Debug.Log($"Friend {presence.DisplayName} is now {presence.Status}");
    }
);

// Don't forget to unregister when done (e.g., in OnDestroy)
FriendsHelper.UnregisterEvents(...);

// Register for chat events
ChatHelper.RegisterEventHandlers(
    onMessageReceived: message => {
        Debug.Log($"Message from {message.SenderName}: {message.Content}");
    },
    onMessageSent: message => {
        Debug.Log($"Message sent to {message.ReceiverId}");
    },
    onChatHistoryLoaded: (friendId, messages) => {
        Debug.Log($"Loaded {messages.Count} messages with {friendId}");
    }
);

// Don't forget to unregister when done
ChatHelper.UnregisterEventHandlers(...);
```

## Friend Codes

Friend codes are generated using a cryptographic hash of the user's ID with a salt, ensuring they are unique but consistent. Codes are formatted as `XXXX-XXXX-XXXX` (12 characters with separators) and use a specific character set that avoids ambiguous characters.

## Technical Details

### P2P Network

The system uses EOS P2P networking for reliable communication between friends. Messages include:
- Ping/pong for connection management
- Friend requests, accepts, and rejects
- Presence updates
- Friend removal notifications
- Chat messages
- Game invites

### Persistence

Friend data and pending requests are stored locally in the persistent data path:
- `FriendsData/friends.json` - List of friends
- `FriendsData/requests.json` - Pending friend requests
- `FriendsData/displayNames.json` - Display name cache
- `FriendsData/friendCodes.json` - Friend code mappings
- `FriendsData/Chats/{friendId}.json` - Chat history with a friend
- `FriendsData/Chats/unread_counts.json` - Unread message counts

## Example

See `Assets/Scripts/Examples/FriendsExample.cs` for a complete example of how to use the Friends system in a game.

## Dependencies

- EOS SDK - Used for P2P networking
- Auth System - Used to get current user information
- Newtonsoft.Json - Used for data serialization
- UI Toolkit - Used for UI components

## Contributing

When extending or modifying the Friends system:
1. Follow the existing module structure
2. Add new functionality through appropriate interfaces
3. Update the documentation
4. Add appropriate tests

## License

Copyright © 2024 RecipeRage. All rights reserved. 