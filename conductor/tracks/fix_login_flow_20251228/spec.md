# Specification: Fix Initial Login Flow

## Goal
Ensure the `LoginView` appears on the first launch of the game (or after logout), and that Auto-Login only occurs if the user has previously logged in successfully.

## Requirements

### 1. Persistence Logic
- The game must persist a `LastLoginType` (string or enum) locally using `PlayerPrefs` or the existing `SaveService`.
- **On Login Success:** Save the `LastLoginType` (e.g., "DeviceID").
- **On Logout:** Clear the `LastLoginType`.

### 2. Bootstrap Logic (`BootstrapState`)
- Modify `InitializeGameSequence` to check for `LastLoginType`.
- **If `LastLoginType` exists:** Attempt `_authService.LoginAsync(type)`.
    - If success -> Main Menu.
    - If failure -> Login Screen.
- **If `LastLoginType` is missing/empty:** Skip auto-login -> Login Screen.

### 3. Auth Service Update
- Update `EOSAuthService` (or `LoginView`) to ensure the successful login event triggers the persistence saving.
- Ideally, `EOSAuthService` should not depend on `SaveService` directly to keep it clean, but `BootstrapState` or a mediator (`AuthenticationManager` logic) should handle it.
- **Decision:** Since `EOSAuthService` is the core logic now, we can inject `ISaveService` into it, or handle it in `LoginView` (UI) and `BootstrapState` (Logic).
- **Better Approach:** Handle saving in `EOSAuthService` so it works for both UI-driven and Auto-driven logins.

## Implementation Details
- **File:** `Assets/Scripts/Modules/Auth/Core/EOSAuthService.cs`
    - Inject `ISaveService` (if available in constructor) OR use `PlayerPrefs` directly for this simple flag if `ISaveService` is too heavy (but we have `ServiceContainer` so injection is easy).
    - In `LoginAsync`, if success, save flag.
    - In `LogoutAsync`, clear flag.
- **File:** `Assets/Scripts/Core/State/States/BootstrapState.cs`
    - Remove blind `LoginAsync` call.
    - Add conditional check.
