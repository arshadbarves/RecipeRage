# Specification: AnimationService Refactor & Modernization

## Goal
To refactor the `AnimationService` to strictly adhere to the Facade pattern by delegating all UI animation logic to `IUIAnimator`, and to modernize the entire animation interface to use `UniTask` instead of `Action` callbacks.

## Scope
1.  **Interface Updates:**
    -   Update `IUIAnimator` and `ITransformAnimator` methods to return `UniTask` instead of `void`.
    -   Remove `Action onComplete` parameters from all animation methods (callers should `await` instead).
    -   Standardize method naming (e.g., ensure consistency between Service and Animators).

2.  **Implementation Refactor:**
    -   **`AnimationService.cs`:** Remove raw DOTween logic for UI elements. Update methods to delegate directly to `_uiAnimator` (e.g., `AnimateOpacity` -> `_uiAnimator.FadeIn/Out` or specific setter).
    -   **`DOTweenUIAnimator.cs`:** Update to return `UniTask`. Implement logic previously residing in `AnimationService` if it was unique (e.g., raw property setters).
    -   **`DOTweenTransformAnimator.cs`:** Update to return `UniTask`.

3.  **Modernization:**
    -   Ensure all animations support `CancellationToken` for safe cancellation.

## Functional Requirements
*   `AnimationService` must not import `DG.Tweening` namespace directly (unless for types like `Ease`). All logic must be in the sub-animators.
*   All animation methods must be awaitable.
*   Existing functionality (Fade, Slide, Scale, Rotate) must remain available but routed correctly.

## Out of Scope
*   Refactoring every single usage of `AnimationService` in the entire codebase (this will be done incrementally or in a separate "Fix Breaking Changes" chore, though we will ensure the Service *compiles*).

## Acceptance Criteria
*   `AnimationService` code is clean and contains only delegation logic.
*   `IUIAnimator` and `ITransformAnimator` methods return `UniTask`.
*   No `Action onComplete` callbacks exist in the animation interfaces.
