# Specification: Core Refactor - Phase 3 (State Encapsulation)

## Goal
To eliminate mutable public state in core gameplay classes, specifically `PlayerController`, enforcing a unidirectional data flow or strictly controlled modification pattern. This prevents "spooky action at a distance" bugs where any system can modify player stats without traceability.

## Scope
1.  **PlayerController Encapsulation:**
    -   Audit `PlayerController` for public fields/properties with public setters.
    -   Convert `InteractionSpeedModifier` and `CarryingCapacity` to use a `StatSystem` or private setters with explicit modification methods (`AddModifier`, `RemoveModifier`).
2.  **Stat System (Optional but Recommended):**
    -   If multiple stats need modifiers (buffs/debuffs), implement a lightweight `Stat` class that handles base value + modifiers.
3.  **Network State Safety:**
    -   Ensure that state changes are only driven by the Server or Authority, and replicated correctly via `NetworkVariable` or RPCs, rather than direct field modification.

## Success Criteria
-   `PlayerController` has NO public mutable fields.
-   All stat modifications go through methods like `ModifyStat(StatType, value, duration)`.
-   Game compiles and plays without regression in player movement/interaction.
