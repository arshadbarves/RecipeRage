# Plan: Core Refactor Assessment

## Phase 1: Analysis & Discovery
Perform deep code analysis to map the current state and identifying issues.

- [x] Task: Analyze Core Architecture
    -   Scan `Assets/Scripts/Core` for legacy patterns (Singleton access, static mutable state).
    -   Identify "God Classes" or services with excessive responsibilities.
- [x] Task: Analyze UI/Logic Coupling
    -   Scan `Assets/Scripts/UI` for direct gameplay logic dependencies.
    -   Identify View classes that handle business logic instead of just display.
- [x] Task: Analyze Gameplay Coupling
    -   Scan `Assets/Scripts/Gameplay` for direct Core/UI dependencies that break modularity.

## Phase 2: Strategy Definition
Synthesize findings into a concrete action plan.

- [x] Task: Generate Technical Debt Report
    -   Compile findings into a structured report in `conductor/tracks/core_refactor_assessment_20251231/report.md`.
    -   Categorize by Priority (Critical, High, Medium) and Type (Architecture, Code Quality, Performance).
- [x] Task: Create Refactoring Roadmap
    -   Define specific actionable tracks for the actual refactoring work (e.g., "Track: Decouple UI", "Track: Split GameBootstrap").
    -   Update `conductor/tracks.md` with these future tracks (if approved).
