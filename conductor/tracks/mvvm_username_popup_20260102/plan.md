# Plan: MVVM Refactor for Username Popup

## Phase 1: ViewModel Implementation
- [x] Task: Create `UsernameViewModel.cs` in `Assets/Scripts/UI/ViewModels/`.
- [x] Task: Implement reactive properties (`BindableProperty<T>`) and validation logic.
- [x] Task: Implement `SaveUsername` command with dependency on `ISaveService` and `ICurrencyService`.
- [x] Task: Register `UsernameViewModel` in `GameLifetimeScope.cs`.

## Phase 2: Template & UI Refinement
- [x] Task: Update `UsernamePopupTemplate.uxml` with cleaner element naming and a loading overlay.
- [x] Task: Update `UsernamePopup.uss` with loading state styles and polish.

## Phase 3: View Refactoring
- [x] Task: Refactor `UsernamePopup.cs` to remove logic and inject `UsernameViewModel`.
- [x] Task: Implement data binding between `UsernamePopup` and `UsernameViewModel`.
- [x] Task: Ensure existing appear animations (DOTween) are preserved.

## Phase 4: Final Verification
- [~] Task: Conductor - User Manual Verification 'Phase 4: Final Verification' (Protocol in workflow.md)
