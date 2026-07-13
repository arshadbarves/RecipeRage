# Plan: System Registration and Dependency Injection Refactor

## Phase 1: Core and Pre-Auth Registration (`RootLifetimeScope`) [checkpoint: 71ec672]
- [x] Task: Audit and Define Interfaces for Core Managers (Localization, UGS Auth) <!-- id: cb9d290 -->
- [x] Task: Update `RootLifetimeScope.cs` to register `ILocalizationManager` and `UGSAuthenticationManager` <!-- id: db64af1 -->
- [x] Task: Implement `ScriptableObject` loading for Core settings (Localization, UGS Config) <!-- id: db64af1 -->
- [x] Task: Register `IPlayerNetworkManager` as a Project-scope singleton <!-- id: db64af1 -->
- [x] Task: Conductor - User Manual Verification 'Core Registration' (Protocol in workflow.md)

## Phase 2: Session-Based Registration (`MenuLifetimeScope`)
- [x] Task: Audit and Define Interfaces for Session Managers (Player, Lobby, Team, UI Stack) <!-- id: b2958ad -->
- [x] Task: Update `MenuLifetimeScope.cs` to register `IPlayerManager`, `ILobbyManager`, and `ITeamManager` <!-- id: 3e0554b -->
- [x] Task: Register `IUIScreenStackManager` for persistent UI navigation <!-- id: 3e0554b -->
- [x] Task: Ensure session-scoped managers are registered as `As<IInterface>` <!-- id: 3e0554b -->
- [x] Task: Conductor - User Manual Verification 'Session Registration' (Protocol in workflow.md)

## Phase 3: Gameplay-Specific Registration (`MatchLifetimeScope`)
- [x] Task: Audit and Define Interfaces for Gameplay Managers (GameState, Score, Order, Spawn, Bot)
- [x] Task: Update `MatchLifetimeScope.cs` to register `IGameStateManager`, `IScoreManager`, and `IOrderManager`
- [x] Task: Register `ISpawnManager` and `IBotManager` using Prefab/Hierarchy injection as appropriate
- [x] Task: Register `IGameplayUIManager` for scene-specific UI logic
- [x] Task: Conductor - User Manual Verification 'Gameplay Registration' (Protocol in workflow.md)

## Phase 4: Verification and Cleanup
- [x] Task: Run project-wide compilation check to ensure no DI circular dependencies
- [x] Task: Validate that all public managers are only accessible via their Interfaces
- [x] Task: Final audit of Assembly dependencies to ensure no "Core" vs "Gameplay" violations
- [x] Task: Conductor - User Manual Verification 'Final Verification' (Protocol in workflow.md)
