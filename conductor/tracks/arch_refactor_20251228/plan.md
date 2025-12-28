# Track Plan: Project-Wide Architectural Refactor

## Phase 1: Foundational Infrastructure
- [x] Task: Standardize shared test mocks and move to a centralized `Tests/Editor/Mocks` directory. [commit: 2fcb925]
- [ ] Task: Refactor `ServiceContainer` to strictly enforce constructor injection and remove static access points.
- [ ] Task: Expand `EventBus` with standardized events for all major service state changes.
- [ ] Task: Conductor - User Manual Verification 'Foundational Infrastructure' (Protocol in workflow.md)

## Phase 2: SDK Decoupling & Service Abstraction
- [ ] Task: Create comprehensive Wrappers/Facades for EOS, Firebase, and Supabase SDKs.
- [ ] Task: Refactor `AuthenticationService` and `RemoteConfigService` to use new SDK wrappers.
- [ ] Task: Refactor `SaveService` and `FriendsService` to use new SDK wrappers and internal providers.
- [ ] Task: Conductor - User Manual Verification 'SDK Decoupling & Service Abstraction' (Protocol in workflow.md)

## Phase 3: UI Framework Consolidation
- [ ] Task: Standardize all `BaseUIScreen` implementations to use the modular `TabSystem` where applicable.
- [ ] Task: Refactor `Lobby`, `Shop`, and `Profile` screens into smaller, reusable UI Components.
- [ ] Task: Ensure all UI Screens utilize the centralized `IUIAnimator` for transitions.
- [ ] Task: Conductor - User Manual Verification 'UI Framework Consolidation' (Protocol in workflow.md)

## Phase 4: Final Validation & Cleanup
- [ ] Task: Execute a full project-wide unit test pass with all mocks.
- [ ] Task: Perform a final code style audit to ensure zero static/singleton pollution in core logic.
- [ ] Task: Conductor - User Manual Verification 'Final Validation & Cleanup' (Protocol in workflow.md)
