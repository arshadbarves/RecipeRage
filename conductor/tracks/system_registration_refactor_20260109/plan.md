# Plan: System Registration and Dependency Injection Refactor

## Phase 1: Core and Pre-Auth Registration (`GameLifetimeScope`)
- [x] Task: Audit and Define Interfaces for Core Managers (Localization, UGS Auth) <!-- id: cb9d290 -->
- [x] Task: Update `GameLifetimeScope.cs` to register `ILocalizationManager` and `UGSAuthenticationManager` <!-- id: db64af1 -->
- [x] Task: Implement `ScriptableObject` loading for Core settings (Localization, UGS Config) <!-- id: db64af1 -->
- [x] Task: Register `IPlayerNetworkManager` as a Project-scope singleton <!-- id: db64af1 -->
- [ ] Task: Conductor - User Manual Verification 'Core Registration' (Protocol in workflow.md)

## Phase 2: Session-Based Registration (`SessionLifetimeScope`)
- [ ] Task: Audit and Define Interfaces for Session Managers (Player, Lobby, Team, UI Stack)
- [ ] Task: Update `SessionLifetimeScope.cs` to register `IPlayerManager`, `ILobbyManager`, and `ITeamManager`
- [ ] Task: Register `IUIScreenStackManager` for persistent UI navigation
- [ ] Task: Ensure session-scoped managers are registered as `As<IInterface>`
- [ ] Task: Conductor - User Manual Verification 'Session Registration' (Protocol in workflow.md)

## Phase 3: Gameplay-Specific Registration (`GameplayLifetimeScope`)
- [ ] Task: Audit and Define Interfaces for Gameplay Managers (GameState, Score, Order, Spawn, Bot)
- [ ] Task: Update `GameplayLifetimeScope.cs` to register `IGameStateManager`, `IScoreManager`, and `IOrderManager`
- [ ] Task: Register `ISpawnManager` and `IBotManager` using Prefab/Hierarchy injection as appropriate
- [ ] Task: Register `IGameplayUIManager` for scene-specific UI logic
- [ ] Task: Conductor - User Manual Verification 'Gameplay Registration' (Protocol in workflow.md)

## Phase 4: Verification and Cleanup
- [ ] Task: Run project-wide compilation check to ensure no DI circular dependencies
- [ ] Task: Validate that all public managers are only accessible via their Interfaces
- [ ] Task: Final audit of Assembly dependencies to ensure no "Core" vs "Gameplay" violations
- [ ] Task: Conductor - User Manual Verification 'Final Verification' (Protocol in workflow.md)
