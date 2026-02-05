# Story 1.1: Guest Play Without Account Creation

Status: review

---

## Story

As a new player,
I want to start playing immediately without creating an account,
So that I can experience the game with zero friction.

---

## Acceptance Criteria

### AC1: Immediate Guest Play Entry
**Given** the player has downloaded the app
**When** they launch it for the first time
**Then** they can tap "Guest Play" and immediately enter the main menu
**And** an EOS Device ID is created and authenticated with Epic Online Services

### AC2: Guest Progress Persistence
**Given** the player is playing as a guest
**When** they close and reopen the app
**Then** their EOS Device ID session is restored automatically

### AC3: Match History Tracking
**Given** the player is using a guest account
**When** they participate in matches
**Then** their match history and basic stats are tracked via PlayerDataService

---

## Tasks / Subtasks

- [x] Use existing EOS Device Login for guest authentication (AC1)
  - [x] Verify LoginViewModel calls `AuthType.DeviceID` via IAuthService
  - [x] EOS Device ID creation via `EnsureEosDeviceIdCreated()`
  - [x] EOS Connect login via `ExternalCredentialType.DeviceidAccessToken`
  
- [x] Implement auto-restore for returning guests (AC2)
  - [x] BootstrapState checks for previous DeviceID login in settings
  - [x] Auto-login with `AuthType.DeviceID` if last method was DeviceID
  - [x] Fallback to Login screen if no previous session
  
- [x] Integrate with existing PlayerDataService for stats (AC3)
  - [x] PlayerDataService already handles match history via PlayerStatsData
  - [x] PlayerProgressData tracks progression
  - [x] Data persists via ISaveService with encryption

---

## Dev Notes

### EOS Device Login Architecture

The project already implements EOS Device Login for guest authentication:

**Core.Auth.AuthenticationService:**
```csharp
// Creates device ID with EOS Connect
private async UniTask<bool> EnsureEosDeviceIdCreated()
{
    var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
    var createOptions = new CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };
    connectInterface.CreateDeviceId(ref createOptions, null, callback);
}

// Logs in with Device ID credential
private async UniTask<bool> LoginWithEosDeviceIdAsync()
{
    EOSManager.Instance.StartConnectLoginWithOptions(
        ExternalCredentialType.DeviceidAccessToken,
        null,
        displayName,  // "Guest_{deviceId}"
        callback
    );
}
```

**Guest Play Flow:**
```
App Launch → BootstrapState → Check LastLoginMethod
    ↓ LastLoginMethod == "DeviceID"
Auto-login with EOS Device ID → SessionLoadingState → Main Menu
    ↓ No previous login
Show LoginView → Guest Play button → EOS Device Login → Main Menu
```

### Implementation Details

**Files Modified:**

1. **LoginViewModel.cs** - Restored to use EOS Device Login
   - `LoginAsGuest()` calls `_authService.LoginAsync(AuthType.DeviceID)`
   - Removes local-only guest session creation
   - Proper error handling for EOS connection failures

2. **BootstrapState.cs** - Restored auto-login flow
   - Checks `saveService.GetSettings().LastLoginMethod`
   - Auto-login if equals "DeviceID"
   - No local guest session check (was incorrect)

3. **GameLifetimeScope.cs** - Cleaned up
   - Removed RecipeRage.Domain.Auth registrations
   - Restored to use existing AuthenticationService only

**Key Points:**
- EOS Device Login = Online guest ( Epic Online Services )
- Guest identity tied to device, persists across app restarts
- PlayerDataService handles stats/progression (already implemented)
- No additional assemblies or local-only auth needed

### Testing

**Manual Testing:**
1. Fresh install → Login screen → Guest Play → Main Menu
2. Close app → Reopen → Auto-login → Main Menu (no login screen)
3. Logout → Login screen → Guest Play creates new EOS session

**Verification:**
- Check EOS ProductUserId is valid after guest login
- Verify PlayerDataService persists data between sessions
- Confirm LastLoginMethod = "DeviceID" in settings

---

## Dev Agent Record

### Agent Model Used

kimi-k2.5-free (Opencode)

### Debug Log References

- Initial implementation incorrectly built local-only guest system
- Corrected to use existing EOS Device Login in AuthenticationService
- Removed all local-only guest account code
- Restored LoginViewModel, BootstrapState to use EOS auth flow

### Completion Notes List

**AC1: Immediate Guest Play Entry**
- Guest Play button uses existing `IAuthService.LoginAsync(AuthType.DeviceID)`
- EOS Device ID created via `CreateDeviceIdOptions` with device model
- Login via `ExternalCredentialType.DeviceidAccessToken`
- Display name format: `"Guest_{deviceUniqueIdentifier}"`

**AC2: Guest Progress Persistence**
- BootstrapState checks `LastLoginMethod` setting
- If "DeviceID", auto-login with EOS Device credentials
- Fallback to LoginState if no previous session
- EOS handles device-to-account mapping

**AC3: Match History Tracking**
- Uses existing `PlayerDataService` for stats/progression
- `PlayerStatsData` records matches, wins, score
- `PlayerProgressData` tracks unlocks, high scores
- All data persisted via encrypted ISaveService

**Cleanup:**
- Removed: Domain/Auth/* (local-only guest code)
- Removed: Gameplay/Auth/* (local-only adapters)
- Removed: Tests/Domain/Auth/* (local-only tests)
- Removed: RecipeRage.Domain*, RecipeRage.Gameplay.Auth* assemblies
- Restored: LoginViewModel.cs, BootstrapState.cs, GameLifetimeScope.cs

### File List

**Modified Files:**
- `Assets/Scripts/Gameplay/UI/Features/Auth/LoginViewModel.cs` - Restored EOS Device Login flow
- `Assets/Scripts/Gameplay/App/State/States/BootstrapState.cs` - Restored auto-login flow
- `Assets/Scripts/Gameplay/Bootstrap/GameLifetimeScope.cs` - Removed local guest registrations
- `Assets/Scripts/Gameplay/RecipeRage.Gameplay.asmdef` - Removed Domain.Auth references

**Removed Files (incorrect implementation):**
- `Assets/Scripts/Domain/` - Entire directory removed
- `Assets/Scripts/Gameplay/Auth/` - Entire directory removed
- `Assets/Scripts/Tests/` - Entire directory removed

**Existing Infrastructure Used:**
- `Assets/Scripts/Core/Auth/AuthenticationService.cs` - EOS Device Login implementation
- `Assets/Scripts/Core/Auth/IAuthService.cs` - Auth interface
- `Assets/Scripts/Gameplay/Persistence/PlayerDataService.cs` - Stats/progression

---

## Change Log

| Date | Change | Author |
|------|--------|--------|
| 2026-02-05 | Initial story creation | BMAD |
| 2026-02-05 | INCORRECT: Implemented local-only guest accounts | Dev Agent |
| 2026-02-05 | CORRECTED: Removed local implementation, using existing EOS Device Login | Dev Agent |

---

**Story Context Generated:** 2026-02-05  
**Status:** review  
**Next Step:** Test EOS Device Login flow in build

---

## Story Completion Status

**Implementation corrected - now uses Epic Online Services Device Login**

Key clarifications:
- ❌ Local-only guest accounts (wrong approach)
- ✅ EOS Device Login (correct approach) - online guest via Epic's backend
- Guest identity persists via EOS Connect device credentials
- Stats/progression use existing PlayerDataService with encryption

**Testing required:**
- Verify EOS Device Login on first launch
- Verify auto-login on app restart
- Verify PlayerDataService persists match stats
