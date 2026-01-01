# Plan: AnimationService Refactor & Modernization

## Phase 1: Interface Modernization
Update the core interfaces to use `UniTask` and remove callbacks.

- [x] Task: Update IUIAnimator
    -   Change return types to `UniTask`.
    -   Remove `Action onComplete` parameters.
    -   Add `CancellationToken` support where applicable.
- [x] Task: Update ITransformAnimator
    -   Change return types to `UniTask`.
    -   Remove `Action onComplete` parameters.
- [ ] Task: Conductor - User Manual Verification 'Interface Modernization' (Protocol in workflow.md)

## Phase 2: Implementation Refactor (UI)
Move logic to the dedicated UI animator.

- [x] Task: Update DOTweenUIAnimator
    -   Implement `UniTask` return types.
    -   Move raw property animation logic (Opacity, Position, etc.) from `AnimationService` to here.
    -   Use `Core.Extensions.TweenExtensions` for `ToUniTask()` conversion.
- [x] Task: Refactor AnimationService (UI Delegation)
    -   Update `AnimationService.Animate*` methods to delegate to `_uiAnimator`.
    -   Remove direct `DOTween` calls for UI elements from `AnimationService`.

## Phase 3: Implementation Refactor (Transform)
Modernize the Transform animator.

- [x] Task: Update DOTweenTransformAnimator
    -   Implement `UniTask` return types.
    -   Update internal DOTween calls to use `ToUniTask()`.
- [x] Task: Refactor AnimationService (Transform Delegation)
    -   Update wrapper methods in `AnimationService` to return the `UniTask` from `_transformAnimator`.

## Phase 4: Cleanup & Verification
Ensure the system is consistent.

- [x] Task: Verify API Consistency
    -   Ensure method names align (e.g., `FadeIn` vs `AnimateOpacity`).
    -   Update `AnimationService` signatures to match the new async pattern.
- [~] Task: Conductor - User Manual Verification 'Cleanup & Verification' (Protocol in workflow.md)
