# Friends System UI Components

This folder contains UI components for the RecipeRage Friends system built with UI Toolkit.

## Overview

The Friends system UI provides a complete user interface for interacting with the Friends system. The UI is built using Unity's UI Toolkit system, providing a modern, flexible, and responsive interface.

Key features:
- Display friends list with online status
- Chat with friends
- Send and receive game invites
- Add friends via friend codes
- Manage friend requests
- View friend profiles

## Architecture

The UI components follow a modular architecture:

```
Assets/Scripts/Modules/Friends/UI/
│
├── Components/                # UI components
│   ├── FriendsUIComponent.cs  # Base class for all components
│   ├── FriendsListComponent.cs # Friends list
│   ├── FriendRequestsComponent.cs # Friend requests
│   ├── AddFriendComponent.cs  # Add friend screen
│   ├── FriendProfileComponent.cs # Friend profile
│   └── ChatComponent.cs       # Chat screen
│
├── Templates/                 # UI Templates (UXML)
│   ├── FriendsList.uxml       # Friends list template
│   ├── FriendItem.uxml        # Friend item template
│   ├── RequestList.uxml       # Requests list template
│   ├── RequestItem.uxml       # Request item template
│   ├── AddFriend.uxml         # Add friend template
│   ├── FriendProfile.uxml     # Friend profile template
│   ├── Chat.uxml              # Chat screen template
│   ├── TextMessage.uxml       # Text message template
│   ├── SystemMessage.uxml     # System message template
│   └── GameInvite.uxml        # Game invite template
│
├── Styles/                    # UI Styles (USS)
│   ├── FriendsCommon.uss      # Common styles
│   ├── FriendsList.uss        # Friends list styles
│   ├── FriendRequests.uss     # Friend requests styles
│   ├── AddFriend.uss          # Add friend styles
│   ├── FriendProfile.uss      # Friend profile styles
│   └── Chat.uss               # Chat styles
│
└── FriendsUIManager.cs        # Main UI manager
```

## How to Use

### Integration

1. Add the FriendsUIManager to a game object in your scene.
2. Assign UI Document and UI components in the inspector.
3. Add UI references to the main UI components.
4. Initialize the system and show the UI when needed.

```csharp
// Get a reference to the UI manager
FriendsUIManager friendsUI = FindObjectOfType<FriendsUIManager>();

// Show the UI
friendsUI.Show();
```

### Customization

The UI can be customized by modifying the UXML and USS files in the Templates and Styles folders.

## Components

### FriendsUIComponent

Base class for all Friends UI components, providing common functionality:
- Initialization
- Show/Hide
- Element finding
- Callback registration

### FriendsListComponent

Displays the list of friends with their online status and activity.
- Shows online status with color indicators
- Displays current activity
- Allows removing friends
- Shows joinable activities
- Empty state for no friends

### FriendRequestsComponent

Displays incoming friend requests.
- Shows sender information
- Displays request message
- Accept/Reject buttons
- Timestamps
- Empty state for no requests

### AddFriendComponent

Provides UI for adding friends via friend codes.
- Displays user's own friend code
- Input field for entering a friend code
- Optional message input
- Input validation
- Error and success messages

### FriendProfileComponent

Displays detailed information about a friend.
- Name and friend code
- Online status and activity
- Notes (editable)
- Remove friend button
- Send message button
- Join game button (if applicable)

### ChatComponent

Provides a UI for chatting with friends.
- Message history
- Text input
- Send button
- Status indicator
- Game invite button
- Support for different message types:
  - Text messages
  - System messages
  - Game invites

## Event Handling

All UI components expose events that can be subscribed to, allowing for easy integration with game logic.

Example:
```csharp
chatComponent.OnGameInviteAccepted += (friendId, gameData) =>
{
    // Handle game invite acceptance
    JoinGame(friendId, gameData);
};
```

## Styling

The UI uses a consistent set of styles defined in USS files. The default styles follow a dark theme with accent colors, but can be easily customized by modifying the USS files.

## Templates

Each component uses a set of UXML templates for its UI elements. These templates can be customized to change the layout or appearance of the UI.

## Accessibility

The UI is designed with accessibility in mind:
- Proper focus navigation
- Keyboard shortcuts
- Screen reader support
- Color contrast for readability

## Best Practices

When extending the UI:
1. Create a new component that inherits from FriendsUIComponent
2. Create a UXML template for the new component
3. Create a USS file for styling the new component
4. Register the component with the FriendsUIManager
5. Subscribe to relevant events from FriendsHelper and ChatHelper

## Integration with Game Events

The UI components can easily be integrated with game events by subscribing to their events. For example:

```csharp
// Subscribe to game invite events
chatComponent.OnGameInviteAccepted += (friendId, gameData) =>
{
    // Parse game data
    var data = JsonUtility.FromJson<GameData>(gameData);
    
    // Join game
    GameManager.JoinGame(data.gameId);
};
```

## License

Copyright © 2024 RecipeRage. All rights reserved. 