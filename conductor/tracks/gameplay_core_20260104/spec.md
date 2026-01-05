# Specification: Gameplay Mechanics & Architecture

## Overview
Implementation of the core gameplay loop, character ability system, and advanced multiplayer systems (P2P fairness, RL bots, and session recovery) for RecipeRage. This track focuses on team-based competitive play where players and bots work together to fulfill orders.

## Functional Requirements

### 1. Game Mode: Team-Based Competition
- **Structure:** 3v3 matches (or multiple teams of 3).
- **Objectives:**
    - **Timed Match:** Fixed duration (e.g., 3-5 minutes). Highest team score wins.
    - **Dynamic Orders:** Randomly generated recipes with individual timers. Fulfilling orders adds points; expiration deducts points.
    - **Sudden Death:** Triggered on ties at the end of the match.

### 2. Bot Integration & Matchmaking
- **Priority:** Matchmaking logic prioritizes matching human players.
- **Dynamic Fill:** If human players are not found within the matchmaking timeout, empty slots in a 3-player team are filled with bots to initiate the match.
- **Bot Behavior:** Adaptive difficulty based on player trophies. Role-based decision making (Chopper, Chef, Runner).
- **Replacement:** If a disconnected player rejoins, they immediately replace the bot that took their place.

### 3. Core Gameplay Loop
- **Interaction System:** Single-button context-sensitive interactions (Pick Up, Drop, Chop, Cook).
- **Stations:** Ingredient Crates, Cutting Boards, Stoves, Empty Crates (for staging), and Delivery Belts.
- **Item States:** Raw -> Chopped -> Cooked -> Plated -> Burnt. (No Sinks/Dishwashing).

### 4. Character & Ability System
- **Architecture:** Extensible system for character-specific data.
- **Active Abilities:** Cooldown-based skills (e.g., Dash, Throw, Instant-Action).
- **Passive Perks:** Stat modifiers (Speed, Action Speed) based on selected character.

### 5. Networking & Fairness (P2P)
- **Authority:** Server-Authoritative model (Host as authority).
- **Responsiveness:** Client-Side Prediction for movement and interaction feedback to minimize perceived lag.
- **Fairness:** Server reconciliation to prevent cheating and resolve interaction conflicts.

### 6. Session Management
- **Host Migration:** EOS-based transition of authority if the host disconnects.
- **Player Rejoin:** Disconnected players can return to the active session. No Late Join (lobby locked once the match starts).

## Non-Functional Requirements
- **Low Latency Feel:** Prediction must make interactions feel "local" for clients.
- **Modular Design:** Easy addition of new stations or ingredients via Scriptable Objects.

## Acceptance Criteria
- [ ] Two teams of 3 (including bots) can compete in a timed match.
- [ ] Matchmaking successfully fills bots only when human players are unavailable.
- [ ] Characters have unique speeds and one active ability that triggers on a button press.
- [ ] A client can disconnect and rejoin the same match, regaining control from their placeholder bot.
- [ ] Interactions remain consistent even with 100ms+ latency via prediction/reconciliation.

## Out of Scope
- Meta-game UI (Trophies/Leaderboards).
- Dishwashing/Sink mechanics.
- Individual (solo) game modes.
