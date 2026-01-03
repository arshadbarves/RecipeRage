# Specification: MVVM Refactor for Username Popup

## Overview
Refactor the existing `UsernamePopup` to adhere to the project's MVVM pattern. This involves moving business logic, validation, and state management into a new `UsernameViewModel` and using reactive data binding (`BindableProperty<T>`) to update the UI.

## Functional Requirements
- **ViewModel Implementation:**
    - Create `UsernameViewModel` inheriting from `BaseViewModel`.
    - Reactive Properties:
        - `Username` (string): Bound to the input field.
        - `StatusMessage` (string): Current validation or system message.
        - `IsStatusError` (bool): Determines if the status message is styled as an error.
        - `IsLoading` (bool): Shows/hides a loading state during save.
        - `CostText` (string): Displays the gem cost.
        - `IsCostVisible` (bool): Hides cost container if cost is 0.
        - `IsCancelVisible` (bool): Hides cancel button for first-time setup.
        - `IsConfirmEnabled` (bool): Disables confirm button if invalid or loading.
    - Logic:
        - Move `ValidateUsername` logic to ViewModel.
        - Handle Gem cost calculation (0 for first time, 50 otherwise).
        - Handle saving via `ISaveService` and spending via `ICurrencyService`.

- **View (UsernamePopup) Implementation:**
    - Remove business logic and service dependencies (move to ViewModel).
    - Bind UI elements to ViewModel properties using `RegisterValueChangedCallback` (for input) and property subscriptions (for output).
    - Maintain existing DOTween appear animations.

## Visual & UI Requirements
- **Template Update:** Refactor `UsernamePopupTemplate.uxml` for cleaner naming and add a loading overlay/spinner container.
- **Styling:** Update `UsernamePopup.uss` to support the loading state and ensure consistent "Sport/Skewed" aesthetic.

## Acceptance Criteria
- [ ] Username validation (3-16 chars, alphanumeric/underscores) works via ViewModel.
- [ ] First-time setup correctly shows free cost and hides Cancel button.
- [ ] Sub-sequent name changes cost 50 gems and show the cost correctly.
- [ ] "Saving..." state correctly disables interactions and shows loading indicator.
- [ ] Successful save triggers `onConfirm` callback and shows success notification.
- [ ] All UI updates are driven by `BindableProperty` changes.

## Out of Scope
- Changing the backend API for name availability (simulated for now).
- Adding complex character customization within this popup.
