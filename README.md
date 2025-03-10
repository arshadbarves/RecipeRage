# RecipeRage

A multiplayer cooking competition game with a modular, extensible framework and rich social features.

## Game Overview

RecipeRage is a fast-paced multiplayer cooking competition where players compete in various game modes to become the ultimate chef!

Key gameplay features:
- Multiple game modes (Classic, Time Attack, Team Battle)
- Character selection with unique abilities
- In-game shop with skins and power-ups
- Season Pass and progression system
- Real-time multiplayer using Epic Online Services
- Rich social features with friends system and chat

## Technical Architecture

Behind the scenes, RecipeRage is built on a comprehensive game framework designed with modularity in mind, allowing for clean separation of concerns and extensibility.

Key technical features:
- Modular architecture with clear separation of concerns
- Comprehensive friends system with P2P communication
- Chat and messaging functionality
- Game invites and social interactions
- Robust logging system
- Cross-platform compatibility
- Extensible design for custom game features

## Architecture

RecipeRage follows a modular architecture where each major feature is encapsulated in its own module:

```
Assets/
├── Scripts/
│   ├── Modules/              # Core modules
│   │   ├── Friends/          # Friends system
│   │   ├── Logging/          # Logging system
│   │   ├── Auth/             # Authentication (placeholder)
│   │   └── ...               # Other modules
│   │
│   ├── Core/                 # Core game systems
│   ├── UI/                   # Common UI components
│   ├── Utils/                # Utility classes
│   └── Examples/             # Example implementations
│
├── Plugins/                  # Third-party plugins
│   └── EOS/                  # Epic Online Services SDK
│
└── Resources/                # Game resources
```

### Module Structure

Each module follows a consistent structure:

```
Module/
├── Core/                 # Core implementations
├── Interfaces/           # Interface definitions
├── Data/                 # Data models
├── UI/                   # UI components (if applicable)
│   ├── Components/       # UI component implementations
│   ├── Templates/        # UI Templates (UXML)
│   └── Styles/           # UI Styles (USS)
├── Utils/                # Module-specific utilities
└── README.md             # Module documentation
```

## Modules

### Friends Module

The Friends module provides a complete P2P-based friends system that works across different authentication providers. It enables players to connect with each other through friend codes, manage friend relationships, see online status, chat, and send game invites.

Key features:
- Friend code generation and sharing
- Friend requests management
- Presence and status updates
- P2P connections using EOS
- Chat messaging
- Game invites
- Cross-auth support
- Persistent data storage
- UI Toolkit components

[Learn more about the Friends module](Assets/Scripts/Modules/Friends/README.md)

### Logging Module

The Logging module provides a centralized way to log messages, warnings, errors, and exceptions throughout the application. It supports different output destinations and log levels, allowing for flexible logging configurations.

Key features:
- Multiple log levels (Debug, Info, Warning, Error)
- Multiple output destinations (Console, File)
- Structured logging with contextual information
- Timestamp and module tagging
- Thread-safe logging
- Event-based notifications
- Log filtering by level
- Historical log access

[Learn more about the Logging module](Assets/Scripts/Modules/Logging/README.md)

## Getting Started

### Prerequisites

- Unity 2022.3 or newer
- Epic Online Services SDK
- .NET 4.x or newer
- Visual Studio 2022 or JetBrains Rider
- Git LFS

### Installation

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/RecipeRage-New.git
   git lfs pull
   ```
2. Open the project in Unity
3. Import the Epic Online Services SDK
4. Configure the EOS SDK with your credentials

### Basic Usage

Initialize the core modules early in your game:

```csharp
void Start()
{
    // Initialize logging
    LogHelper.SetConsoleOutput(true);
    LogHelper.SetFileOutput(true);
    LogHelper.SetLogLevel(LogLevel.Debug);
    
    LogHelper.Info("Main", "Game starting up...");
    
    // Initialize authentication (placeholder)
    // AuthHelper.Initialize(...);
    
    // Initialize friends system after authentication
    FriendsHelper.Initialize(enableDiscovery: true, onComplete: success =>
    {
        if (success)
        {
            LogHelper.Info("Main", "Friends system initialized!");
            
            // Initialize chat
            ChatHelper.Initialize(chatSuccess =>
            {
                if (chatSuccess)
                {
                    LogHelper.Info("Main", "Chat system initialized!");
                }
            });
        }
    });
}
```

## Building

1. Set up build settings:
   - File > Build Settings
   - Add necessary scenes
   - Configure player settings

2. Create a build:
   - Select platform
   - Click "Build"
   - Choose output location

## Examples

The project includes several examples to help you get started:

- `Assets/Scripts/Examples/FriendsExample.cs` - Demonstrates how to use the Friends system
- `Assets/Scripts/Examples/LoggingExample.cs` - Demonstrates how to use the Logging system

## Development

When developing new features:

1. Create a new feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Follow the development guidelines in the documentation

3. Run tests before submitting changes

4. Create a pull request against the develop branch

## Roadmap

For details on planned features and improvements, see our [Roadmap](ROADMAP.md).

## Security

- Never commit sensitive data
- Use environment variables for keys
- Follow security guidelines in documentation
- Report vulnerabilities to security@reciperage.com

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Follow the existing architecture and coding style
2. Document all public APIs with XML comments
3. Include appropriate tests
4. Update the documentation

## Support

For support:
- Check documentation
- Create an issue
- Contact support@reciperage.com

## License

Copyright © 2024 RecipeRage. All rights reserved. 