# Smoke: Guest login (EOS Dev)

## Preconditions
- EOS configs all Dev (Task 2)
- `EOS_AVAILABLE` define on target platform
- Network online; Epic product active

## Steps
1. Enter Play Mode on `Assets/Scenes/Bootstrap.unity` (or player build).
2. Reach Login; tap **Continue as Guest** / guest button.
3. Expect: no error modal; transition toward Home within 15s.
4. Log markers (filter Console):
   - EOS platform init success
   - DeviceId create or already exists
   - Connect login success
   - `IAuthService` reports authenticated
5. Kill app; relaunch; guest session restores or re-auths without crash.

## Fail criteria
- AuthResult.Failed with "not yet implemented" on guest path
- Hang >30s with no log
- Exception in AuthenticationService

## Code audit (2026-07-24)
`LoginAsGuestAsync()` calls only `InitializeUgsAsync()` (bridge-gated) → `LoginWithEosDeviceIdAsync()` → `LoginToUgsWithEosAsync()` (bridge-gated). Social stubs (`LoginWithGoogleAsync`, `LoginWithFacebookAsync`, `LoginWithAppleAsync`) are never touched on the guest path. Guest path cannot return "not yet implemented".

## Last run
- Date: _pending — requires Unity Editor run_
- Platform: _
- Result: _PENDING_
