# Story 1.1: Guest Play Without Account Creation

Status: done

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
  - [x] LoginViewModel calls `AuthType.DeviceID` via IAuthService
  - [x] EOS Device ID creation via `EnsureEosDeviceIdCreated()` with retry logic (3 attempts)
  - [x] EOS Connect login via `ExternalCredentialType.DeviceidAccessToken`
  - [x] Safe device ID extraction with null/empty check
  - [x] Added `LOGIN_METHOD_DEVICE_ID` constant for consistency

- [x] Implement auto-restore for returning guests (AC2)
  - [x] BootstrapState checks for previous DeviceID login in settings
  - [x] Auto-login with `AuthType.DeviceID` if last method was DeviceID
  - [x] Fallback to Login screen if no previous session
  - [x] Error handling distinguishes network errors from "no session"
  - [x] Fixed SettingsViewModel guest detection (distinguishes guest from new user)

- [x] Prepare data structure for account linking (AC3)
  - [x] PlayerStatsData: Added EosProductUserId, LinkedAccountType, AccountCreatedAt, LastLinkedAt, AccountLinkingVersion
  - [x] PlayerStatsData: Added LinkToEosAccount(), IsLinkedToPermanentAccount, CreateMigrationSnapshot()
  - [x] PlayerProgressData: Added EosProductUserId, DataVersion
  - [x] PlayerProgressData: Added LinkToEosAccount(), CreateMigrationSnapshot()
  - [x] Unit tests for account linking functionality

- [x] Testing
  - [x] PlayerStatsDataTests - 13 test cases for account linking
  - [x] PlayerProgressDataTests - 12 test cases for migration
  - [x] AuthenticationServiceTests - constants and device ID extraction
  - [x] Test assembly: RecipeRage.Tests.EditMode

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
- **FIXED:** Safe device ID extraction with null/empty check (prevents NRE)
- **FIXED:** Added retry logic (3 attempts with exponential backoff) for device ID creation
- **FIXED:** Added `LOGIN_METHOD_DEVICE_ID` constant to avoid nameof() fragility

**AC2: Guest Progress Persistence**
- BootstrapState checks `LastLoginMethod` setting
- If "DeviceID", auto-login with EOS Device credentials
- Fallback to LoginState if no previous session
- **FIXED:** SettingsViewModel now distinguishes guest (DeviceID login) from new user (no login)
- **FIXED:** BootstrapState error handling catches and logs auto-login exceptions
- EOS handles device-to-account mapping

**AC3: Match History Tracking + Account Linking Preparation**
- Uses existing `PlayerDataService` for stats/progression
- `PlayerStatsData` records matches, wins, score
- `PlayerProgressData` tracks unlocks, high scores
- All data persisted via encrypted ISaveService
- **NEW:** Account linking fields in PlayerStatsData:
  - EosProductUserId, LinkedAccountType, AccountCreatedAt, LastLinkedAt, AccountLinkingVersion
  - LinkToEosAccount(), IsLinkedToPermanentAccount, CreateMigrationSnapshot()
- **NEW:** Account linking fields in PlayerProgressData:
  - EosProductUserId, DataVersion
  - LinkToEosAccount(), CreateMigrationSnapshot()
- **EOS Account Linking Flow (per docs):**
  1. Login with Device ID
  2. Get ContinuanceToken from real identity login
  3. Call EOS_Connect_LinkAccount to associate accounts
  4. Data persists (same ProductUserId)

**Testing:**
- PlayerStatsDataTests: 13 test cases covering account linking
- PlayerProgressDataTests: 12 test cases covering migration
- AuthenticationServiceTests: constants and device ID extraction
- Test assembly: RecipeRage.Tests.EditMode

**Cleanup:**
- Removed: Domain/Auth/* (local-only guest code)
- Removed: Gameplay/Auth/* (local-only adapters)
- Removed: Tests/Domain/Auth/* (local-only tests)
- Removed: RecipeRage.Domain*, RecipeRage.Gameplay.Auth* assemblies
- Restored: LoginViewModel.cs, BootstrapState.cs, GameLifetimeScope.cs
- Fixed comment typo: "in hierarchy" → "in the hierarchy"

### File List

**Modified Files:**
- `Assets/Scripts/Core/Auth/AuthenticationService.cs` - Fixed NRE risk, added retry logic, added LOGIN_METHOD_DEVICE_ID constant
- `Assets/Scripts/Gameplay/UI/Features/Auth/LoginViewModel.cs` - Restored EOS Device Login flow
- `Assets/Scripts/Gameplay/UI/Features/Settings/SettingsViewModel.cs` - Fixed guest detection logic
- `Assets/Scripts/Gameplay/App/State/States/BootstrapState.cs` - Restored auto-login flow, added error handling
- `Assets/Scripts/Gameplay/Bootstrap/GameLifetimeScope.cs` - Removed local guest registrations, fixed comment typo
- `Assets/Scripts/Gameplay/RecipeRage.Gameplay.asmdef` - Removed Domain.Auth references
- `Assets/Scripts/Gameplay/Persistence/Data/PlayerStatsData.cs` - Added account linking fields and methods
- `Assets/Scripts/Gameplay/Persistence/Data/PlayerProgressData.cs` - Added account linking fields and methods

**New Test Files:**
- `Assets/Scripts/Tests/RecipeRage.Tests.EditMode.asmdef` - Test assembly definition
- `Assets/Scripts/Tests/EditMode/Core/Auth/AuthenticationServiceTests.cs` - Constants and device ID extraction tests
- `Assets/Scripts/Tests/EditMode/Gameplay/Persistence/PlayerStatsDataTests.cs` - Account linking tests (13 cases)
- `Assets/Scripts/Tests/EditMode/Gameplay/Persistence/PlayerProgressDataTests.cs` - Migration tests (12 cases)

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
| 2026-02-05 | FIXED: Code review issues - NRE risk, retry logic, constants, account linking | Dev Agent |

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
