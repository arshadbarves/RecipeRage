# RecipeRage

RecipeRage is a Unity 2022.3 multiplayer cooking game built around NGO networking, EOS services, UI Toolkit screens, and VContainer-based dependency injection.

## Current Runtime Shape

- Boot scene: `Assets/Scenes/Bootstrap.unity`
- Root composition: `Assets/Prefabs/General/GameBootstrap.prefab`
- Root DI scope: `Assets/Scripts/Gameplay/Bootstrap/GameLifetimeScope.cs`
- Session DI scope: `Assets/Scripts/Gameplay/Bootstrap/SessionLifetimeScope.cs`
- Runtime state flow:
  - `BootstrapState -> LoginState/MainMenuState -> MatchmakingState -> GameplayState -> GameOverState`

The project is state-driven. Matchmaking is owned by `MatchmakingState`, gameplay scene objects are bridged back into app systems through `MatchContext`, and UI screens are managed by `UIService` plus `UIScreenStackManager`.

## Source Of Truth Docs

Read these first:

- [Documentation/README.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/README.md)
- [PROJECT_MEMORY.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/Architecture/PROJECT_MEMORY.md)
- [CURRENT_CODEBASE_AUDIT.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md)
- [gameplay-scene-setup.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/Guides/gameplay-scene-setup.md)
- [KitchenClash_GDD_v3.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/KitchenClash_GDD_v3.md)

Older architecture notes and implementation milestones now live under `Documentation/Archive/`.

## Key Folders

```text
Assets/Scripts/Core/           Shared engine-level systems
Assets/Scripts/Gameplay/       App states, gameplay runtime, UI, networking
Assets/Resources/UI/           UI Toolkit templates and styles
Documentation/Architecture/    Current architecture references
Documentation/Guides/          Active setup and implementation guides
Documentation/Archive/         Historical docs and superseded notes
```

## Development Notes

- The worktree may contain active gameplay/UI changes outside this cleanup task. Do not assume a clean tree.
- Root-owned gameplay services that need to be available in both root UI and session context are resolved through root DI and exposed via `SessionContext`.
- Third-party/plugin assets under `Assets/Firebase`, `Assets/ExternalDependencyManager`, and `Assets/Samples` are not the same as project-owned gameplay code.

## Build And Test

- Gameplay build:
  - `dotnet build RecipeRage.Gameplay.csproj -nologo`
- EditMode tests:
  - `dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo`
- Check documentation
- Create an issue
- Contact support@reciperage.com

## License

Copyright © 2024 RecipeRage. All rights reserved. 
