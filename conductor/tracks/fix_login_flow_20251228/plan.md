# Plan: Fix Initial Login Flow

## Phase 1: Persistence & Logic Update
- [~] Task: Update EOSAuthService with Persistence
    - Inject `ISaveService` into `EOSAuthService`.
    - Update `LoginAsync` to save `LastLoginMethod` on success.
    - Update `LogoutAsync` to clear `LastLoginMethod`.
- [ ] Task: Update ServiceContainer Registration
    - Pass `SaveService` to `EOSAuthService` constructor in `ServiceContainer.cs`.
- [ ] Task: Update BootstrapState Logic
    - Modify `InitializeGameSequence` to check `SaveService.GetSettings().LastLoginMethod`.
    - Only call `LoginAsync` if a method is saved.
- [ ] Task: Conductor - User Manual Verification 'Persistence & Logic Update' (Protocol in workflow.md)
