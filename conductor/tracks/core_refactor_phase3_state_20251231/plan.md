# Plan: Core Refactor - Phase 3 (State Encapsulation)

## Phase 1: Stat System Foundation
Create a robust way to handle gameplay values.

- [x] Task: Create ModifiableStat
    -   Create `ModifiableStat` class (Base Value, List of Modifiers, Current Value).
    -   Implement `AddModifier` and `RemoveModifier` methods.
    -   Ensure it's lightweight and ideally a struct or simple class.

## Phase 2: PlayerController Refactoring
Apply the new system to the player.

- [x] Task: Encapsulate Player Stats
    -   Replace `float InteractionSpeedModifier { get; set; }` with `ModifiableStat InteractionSpeed`.
    -   Replace `int CarryingCapacity { get; set; }` with `ModifiableStat CarryingCapacity`.
    -   Update all references (MovementController, InteractionController) to read from `.Value`.
    -   Update references that *set* these values (Powerups, etc.) to use `AddModifier`.

## Phase 3: Cleanup
- [x] Task: Review Public API
    -   Scan `PlayerController` for any other public setters.
    -   Restrict access to `internal` or `private` where possible.
