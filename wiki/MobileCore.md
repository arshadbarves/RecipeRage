# Playcenter MobileCore

Reusable mobile multiplayer core: dual-stick input, session lifecycle, bot task-planner
framework, net reconnect glue. First consumer: RecipeRage (cutover complete 2026-07).

**Location:** `Assets/Playcenter/MobileCore/Runtime/`
**Spec:** `docs/superpowers/specs/2026-07-24-playcenter-mobile-core-design.md`
**Plan:** `docs/superpowers/plans/2026-07-24-playcenter-mobile-core.md`

## Layout

- `Core/` — engine-free (CI grep gate: no UnityEngine/VContainer/Netcode/Epic/Firebase/Cysharp)
- `Adapters/` — the only vendor zone (InputSystem touch provider, UnityGameClock)
- `Bootstrap/` — `PlaycenterBootstrap` MonoBehaviour, sole scene entry point

## Subsystems

| Area | Core types | Game adapters |
|------|-----------|---------------|
| Input | `DualStickModel`, `TapGestureDetector`, `InputFrame` (v1) | `TouchDualStickProvider`, `MobileCoreInputBridge` |
| Session | `SessionLifecycleController` (fail-closed FSM) | `VContainerSessionScopeFactory` (game Composition) |
| Bots | `TaskPlanner<TS,T>`, `ClaimRegistry<TK>`, `BotHost` (budgeted) | Kitchen evaluators, `BotController` |
| Net | `ReconnectStateMachine`, `BackoffPolicy`, `ConnectionQualityTracker`, `NetSessionOrchestrator` | `NetSessionConnectivityBridge` |

## Testing policy (module amendment 2026-07-24)

Money-path tests only: core state machines and planners. Thin adapters, DTOs, and
bootstrap glue are verified by inspection. This amends the blanket ">80% on all new
code" rule for this module (approved by project owner).

## RC keys

`mc_bot_budget_ms` (2) · `mc_reconnect_menu_interval_ms` (3000) · `mc_reconnect_match_attempts` (3) ·
`mc_reconnect_match_interval_ms` (5000) · `mc_reconnect_backoff_base_ms` (1000) ·
`mc_reconnect_degraded_ms` (150) · `mc_reconnect_poor_ms` (400) ·
`mc_input_deadzone` (0.15) · `mc_input_tap_window_ms` (300) · `mc_input_tap_idle_reset_ms` (500)

## RecipeRage cutover notes (2026-07-25)

- Sole `IInputProvider` = `MobileCoreInputProvider` (VContainer-injected into `PlayerController`).
  Legacy chain deleted: `IDualStickInput`, `GameplayInputMapper`, `TouchInputProvider`,
  `InputSystemProvider`, `InputProviderFactory`, `GameplayInputService`, `InputReceiver`,
  `GameplayInputBridge`, both generated `PlayerInputActions` copies.
- `PlayerInputData` gained `Aim` (wire change, pre-soft-launch safe).
- `SessionManager` delegates to `SessionLifecycleController`; VContainer session
  factory/handle live in game Composition (module stays DI-neutral).
- Bots: 8 kitchen evaluators implement `ITaskEvaluator`; priority chain preserved
  (fire → deliver → cooking → prep → recover → claim → fetch → wander).
  `BotClaimRegistry` renamed `BotOrderClaims` (counter-claiming is game-specific).
  `BotManager` kept (roster factory, not planning).
- Net: `NetSessionOrchestrator` is the sole start/stop path; registered session-scoped
  alongside `ReconnectStateMachine` + `ConnectionQualityTracker` in `MenuSessionRegistrations`.
