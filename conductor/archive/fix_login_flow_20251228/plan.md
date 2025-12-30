# Plan: Fix Initial Login Flow

## Phase 1: Persistence & Logic Update
- [x] Task: Update EOSAuthService with Persistence e2f0eaa
    - Inject `ISaveService` into `EOSAuthService`.
    - Update `LoginAsync` to save `LastLoginMethod` on success.
    - Update `LogoutAsync` to clear `LastLoginMethod`.
- [x] Task: Update ServiceContainer Registration e054578
    - Pass `SaveService` to `EOSAuthService` constructor in `ServiceContainer.cs`.
- [x] Task: Update BootstrapState Logic 996149f
    - Modify `InitializeGameSequence` to check `SaveService.GetSettings().LastLoginMethod`.
    - Only call `LoginAsync` if a method is saved.
- [ ] Task: Conductor - User Manual Verification 'Persistence & Logic Update' (Protocol in workflow.md)
