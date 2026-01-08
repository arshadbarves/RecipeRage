# Technology Stack

## Core Engine
- **Engine:** Unity 6.0 (6000.3.0f1)
- **Language:** C#

## Multiplayer & Networking
- **Networking Framework:** Netcode for GameObjects (NGO)
- **Services SDK:** Epic Online Services (EOS) - Identity, Friends, P2P

## Architecture & Design Patterns
- **Architecture:** Strict Two-Bucket Assembly Architecture (Modules & Gameplay)
- **Dependency Injection:** VContainer
- **Async Handling:** UniTask
- **Modules:** Custom Assembly (RecipeRage.Modules) for high-level, project-agnostic logic.
- **Gameplay:** Separated Gameplay Assembly (RecipeRage.Gameplay) for game-specific mechanics.

## User Interface
- **UI System:** UI Toolkit
- **Styling:** USS (Unity Style Sheets) with UXML templates

## Key Libraries & Tools
- **Logging:** Custom Logging Module
- **Version Control:** Git LFS
- **Authentication:** Custom EOS Auth Module
- **Banking:** Generic Banking System with EOS Player Data Storage backend.
