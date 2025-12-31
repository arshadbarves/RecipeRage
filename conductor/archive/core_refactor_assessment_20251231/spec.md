# Specification: Core Refactor Assessment

## Goal
To perform a comprehensive architectural analysis of the current codebase following the VContainer migration. The objective is to identify technical debt, modularity violations, and legacy patterns to define a clear scope and strategy for a full Core Refactor.

## Scope
1.  **Codebase Analysis:**
    -   Identify `MonoBehaviour` logic that should be pure C# classes.
    -   Locate remaining "Manager" classes that violate Single Responsibility Principle.
    -   Assess the current state of UI/Logic coupling.
    -   Identify hardcoded dependencies or "God classes" (e.g., `GameBootstrap` leftovers).
    -   Review `TODO` and `HACK` comments for hidden debt.

2.  **Architecture Mapping:**
    -   Map current cross-module dependencies (Core <-> Gameplay <-> UI).
    -   Identify circular dependencies or "leaky" abstractions.

3.  **Refactoring Strategy:**
    -   Propose a phased plan to decouple systems.
    -   Define strict boundaries for modules (e.g., "Gameplay code cannot directly reference UI components").

## Deliverables
-   A detailed **Refactoring Plan** (to be used as the next Track).
-   A **Technical Debt Report** categorizing issues by severity and effort.
-   Updated `tech-stack.md` guidelines if new architectural rules are established.
