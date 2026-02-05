---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories']
inputDocuments:
  - /Users/arshadbarves/MyProject/RecipeRage/_bmad-output/planning-artifacts/prd.md
  - /Users/arshadbarves/MyProject/RecipeRage/_bmad-output/planning-artifacts/architecture.md
  - /Users/arshadbarves/MyProject/RecipeRage/_bmad-output/planning-artifacts/ux-design-specification.md
status: stories-created
epicsCount: 7
storiesCount: 50
---

# RecipeRage - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for RecipeRage, decomposing the requirements from the PRD, UX Design, and Architecture documents into implementable stories.

## Requirements Inventory

### Functional Requirements

**Matchmaking & Session Management (8 FRs):**
- FR1: Players can enter Quick Play matchmaking with automatic bot filling
- FR2: Players can create private lobbies with shareable invite links
- FR3: Players can join matches via invite links or friend invites
- FR4: The system can start matches based on game mode configuration, filling with AI bots when no players available
- FR5: The system can replace disconnected human players with AI bots mid-match
- FR6: The system can migrate host authority to another client if current host disconnects
- FR7: Players can view match history including scores, team compositions, and duration
- FR8: Players can form teams in the lobby if the game mode supports team-based play

**Character & Ability System (7 FRs):**
- FR9: Players can select from unlocked characters before match start
- FR10: Characters can have unique passive abilities that affect gameplay
- FR11: Players can activate character abilities during matches with cooldown constraints
- FR12: The system can validate ability usage server-authoritatively to prevent cheating
- FR13: The system can track ability usage statistics per player
- FR14: Players can unlock new characters through gameplay progression
- FR15: The system can enforce ability cooldowns and duration limits

**Bot AI System (8 FRs):**
- FR16: AI bots can navigate kitchen environments to reach stations
- FR17: AI bots can identify and pick up ingredients from storage crates
- FR18: AI bots can process ingredients at appropriate stations (cutting, cooking, etc.)
- FR19: AI bots can assemble completed dishes on plates
- FR20: AI bots can deliver finished orders to serving stations
- FR21: AI bots can prioritize orders based on expiration time and complexity
- FR22: AI bots can interact with other players' characters (collision, ability effects)
- FR23: The system can scale AI difficulty based on player skill level

**Cooking & Station Mechanics (6 FRs):**
- FR24: Players can move characters via touch controls to navigate kitchen
- FR25: Players can interact with cooking stations by tapping them
- FR26: Players can pick up and carry ingredients and prepared items
- FR27: Players can place ingredients on empty stations or plates for other players to use
- FR28: The system can track cooking progress and alert players when items are ready
- FR29: The system can destroy burnt or expired items

**Order & Scoring System (8 FRs):**
- FR30: The system can generate random orders with varying complexity based on active game mode
- FR31: The system can assign time limits to orders based on complexity and game mode
- FR32: Players can serve completed orders to score points
- FR33: The system can calculate scores based on order complexity, speed, and accuracy
- FR34: The system can apply combo multipliers for consecutive quick serves
- FR35: The system can determine match winners based on final scores or time expiration
- FR36: Players can view real-time scoreboards during matches
- FR37: The system can track personal bests and session statistics

**Social & Friends (5 FRs):**
- FR38: Players can send friend invites via contact list synchronization
- FR39: Players can view friends' online status and current activity
- FR40: Players can invite friends to private matches
- FR41: Players can block or report other players
- FR42: The system can suggest friends based on match history and contacts

**Progression & Unlocks (5 FRs):**
- FR43: Players can earn experience points through match participation and performance
- FR44: Players can level up accounts to unlock new characters and features
- FR45: Players can earn soft currency (coins) through gameplay
- FR46: Players can spend coins to unlock characters, skins, and cosmetics
- FR47: The system can track player statistics (matches played, win rate, favorite character)

**Account & Authentication (4 FRs):**
- FR48: Players can play as guests without account creation
- FR49: Players can link guest accounts to permanent accounts via email
- FR50: Players can authenticate via Facebook login
- FR51: The system can authenticate players via Epic Online Services

**Communication (3 FRs):**
- FR52: Players can send pre-set quick chat messages in the lobby (pre-match and post-match)
- FR53: Players can view post-match lobby with recent teammates and opponents
- FR54: The system can display player status indicators (online, in-match, away)

### Non-Functional Requirements

**Network Performance (Critical - Core Differentiator):**
- NFR1: P2P connection latency between clients in same region must be <100ms (95th percentile)
- NFR2: Matchmaking from "tap to play" to match start must complete within 5 seconds
- NFR3: Game state synchronization must maintain 30Hz tick rate between host and clients
- NFR4: Client-side prediction must mask network latency up to 150ms without perceptible lag
- NFR5: Game frame rate must maintain 30+ FPS on target devices (iPhone 8 equivalent, mid-tier Android)
- NFR6: Memory usage must not exceed 200MB RAM during active gameplay
- NFR7: App cold start must complete within 3 seconds on target devices

**Game Responsiveness:**
- NFR8: Touch input must register within 50ms and display visual feedback within 100ms
- NFR9: Ability activation must propagate to all clients within 200ms (host authoritative)
- NFR10: Bot AI decision cycle must complete within 100ms to avoid gameplay stutter

**Security:**
- NFR11: Guest account data must be encryptable for future account linking
- NFR12: Facebook OAuth integration must follow OAuth 2.0 security standards
- NFR13: Player display names must be filtered for profanity and offensive content
- NFR14: Ability usage must be server-authoritatively validated (host verification)
- NFR15: Score submissions must include match replay hash for verification
- NFR16: Client memory must not expose sensitive game state to modification

**Data Privacy:**
- NFR17: Contact list access must be optional and clearly explained to users
- NFR18: Analytics data must be anonymized and exclude personally identifiable information
- NFR19: COPPA compliance required - no data collection from users under 13

**Reliability:**
- NFR20: Match completion rate must exceed 95% (including bot replacement scenarios)
- NFR21: Host migration must complete within 5 seconds when current host disconnects
- NFR22: Client reconnection window after brief disconnection must be 30 seconds
- NFR23: Crash rate must not exceed 0.1% per user session

**Data Persistence:**
- NFR24: Player progression must be synced to cloud within 5 seconds of match completion
- NFR25: Critical match results must be stored locally as backup if cloud sync fails, with retry mechanism

**Accessibility:**
- NFR26: Color-coded game elements must have secondary visual indicators for colorblind users
- NFR27: Touch targets must be minimum 44x44 points to accommodate motor impairments
- NFR28: Visual feedback for actions must include haptic confirmation for hearing-impaired users
- NFR29: Tutorial must be skippable for experienced users, replayable for new users
- NFR30: Game mode complexity must be indicated clearly (Simple/Medium/Complex order types)

**Compatibility:**
- NFR31: iOS support must include iOS 13.0 through latest stable version
- NFR32: Android support must include API 26 (Android 8.0) through latest stable version
- NFR33: App must gracefully degrade on lower-spec devices (reduced effects, simpler shaders)
- NFR34: Quick chat messages must support localization in English, Spanish, Portuguese, Japanese, and Korean at launch
- NFR35: UI layout must accommodate RTL languages for future expansion
- NFR36: Regional servers must be available in Asia-Pacific, North America, and Europe

### Additional Requirements

**From Architecture Document:**

**Technical Architecture Patterns:**
- Bot AI runs exclusively on host client using NGO networking patterns (Host-Only AI)
- Smart host selection algorithm based on ping, connection quality, and device capability
- Host migration protocol: Pause match (3 sec) → select new host → transfer authority → resume
- Server-authoritative ability validation using [ServerRpc] pattern
- Layered state sync: Critical State (Reliable), Gameplay State (30Hz), Cosmetic State (Unreliable)

**Implementation Patterns:**
- C# naming conventions: PascalCase classes/methods, camelCase variables, UPPER_SNAKE_CASE constants
- Network RPCs: Verb + Action + Rpc suffix (e.g., ActivateAbilityServerRpc)
- NetworkVariables: network + PascalCase (e.g., networkMatchTimer)
- Bot AI: Host-only execution with `if (!IsHost) return;` check
- All abilities implement IAbility interface with Type, Cooldown, Duration properties

**Code Organization:**
- Domain/ folder for pure C# logic (testable without Unity)
- Gameplay/ folder for Unity-specific code (MonoBehaviour adapters)
- Core/ folder for infrastructure (don't touch without discussion)

**Testing Requirements:**
- Unity Test Framework for unit tests
- Network scenario tests for host migration
- Automated ability balance validation

**From UX Design Document:**

**Mobile UX Requirements:**
- Dual virtual stick controls (Left: Movement, Right: Action/Ability)
- Context-sensitive "Smart Button" that changes based on context (Chop/Throw/Serve)
- Smart throw assist with magnetic aim snap-to-target
- Target preview with ghost icons confirming destination before release
- Trajectory line color coding (White = Wall/Floor, Green = Valid Target)

**Visual Design:**
- "Neon Kitchen" aesthetic: Dark mode, gold/red accents, glassmorphism
- Unity UI Toolkit for menus/UXML, uGUI for in-game HUD
- 8px baseline grid, 48x48px minimum touch targets
- Color system: Tomato Red (#FF5252), Lettuce Green (#66BB6A), Mustard Yellow (#FFD54F)

**Accessibility:**
- Double coding: Status changes use Color + Shape + Animation
- High contrast mode with thick white outlines on interactable elements
- Screen shake toggle for motion sensitivity
- Haptic feedback with audio fallbacks

### FR Coverage Map

| FR | Epic | Description |
|-----|------|-------------|
| FR1 | Epic 2 | Quick Play matchmaking |
| FR2 | Epic 2 | Private lobbies |
| FR3 | Epic 2 | Join via invite links |
| FR4 | Epic 2 | Bot filling algorithm |
| FR5 | Epic 2 | Bot replacement mid-match |
| FR6 | Epic 2 | Host migration |
| FR7 | Epic 2 | Match history |
| FR8 | Epic 2 | Team formation |
| FR9 | Epic 5 | Character selection |
| FR10 | Epic 5 | Passive abilities |
| FR11 | Epic 5 | Ability activation |
| FR12 | Epic 5 | Server-authoritative validation |
| FR13 | Epic 5 | Usage statistics |
| FR14 | Epic 5 | Character unlocks |
| FR15 | Epic 5 | Cooldown enforcement |
| FR16 | Epic 3 | Bot navigation |
| FR17 | Epic 3 | Bot ingredient pickup |
| FR18 | Epic 3 | Bot station processing |
| FR19 | Epic 3 | Bot assembling dishes |
| FR20 | Epic 3 | Bot serving orders |
| FR21 | Epic 3 | Bot order prioritization |
| FR22 | Epic 3 | Bot player interaction |
| FR23 | Epic 3 | Bot difficulty scaling |
| FR24 | Epic 4 | Touch movement |
| FR25 | Epic 4 | Station interaction |
| FR26 | Epic 4 | Pick up/carry items |
| FR27 | Epic 4 | Place ingredients |
| FR28 | Epic 4 | Cooking progress tracking |
| FR29 | Epic 4 | Item destruction |
| FR30 | Epic 4 | Order generation |
| FR31 | Epic 4 | Order time limits |
| FR32 | Epic 4 | Serve completed orders |
| FR33 | Epic 4 | Score calculation |
| FR34 | Epic 4 | Combo multipliers |
| FR35 | Epic 4 | Win conditions |
| FR36 | Epic 4 | Real-time scoreboards |
| FR37 | Epic 4 | Personal bests |
| FR38 | Epic 6 | Friend invites |
| FR39 | Epic 6 | Online status |
| FR40 | Epic 6 | Private match invites |
| FR41 | Epic 6 | Block/report players |
| FR42 | Epic 6 | Friend suggestions |
| FR43 | Epic 7 | XP earning |
| FR44 | Epic 7 | Level up system |
| FR45 | Epic 7 | Coin earning |
| FR46 | Epic 7 | Coin spending |
| FR47 | Epic 7 | Statistics tracking |
| FR48 | Epic 1 | Guest play |
| FR49 | Epic 1 | Account linking |
| FR50 | Epic 1 | Facebook login |
| FR51 | Epic 1 | EOS authentication |
| FR52 | Epic 6 | Quick chat messages |
| FR53 | Epic 6 | Post-match lobby |
| FR54 | Epic 6 | Status indicators |

## Epic List

### Epic 1: Account & Onboarding
Players can immediately start playing without friction through guest access and optional account linking
**FRs covered:** FR48, FR49, FR50, FR51

### Epic 2: Matchmaking & Session Management
Players can instantly join matches with automatic bot filling and seamless networking
**FRs covered:** FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8

### Epic 3: Bot AI System
AI bots complete full cooking loops to ensure matches are playable even with few human players
**FRs covered:** FR16, FR17, FR18, FR19, FR20, FR21, FR22, FR23

### Epic 4: Core Cooking Gameplay
Players can navigate kitchens, interact with stations, process ingredients, and complete orders for scoring
**FRs covered:** FR24, FR25, FR26, FR27, FR28, FR29, FR30, FR31, FR32, FR33, FR34, FR35, FR36, FR37

### Epic 5: Character & Ability System
Players can select characters with unique abilities that add strategic depth to cooking matches
**FRs covered:** FR9, FR10, FR11, FR12, FR13, FR14, FR15

### Epic 6: Social Features
Players can connect with friends, invite them to matches, and communicate
**FRs covered:** FR38, FR39, FR40, FR41, FR42, FR52, FR53, FR54

### Epic 7: Progression & Unlocks
Players earn rewards, unlock characters, and track their improvement over time
**FRs covered:** FR43, FR44, FR45, FR46, FR47

---

## Epic 1: Account & Onboarding

Players can immediately start playing without friction through guest access and optional account linking

### Story 1.1: Guest Play Without Account Creation

As a new player,
I want to start playing immediately without creating an account,
So that I can experience the game with zero friction.

**Acceptance Criteria:**

**Given** the player has downloaded the app
**When** they launch it for the first time
**Then** they can tap "Guest Play" and immediately enter the main menu
**And** a guest account ID is generated and stored locally

**Given** the player is playing as a guest
**When** they close and reopen the app
**Then** their guest progress is retained on the same device

**Given** the player is using a guest account
**When** they participate in matches
**Then** their match history and basic stats are tracked

---

### Story 1.2: Link Guest Account to Permanent Account

As a guest player,
I want to link my guest account to a permanent account via email,
So that my progress is saved and accessible across devices.

**Acceptance Criteria:**

**Given** the player has a guest account with existing progress
**When** they navigate to Account Settings and select "Link Account"
**Then** they can enter an email and password to create a permanent account
**And** all guest progress is transferred to the new account

**Given** the player has linked their account
**When** they log in on a different device with the same credentials
**Then** their progress (level, unlocks, stats) is synchronized

**Given** the account linking process
**When** the player enters an email
**Then** email format validation is performed
**And** duplicate email checks prevent multiple accounts with same email

---

### Story 1.3: Facebook Authentication

As a player,
I want to authenticate using my Facebook account,
So that I can quickly start playing without creating new credentials.

**Acceptance Criteria:**

**Given** the player is on the login screen
**When** they tap "Login with Facebook"
**Then** the Facebook OAuth flow is initiated
**And** upon successful authentication, they are logged in

**Given** the player authenticates via Facebook
**When** they grant permission
**Then** their Facebook profile name and picture can be used as display name and avatar

**Given** the player has an existing account
**When** they link Facebook to their account
**Then** they can use Facebook login for future sessions
**And** existing progress is preserved

---

### Story 1.4: Epic Online Services Authentication

As a player,
I want to authenticate via Epic Online Services,
So that I can leverage cross-platform capabilities and secure authentication.

**Acceptance Criteria:**

**Given** the player is on the login screen
**When** they select EOS authentication
**Then** the Epic Online Services auth flow is initiated
**And** upon success, they are logged into the game

**Given** the player authenticates via EOS
**When** they access multiplayer features
**Then** their EOS identity is used for P2P networking and matchmaking

**Given** the player has an existing guest or linked account
**When** they connect EOS authentication
**Then** the accounts are merged or linked appropriately
**And** no progress is lost

---

## Epic 2: Matchmaking & Session Management

Players can instantly join matches with automatic bot filling and seamless networking

### Story 2.1: Quick Play Matchmaking

As a player,
I want to enter Quick Play matchmaking that fills empty slots with bots,
So that I can start a match instantly without waiting.

**Acceptance Criteria:**

**Given** the player is in the main menu
**When** they tap the "Play" button
**Then** matchmaking begins immediately
**And** a match is found or created within 5 seconds

**Given** a match is being formed
**When** there are fewer than 4 human players
**Then** AI bots automatically fill empty slots
**And** the match starts with a minimum of 4 total players (humans + bots)

**Given** the match is starting
**When** all slots are filled
**Then** the match countdown begins
**And** the game transitions to the kitchen level

---

### Story 2.2: Private Lobbies with Invite Links

As a player,
I want to create private lobbies and invite friends via shareable links,
So that I can play exclusively with people I choose.

**Acceptance Criteria:**

**Given** the player is in the main menu
**When** they select "Create Private Lobby"
**Then** a private lobby is created with a unique room code

**Given** the player is in a private lobby
**When** they tap "Invite Friends"
**Then** a shareable invite link is generated
**And** the link can be shared via WhatsApp, SMS, or other apps

**Given** another player receives an invite link
**When** they tap the link
**Then** the game opens (or App Store if not installed)
**And** they are dropped directly into the private lobby

**Given** the lobby has fewer than 4 players
**When** the host taps "Start Match"
**Then** remaining slots are filled with bots
**And** the match begins

---

### Story 2.3: Join Matches via Invite Links

As a player,
I want to join matches by clicking invite links or accepting friend invites,
So that I can easily play with friends.

**Acceptance Criteria:**

**Given** the player receives an invite link
**When** they tap the link with the app already open
**Then** a join confirmation dialog appears
**And** upon confirmation, they are transported to the lobby

**Given** the player accepts a friend invite
**When** the inviter is in a match or lobby
**Then** the player can choose to join immediately or later

**Given** the player taps an expired or invalid invite link
**When** the app opens
**Then** an error message is displayed
**And** the player is directed to the main menu

---

### Story 2.4: Bot Replacement for Disconnected Players

As a player in a match,
I want disconnected human players to be replaced by AI bots,
So that the match can continue without being ruined.

**Acceptance Criteria:**

**Given** a player disconnects mid-match
**When** the disconnection is detected (after 5 seconds of no response)
**Then** an AI bot immediately takes over their character
**And** gameplay continues without interruption

**Given** a bot has replaced a disconnected player
**When** the original player reconnects within 30 seconds
**Then** they can rejoin the match and resume control
**And** the bot is removed

**Given** the disconnected player does not return
**When** the match ends
**Then** the bot continues playing until completion
**And** match results are calculated normally

---

### Story 2.5: Host Migration on Disconnect

As a player in a P2P match,
I want host authority to migrate to another client if the host disconnects,
So that the match continues without failure.

**Acceptance Criteria:**

**Given** the current host disconnects mid-match
**When** the disconnection is detected
**Then** the system selects a new host based on ping and connection quality

**Given** a new host is selected
**When** the migration occurs
**Then** the match pauses for up to 3 seconds
**And** full match state is transferred to the new host
**And** gameplay resumes seamlessly

**Given** host migration is in progress
**When** all clients receive the new host assignment
**Then** they reconnect to the new host automatically
**And** no player action is required

**Given** the host disconnects and no suitable replacement is found
**When** after 10 seconds of attempts
**Then** the match ends gracefully
**And** players are returned to the main menu with an explanation

---

### Story 2.6: Match History

As a player,
I want to view my match history including scores and team compositions,
So that I can track my performance over time.

**Acceptance Criteria:**

**Given** the player navigates to their profile or stats section
**When** they select "Match History"
**Then** a list of recent matches is displayed

**Given** the match history is displayed
**When** the player views a match entry
**Then** they see: date/time, game mode, final score, team composition, and duration

**Given** the player taps on a specific match
**When** the match details load
**Then** detailed statistics are shown (personal score, team scores, characters used)

**Given** the player has many matches
**When** they scroll through history
**Then** at least 50 recent matches are available
**And** older matches can be loaded on demand

---

### Story 2.7: Team Formation in Lobby

As a player,
I want to form teams in the lobby for team-based game modes,
So that I can coordinate with friends or be matched with allies.

**Acceptance Criteria:**

**Given** the game mode supports teams (2v2)
**When** players are in the lobby
**Then** they can see team assignments (Red Team vs Blue Team)

**Given** the player is in a private lobby
**When** they are the host
**Then** they can manually assign players to teams
**And** balance teams as desired

**Given** the lobby has open slots
**When** the match starts
**Then** teams are balanced automatically if not manually set
**And** the match begins with proper team composition

---

## Epic 3: Bot AI System

AI bots complete full cooking loops to ensure matches are playable even with few human players

### Story 3.1: Bot Navigation to Stations

As a match host,
I want AI bots to navigate kitchen environments to reach stations,
So that bots can participate in cooking activities.

**Acceptance Criteria:**

**Given** a bot is active in the match
**When** the bot decides to move to a station
**Then** it calculates a path using the navigation mesh
**And** moves to the target station efficiently

**Given** the bot is navigating
**When** obstacles or other players block the path
**Then** the bot recalculates the path dynamically
**And** avoids getting stuck

**Given** multiple valid stations exist
**When** the bot selects a destination
**Then** it chooses the optimal path based on distance and current needs

---

### Story 3.2: Bot Ingredient Pickup

As a match host,
I want AI bots to identify and pick up ingredients from storage crates,
So that they can gather materials for cooking.

**Acceptance Criteria:**

**Given** a bot needs an ingredient
**When** it locates the appropriate storage crate
**Then** it navigates to the crate and interacts with it
**And** picks up the required ingredient

**Given** the bot is carrying an ingredient
**When** it decides to pick up another
**Then** it checks capacity limits
**And** drops or replaces items according to priority

**Given** multiple ingredient types are needed
**When** the bot plans its actions
**Then** it optimizes the pickup route to minimize travel time

---

### Story 3.3: Bot Station Processing

As a match host,
I want AI bots to process ingredients at appropriate stations,
So that they can prepare food items for serving.

**Acceptance Criteria:**

**Given** a bot has raw ingredients
**When** it identifies the correct processing station (cutting board, stove, etc.)
**Then** it navigates there and initiates the processing action

**Given** the bot is processing an item
**When** the processing completes
**Then** the bot picks up the processed item
**And** moves to the next step (further processing or plating)

**Given** a station is occupied by another player
**When** the bot needs to use it
**Then** it either waits or selects an alternative station if available

---

### Story 3.4: Bot Order Assembly and Serving

As a match host,
I want AI bots to assemble completed dishes and deliver them to serving stations,
So that they contribute to team scoring.

**Acceptance Criteria:**

**Given** a bot has all required processed ingredients
**When** it locates an empty plate
**Then** it assembles the dish by placing ingredients on the plate

**Given** a dish is complete
**When** the bot identifies the serving station
**Then** it navigates there and delivers the order
**And** points are awarded to the team

**Given** multiple orders are pending
**When** the bot decides which to serve
**Then** it prioritizes based on expiration time and complexity

---

### Story 3.5: Bot Order Prioritization

As a match host,
I want AI bots to prioritize orders based on expiration time and complexity,
So that they make strategic decisions that help the team.

**Acceptance Criteria:**

**Given** multiple active orders exist
**When** the bot evaluates what to cook next
**Then** it considers: time remaining, complexity, and ingredients available

**Given** an order is about to expire
**When** the bot can complete it quickly
**Then** it prioritizes that order to prevent point loss

**Given** the bot has limited time or resources
**When** choosing between orders
**Then** it selects the optimal order for maximum point contribution

---

### Story 3.6: Bot Player Interaction

As a match host,
I want AI bots to interact with other players (collision, ability effects),
So that they participate naturally in the chaotic kitchen environment.

**Acceptance Criteria:**

**Given** players or other bots are nearby
**When** the bot moves through the kitchen
**Then** collision detection works naturally
**And** the bot avoids excessive blocking of human players

**Given** a player uses an ability affecting the bot (Push, Freeze, etc.)
**When** the ability hits the bot
**Then** the bot responds appropriately (moved, stunned, etc.)
**And** resumes normal behavior after the effect ends

**Given** the bot is in a crowded area
**When** navigating around players
**Then** it chooses alternative routes to minimize interference

---

### Story 3.7: Bot Difficulty Scaling

As the game system,
I want to scale AI bot difficulty based on player skill level,
So that matches remain challenging and engaging.

**Acceptance Criteria:**

**Given** players have varying skill levels
**When** bots are added to a match
**Then** bot difficulty is set based on the average player skill in the lobby

**Given** a player is in their first few matches
**When** bots fill slots
**Then** Easy difficulty bots are used (slower, more forgiving)

**Given** experienced players are in the match
**When** bots fill slots
**Then** Hard difficulty bots are used (faster, more efficient)

**Given** the match is in progress
**When** the system detects one-sided matches
**Then** bot difficulty can be adjusted dynamically to balance gameplay

---

## Epic 4: Core Cooking Gameplay

Players can navigate kitchens, interact with stations, process ingredients, and complete orders for scoring

### Story 4.1: Touch Control Movement

As a player,
I want to move my character via touch controls to navigate the kitchen,
So that I can reach stations and ingredients quickly.

**Acceptance Criteria:**

**Given** the player is in a match
**When** they touch and drag the left virtual stick
**Then** the character moves in the corresponding direction
**And** movement feels responsive and smooth

**Given** the player releases the stick
**When** the touch ends
**Then** the character stops moving immediately

**Given** the player double-taps the movement stick
**When** in a valid game mode
**Then** the character sprints at increased speed

**Given** the player navigates around obstacles
**When** collision occurs
**Then** the character stops or slides naturally
**And** visual feedback indicates the collision

---

### Story 4.2: Station Interaction

As a player,
I want to interact with cooking stations by tapping them,
So that I can process ingredients and prepare food.

**Acceptance Criteria:**

**Given** the player is near a station
**When** they tap the station or the action button
**Then** the appropriate interaction begins (chopping, cooking, washing, etc.)

**Given** the player is holding an ingredient
**When** they interact with a compatible station
**Then** the ingredient is placed on the station for processing

**Given** a station is already occupied
**When** the player tries to interact
**Then** they receive feedback that the station is busy
**And** they cannot place items there

**Given** a station has processed items ready
**When** the player interacts with it
**Then** they pick up the completed item

---

### Story 4.3: Item Pickup and Carrying

As a player,
I want to pick up and carry ingredients and prepared items,
So that I can transport materials around the kitchen.

**Acceptance Criteria:**

**Given** the player is near an item
**When** they tap the action button
**Then** they pick up the item and hold it visibly

**Given** the player is already carrying an item
**When** they pick up another item
**Then** the new item replaces the old one (or stacks if applicable)

**Given** the player is carrying an item
**When** they move around the kitchen
**Then** the item remains in their hands
**And** is visible to other players

**Given** the player is carrying multiple stackable items (e.g., 3 tomatoes)
**When** they look at their character
**Then** the held items are visually represented

---

### Story 4.4: Ingredient Placement

As a player,
I want to place ingredients on empty stations or plates for other players to use,
So that we can collaborate on order preparation.

**Acceptance Criteria:**

**Given** the player is carrying an ingredient
**When** they approach an empty station or plate
**Then** they can place the ingredient there

**Given** the player places an ingredient on a station
**When** another player approaches
**Then** they can see and interact with the placed ingredient

**Given** the player places an ingredient on a plate
**When** it matches the order requirements
**Then** the plate shows partial completion

**Given** the player tries to place an incompatible item
**When** they attempt the action
**Then** they receive feedback that the placement is invalid

---

### Story 4.5: Cooking Progress Tracking

As a player,
I want the system to track cooking progress and alert me when items are ready,
So that I can time my actions correctly.

**Acceptance Criteria:**

**Given** an item is being processed at a station
**When** time passes
**Then** a progress bar or visual indicator shows completion percentage

**Given** an item is nearly complete
**When** it reaches 90% progress
**Then** a visual alert (glow, icon, or color change) notifies the player

**Given** the player is away from the station
**When** processing completes
**Then** an audio cue plays (if nearby)
**And** a visual indicator remains until the item is collected

**Given** an item sits too long after completion
**When** the expiration timer runs out
**Then** the item becomes invalid or burnt

---

### Story 4.6: Item Destruction for Burnt/Expired Items

As a player,
I want burnt or expired items to be destroyed automatically,
So that the kitchen doesn't get cluttered with invalid items.

**Acceptance Criteria:**

**Given** an item is cooking on a stove
**When** it cooks beyond the valid time
**Then** it becomes "burnt" and turns black

**Given** an item is burnt or expired
**When** a few seconds pass
**Then** it is automatically removed from the station
**And** a smoke/fire effect may play

**Given** a player tries to serve an expired item
**When** they attempt to deliver it
**Then** the action is rejected
**And** feedback explains the item is no longer valid

---

### Story 4.7: Order Generation System

As a player,
I want the system to generate random orders with varying complexity,
So that each match has unique challenges.

**Acceptance Criteria:**

**Given** a match is active
**When** the game mode determines new orders are needed
**Then** orders are generated based on the game mode rules

**Given** orders are generated
**When** they appear on the order display
**Then** they show: recipe icon, required ingredients, and time limit

**Given** the game mode is Classic
**When** orders generate
**Then** complexity varies from simple (1-2 ingredients) to complex (4-5 ingredients)

**Given** multiple orders are active simultaneously
**When** displayed on screen
**Then** they are arranged by priority (closest to expiring first)

---

### Story 4.8: Order Time Limits

As a player,
I want orders to have time limits based on complexity,
So that there's urgency and challenge in completing them.

**Acceptance Criteria:**

**Given** a new order is generated
**When** it appears
**Then** a countdown timer starts based on the order's complexity

**Given** a simple order (1-2 ingredients)
**When** the timer starts
**Then** it has approximately 45-60 seconds

**Given** a complex order (4-5 ingredients)
**When** the timer starts
**Then** it has approximately 90-120 seconds

**Given** the timer is running low
**When** it reaches the final 10 seconds
**Then** the timer turns red and may pulse for visibility

---

### Story 4.9: Order Serving and Scoring

As a player,
I want to serve completed orders to score points for my team,
So that I can contribute to winning the match.

**Acceptance Criteria:**

**Given** the player has a completed dish on a plate
**When** they navigate to the serving station and interact
**Then** the order is submitted and points are awarded

**Given** an order is served
**When** the ingredients match the order exactly
**Then** full points are awarded

**Given** an order is served with incorrect ingredients
**When** submitted
**Then** reduced or zero points are awarded
**And** feedback indicates what was wrong

**Given** an order is served quickly after completion
**When** within combo window
**Then** bonus points are awarded for combo streaks

---

### Story 4.10: Score Calculation

As a player,
I want the system to calculate scores based on order complexity, speed, and accuracy,
So that skilled play is rewarded appropriately.

**Acceptance Criteria:**

**Given** an order is completed
**When** points are calculated
**Then** base points depend on complexity (simple=50, complex=150)

**Given** an order is served quickly
**When** within optimal time window
**Then** speed bonus is added (up to 50% of base points)

**Given** consecutive orders are served quickly
**When** within combo time window (5 seconds)
**Then** combo multiplier applies (2x, 3x, up to 5x max)

**Given** the match ends
**When** final scores are tallied
**Then** team scores are the sum of all individual contributions

---

### Story 4.11: Win Conditions

As a player,
I want the system to determine match winners based on final scores or time expiration,
So that matches have clear outcomes.

**Acceptance Criteria:**

**Given** the match timer reaches zero
**When** time expires
**Then** the match ends and the team with highest score wins

**Given** a team reaches the target score (if applicable)
**When** the winning order is served
**Then** the match ends immediately and that team wins

**Given** scores are tied at match end
**When** the winner is determined
**Then** the match is declared a tie or sudden death rules apply

**Given** the match ends
**When** the winner is declared
**Then** all players see the victory/defeat screen
**And** match statistics are displayed

---

### Story 4.12: Real-Time Scoreboard

As a player,
I want to view real-time scoreboards during matches,
So that I know how my team is performing.

**Acceptance Criteria:**

**Given** a match is in progress
**When** the player looks at the screen
**Then** the current scores for both teams are visible

**Given** points are scored
**When** the score changes
**Then** the scoreboard updates immediately
**And** visual/audio feedback indicates the point gain

**Given** the player wants more details
**When** they tap the scoreboard area
**Then** an expanded view shows individual player contributions

---

### Story 4.13: Personal Bests and Session Statistics

As a player,
I want the system to track my personal bests and session statistics,
So that I can see my improvement over time.

**Acceptance Criteria:**

**Given** a match completes
**When** the player views results
**Then** their individual stats are shown (orders served, accuracy, personal score)

**Given** the player achieves a new personal best
**When** any statistic is exceeded
**Then** a "New Record!" notification appears
**And** the new record is saved

**Given** the player views their profile
**When** they check statistics
**Then** they see: total matches, win rate, favorite character, highest score

**Given** a gaming session includes multiple matches
**When** the session ends
**Then** session stats show total orders served, average score, and best match

---

## Epic 5: Character & Ability System

Players can select characters with unique abilities that add strategic depth to cooking matches

### Story 5.1: Character Selection

As a player,
I want to select from unlocked characters before match start,
So that I can play with my preferred style and abilities.

**Acceptance Criteria:**

**Given** the player is in the pre-match lobby
**When** they view the character selection screen
**Then** all unlocked characters are displayed with their abilities

**Given** the player taps a character
**When** the character is unlocked
**Then** that character is selected for the match
**And** other players can see the selection

**Given** the player has not selected a character
**When** the match countdown begins
**Then** a default character is auto-selected

**Given** the player views a locked character
**When** they try to select it
**Then** they see the unlock requirements (level, coins, etc.)

---

### Story 5.2: Passive Abilities

As a player,
I want characters to have unique passive abilities that affect gameplay,
So that each character feels different to play.

**Acceptance Criteria:**

**Given** a character has a passive ability
**When** the match starts
**Then** the passive effect is active for the entire match

**Given** Chef Swift (SpeedBoost passive)
**When** the character moves
**Then** base movement speed is increased by 15%

**Given** Chef Strong (Carry capacity passive)
**When** carrying items
**Then** the character can carry 2 items instead of 1

**Given** any passive ability is active
**When** the player views their character status
**Then** a passive ability indicator is visible

---

### Story 5.3: Ability Activation (5 MVP Abilities)

As a player,
I want to activate character abilities during matches with cooldown constraints,
So that I can use strategic skills to gain advantage.

**Acceptance Criteria:**

**Given** the player has selected a character with an active ability
**When** they tap the ability button
**Then** the ability activates (if off cooldown)

**Given** SpeedBoost ability is activated
**When** the effect starts
**Then** movement speed doubles for 5 seconds
**And** a visual effect indicates the boost

**Given** Push ability is activated
**When** executed
**Then** nearby enemy players are pushed back 3 meters
**And** they may drop held items

**Given** Magnet ability is activated
**When** the effect is active
**Then** ingredients within 5 meters are automatically pulled toward the player

**Given** InstantCook ability is activated
**When** aimed at a station with processing items
**Then** those items complete instantly

**Given** FreezeTime ability is activated
**When** executed
**Then** all order timers pause for 3 seconds

**Given** any ability is used
**When** the cooldown period begins
**Then** the ability button shows a cooldown overlay
**And** the ability cannot be used again until ready

---

### Story 5.4: Server-Authoritative Ability Validation

As the game system,
I want ability usage to be validated server-authoritatively,
So that cheating is prevented and competitive integrity is maintained.

**Acceptance Criteria:**

**Given** the player attempts to activate an ability
**When** the activation request is sent
**Then** the host validates: cooldown status, player state, and ability availability

**Given** the ability validation passes
**When** confirmed by host
**Then** the ability effect is broadcast to all clients
**And** the effect is applied consistently

**Given** the ability validation fails (cooldown active, etc.)
**When** the host rejects the request
**Then** the activation is denied silently
**And** the player's client is reconciled with server state

**Given** a player attempts to exploit (rapid-fire abilities)
**When** multiple invalid requests are detected
**Then** the system may flag or throttle the player

---

### Story 5.5: Ability Usage Statistics

As a player,
I want the system to track ability usage statistics per player,
So that I can see how effectively I use my character's abilities.

**Acceptance Criteria:**

**Given** abilities are used during matches
**When** the match ends
**Then** ability usage stats are recorded (times used, hit rate for offensive abilities)

**Given** the player views their profile
**When** checking character stats
**Then** they see ability usage per character (total activations, effectiveness)

**Given** the player uses abilities effectively
**When** viewing post-match results
**Then** ability-related achievements or highlights are shown

---

### Story 5.6: Character Unlocks

As a player,
I want to unlock new characters through gameplay progression,
So that I have goals to work toward and variety in gameplay.

**Acceptance Criteria:**

**Given** the player earns XP and levels up
**When** they reach certain level milestones
**Then** new characters become unlocked

**Given** the player has coins
**When** they view the shop
**Then** they can spend coins to unlock characters early

**Given** a character is unlocked
**When** the player selects it
**Then** it is available for use in matches immediately

**Given** special events or achievements
**When** completed
**Then** exclusive characters may be unlocked as rewards

---

### Story 5.7: Cooldown Enforcement

As the game system,
I want to enforce ability cooldowns and duration limits,
So that abilities are balanced and not spammed.

**Acceptance Criteria:**

**Given** an ability is activated
**When** the effect duration expires
**Then** the ability effect ends naturally

**Given** an ability is on cooldown
**When** the player tries to activate it
**Then** the action is rejected
**And** visual feedback shows remaining cooldown time

**Given** cooldown reduction mechanics exist (future feature)
**When** applied
**Then** cooldowns are calculated correctly

**Given** the match ends
**When** a new match starts
**Then** all cooldowns are reset to ready state

---

## Epic 6: Social Features

Players can connect with friends, invite them to matches, and communicate

### Story 6.1: Friend Invites via Contacts

As a player,
I want to send friend invites via contact list synchronization,
So that I can easily find and play with people I know.

**Acceptance Criteria:**

**Given** the player grants contacts permission
**When** they access the friends section
**Then** contacts who play RecipeRage are suggested as friends

**Given** the player finds a contact to invite
**When** they send an invite
**Then** the recipient receives a notification
**And** they can accept or decline the friend request

**Given** a friend request is accepted
**When** confirmed by both parties
**Then** the players become friends in the game
**And** they can see each other's online status

---

### Story 6.2: Online Status Display

As a player,
I want to view my friends' online status and current activity,
So that I know when they are available to play.

**Acceptance Criteria:**

**Given** the player views their friends list
**When** looking at a friend
**Then** they see the current status: Online, In Match, or Offline

**Given** a friend is Online
**When** viewing their status
**Then** the player can see if they are in a menu, lobby, or match

**Given** a friend is In Match
**When** the match is joinable (private lobby)
**Then** a "Join" option may be available

**Given** status changes occur
**When** a friend goes online or offline
**Then** the status updates in real-time (or within a few seconds)

---

### Story 6.3: Private Match Invites

As a player,
I want to invite friends to private matches,
So that we can play together exclusively.

**Acceptance Criteria:**

**Given** the player is in a private lobby
**When** they open the invite menu
**Then** they see a list of online friends

**Given** the player selects friends to invite
**When** invites are sent
**Then** invited friends receive notifications
**And** they can tap to join directly

**Given** invited friends join
**When** they enter the lobby
**Then** the host sees them arrive
**And** can manage the lobby (start match, remove players, etc.)

---

### Story 6.4: Block and Report Players

As a player,
I want to block or report other players,
So that I can avoid toxic behavior and maintain a positive experience.

**Acceptance Criteria:**

**Given** the player encounters another player
**When** they access the player's profile or post-match screen
**Then** they can select "Block" or "Report"

**Given** a player is blocked
**When** they are in the same matchmaking queue
**Then** the blocker will not be matched with them

**Given** a player is reported
**When** multiple reports are received
**Then** the system flags the account for review
**And** appropriate action is taken if violations are confirmed

**Given** the player views their blocked list
**When** in settings
**Then** they can see and manage blocked players
**And** unblock if desired

---

### Story 6.5: Friend Suggestions

As a player,
I want the system to suggest friends based on match history and contacts,
So that I can expand my social circle in the game.

**Acceptance Criteria:**

**Given** the player has completed matches
**When** viewing the friends section
**Then** players from recent matches are suggested as potential friends

**Given** the player has contacts not yet added
**When** viewing suggestions
**Then** contacts who play RecipeRage are prominently suggested

**Given** the player accepts a suggestion
**When** they send a friend request
**Then** the request is sent
**And** if accepted, they become friends

---

### Story 6.6: Quick Chat Messages

As a player,
I want to send pre-set quick chat messages in the lobby,
So that I can communicate quickly without typing.

**Acceptance Criteria:**

**Given** the player is in a pre-match or post-match lobby
**When** they open the quick chat menu
**Then** pre-set message options are displayed

**Given** the player selects a quick chat message
**When** sent
**Then** the message appears to all players in the lobby
**And** may include emotes or animations

**Given** pre-set messages
**When** displayed
**Then** they are localized based on player language settings

**Examples of quick chat messages:**
- "Good game!"
- "Let's play again"
- "Nice moves!"
- "Thanks!"

---

### Story 6.7: Post-Match Lobby

As a player,
I want to view a post-match lobby with recent teammates and opponents,
So that I can connect with players I enjoyed playing with.

**Acceptance Criteria:**

**Given** a match ends
**When** the results are displayed
**Then** a post-match lobby shows all participants

**Given** the post-match lobby is displayed
**When** viewing participants
**Then** each player shows: name, character used, score, and friend options

**Given** the player wants to friend someone
**When** they tap the player
**Then** a friend request can be sent

**Given** the player wants to play again
**When** they tap "Play Again"
**Then** they return to matchmaking or lobby
**And** can invite the same players

---

### Story 6.8: Status Indicators

As a player,
I want the system to display player status indicators (online, in-match, away),
So that I know who is available to play.

**Acceptance Criteria:**

**Given** the player views their friends list or lobby
**When** looking at any player
**Then** a status indicator icon is visible

**Given** status states
**When** displayed
**Then** they use clear visual coding:
- Green = Online/Available
- Yellow = In Match
- Gray = Offline
- Red = Do Not Disturb (if implemented)

**Given** status changes
**When** a player goes offline or online
**Then** the indicator updates within 5-10 seconds

---

## Epic 7: Progression & Unlocks

Players earn rewards, unlock characters, and track their improvement over time

### Story 7.1: Experience Points and Leveling

As a player,
I want to earn experience points through match participation and performance,
So that I can level up and show my dedication to the game.

**Acceptance Criteria:**

**Given** a match completes
**When** results are calculated
**Then** XP is awarded based on: participation, score, win/loss, and individual performance

**Given** the player earns XP
**When** enough XP is accumulated
**Then** the player levels up
**And** a level-up animation and notification appears

**Given** the player levels up
**When** reaching certain milestones
**Then** rewards are unlocked (characters, coins, cosmetics)

**Given** the player views their profile
**When** checking their level
**Then** current level and progress to next level are displayed

---

### Story 7.2: Soft Currency (Coins)

As a player,
I want to earn soft currency (coins) through gameplay,
So that I can purchase unlocks and cosmetics.

**Acceptance Criteria:**

**Given** a match completes
**When** results are calculated
**Then** coins are awarded based on performance and match outcome

**Given** daily login
**When** the player opens the app
**Then** they may receive daily login coin rewards

**Given** achievements are completed
**When** milestones are reached
**Then** bonus coins may be awarded

**Given** the player views their profile
**When** checking currency
**Then** current coin balance is displayed prominently

---

### Story 7.3: Character and Cosmetic Unlocks

As a player,
I want to spend coins to unlock characters, skins, and cosmetics,
So that I can customize my appearance and gameplay options.

**Acceptance Criteria:**

**Given** the player has enough coins
**When** they view an unlockable character in the shop
**Then** they can purchase it with coins

**Given** the player purchases a character
**When** the transaction completes
**Then** the character is immediately unlocked
**And** available in character selection

**Given** the player views cosmetics
**When** in the shop
**Then** they can purchase: character skins, taunts, emotes, and profile customizations

**Given** the player has insufficient coins
**When** attempting to purchase
**Then** they are informed of the coin deficit
**And** may be directed to gameplay to earn more

---

### Story 7.4: Player Statistics Tracking

As a player,
I want the system to track my statistics (matches played, win rate, favorite character),
So that I can see my play patterns and improvement.

**Acceptance Criteria:**

**Given** the player views their profile
**When** checking statistics
**Then** they see:
- Total matches played
- Win rate percentage
- Favorite character (most played)
- Total orders served
- Highest single-match score

**Given** statistics are tracked
**When** matches are completed
**Then** all stats are updated in real-time

**Given** the player wants to compare
**When** viewing stats
**Then** they can see their progress over time (week, month, all-time)

**Given** the player achieves milestones
**When** certain stat thresholds are reached
**Then** achievements or badges may be awarded

---
