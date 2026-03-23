# RecipeRage Documentation

This folder is split into active source-of-truth docs and archived historical material.

## Read First

Use these files in this order:

1. [PROJECT_MEMORY.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/Architecture/PROJECT_MEMORY.md)
2. [CURRENT_CODEBASE_AUDIT.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md)
3. [gameplay-scene-setup.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/Guides/gameplay-scene-setup.md)
4. [KitchenClash_GDD_v3.md](/Users/arshadbarves/MyProject/Projects/RecipeRage/KitchenClash_GDD_v3.md)
5. [KitchenClash_GDD_v3_aspirational.docx](/Users/arshadbarves/MyProject/Projects/RecipeRage/Documentation/KitchenClash_GDD_v3_aspirational.docx) for phase-development targets only

## Active Documentation

### Architecture

- `Documentation/Architecture/PROJECT_MEMORY.md`
- `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md`

### Guides

- `Documentation/Guides/gameplay-scene-setup.md`
- any active guide that still matches current code and scene wiring

### Product / Design

- `KitchenClash_GDD_v3.md`
- `Documentation/KitchenClash_GDD_v3_aspirational.docx`

When the two GDD tracks disagree:

- `KitchenClash_GDD_v3.md` wins for implementation, testing, and cleanup decisions
- `Documentation/KitchenClash_GDD_v3_aspirational.docx` is the planning reference for future phases

### Alignment Tracking

- `Documentation/Architecture/GDD_ALIGNMENT_MATRIX.md`

## Historical References

These files are still useful for context, but they are not source-of-truth:

- `Documentation/Architecture/FINAL_ARCHITECTURE.md`
- `Documentation/Architecture/PLAYER_CONTROLLER_ARCHITECTURE.md`
- `Documentation/Architecture/STATE_TRANSITION_FLOW.md`

## Archive Policy

- Superseded docs move to `Documentation/Archive/<date-or-topic>/`
- Milestone completion notes, temporary implementation logs, and stale flow diagrams do not stay in active folders
- Documentation does not belong under `Assets/Scripts` unless it is intentionally package-local

## Firebase Folder

`Documentation/Firebase/` now contains sample Remote Config payloads and setup notes only. Local dashboard scaffolding, committed dependencies, and secrets do not belong in source control.
