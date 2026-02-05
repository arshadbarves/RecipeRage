---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - prd.md
  - product-brief-RecipeRage-2026-02-02.md
workflowType: architecture
project_name: 'RecipeRage'
user_name: 'Arshad'
date: '2026-02-04'
lastStep: 8
status: complete
completedAt: '2026-02-04'
---

# Architecture Decision Document - RecipeRage

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Overview

This architecture document will capture all critical technical decisions for RecipeRage - a mobile-first, online multiplayer cooking competition game built with Unity 6.0, Netcode for GameObjects, and Epic Online Services.

## Input Documents

- **PRD**: Product Requirements Document with detailed feature specifications
- **Product Brief**: Strategic vision, target users, and success metrics

---

## Established Technical Foundation

RecipeRage is built on an existing Unity codebase with established architectural patterns. The following technical stack is **LOCKED IN** as the foundation for all architectural decisions:

### Core Engine & Networking Stack

| Component | Technology | Status | Notes |
|-----------|------------|--------|-------|
| **Game Engine** | Unity 6.0 | Active | Fallback to 2022 LTS if critical stability issues |
| **Networking** | Netcode for GameObjects (NGO) | 85% Complete | Authoritative server-client architecture |
| **P2P Relay** | Epic Online Services (EOS) | 85% Complete | Relay fallback, matchmaking, authentication |
| **DI Framework** | VContainer | Active | Clean architecture, dependency injection |
| **Async Programming** | UniTask | Active | Performance-optimized async/await |
| **Analytics** | Firebase | Planned | Crash reporting, analytics, push notifications |

### Established Codebase Systems

The following systems are production-ready and establish architectural patterns:

- **PlayerController** (90% complete) - Movement, networking, carrying mechanics
- **StationSystem** (85% complete) - 9 functional station types
- **OrderManagement** (80% complete) - Generation, completion, expiration
- **Matchmaking** (85% complete) - PUBG-style with bot filling
- **NetworkInfrastructure** (85% complete) - NGO + EOS integration

### Platform Strategy (LOCKED)

- **Mobile-First**: iOS 13+, Android API 26+
- **Always-Online**: No offline mode (Brawl Stars model)
- **Touch-Only**: Touch-optimized controls, no gamepad support initially

### Rationale for Locking Current Stack

1. **Timeline Reality**: 6-week MVP deadline makes any stack change impossible
2. **Proven Foundation**: 85%+ complete networking infrastructure is functional
3. **Appropriate for Requirements**: NGO + EOS fit the P2P competitive gaming needs
4. **Clean Architecture**: VContainer + UniTask show solid engineering practices

---

## Project Context Analysis

### Requirements Overview

**Functional Requirements (55 Total):**

The project spans 10 major functional areas:
1. **Matchmaking & Session Management** (8 FRs): Quick Play, private lobbies, P2P sessions, bot filling, host migration
2. **Character & Ability System** (7 FRs): 5 MVP abilities (SpeedBoost, Push, Magnet, InstantCook, FreezeTime), server-authoritative validation
3. **Bot AI System** (8 FRs): Full cooking loop - navigation, ingredient pickup, station interaction, order fulfillment
4. **Cooking & Station Mechanics** (6 FRs): Touch controls, station interactions, progress tracking
5. **Order & Scoring System** (8 FRs): Dynamic orders, combo scoring, win conditions
6. **Social & Friends** (5 FRs): Contact sync, online status, invites, blocking
7. **Progression & Unlocks** (5 FRs): XP/leveling (1-20+), coin economy, character unlocks
8. **Account & Authentication** (4 FRs): Guest play, Facebook login, EOS integration, account linking
9. **Communication** (3 FRs): Quick chat (pre-set messages), post-match lobby, status indicators
10. **Game Mode System** (Multiple modes): Classic (MVP), Time Attack (v1.3), Team Battle (v1.3), Kitchen Wars (v2.0)

**Shop & Economy Features:**
- Soft currency (coins) earned through gameplay
- Character unlocks (500-2000 coins)
- Cosmetics shop (skins, taunts, emotes)
- Season Pass premium track ($4.99)
- Daily login rewards

**Settings & Configuration:**
- Audio controls (SFX, music, volume)
- Touch control sensitivity
- Haptic feedback toggle
- Push notification preferences (match alerts, friend activity, events)
- Account management (guest/link)

**Non-Functional Requirements (36 Total):**

*Network Performance (P0 - Core Differentiator):*
- P2P latency <100ms (95th percentile)
- Matchmaking <5 seconds
- 30Hz game state sync tick rate
- Client-side prediction masking up to 150ms latency

*Game Performance:*
- 30+ FPS on mid-tier devices (iPhone 8 equivalent)
- <200MB RAM usage
- <100MB initial download
- Touch input <50ms response

*Reliability:*
- >95% match completion rate
- 30-second reconnection window
- <0.1% crash rate per session

*Security:*
- Server-authoritative ability validation
- Anti-cheat measures
- COPPA compliance (no under-13 data collection)

**Scale & Complexity:**

- **Primary domain**: Mobile multiplayer game (online-only)
- **Complexity level**: **HIGH**
- **Estimated architectural components**: 15+ major systems

*Complexity Drivers:*
- Real-time P2P networking with host authority
- AI bot integration with networking layer
- Cross-platform mobile (iOS/Android)
- Server-authoritative validation for competitive integrity
- Multiple game modes with different rules
- In-game economy and progression systems

### Technical Constraints & Dependencies

**Engine & Stack:**
- Unity 6.0 (fallback to 2022 LTS if critical issues)
- Netcode for GameObjects (NGO) - authoritative server-client
- Epic Online Services (EOS) - P2P relay, matchmaking, auth
- VContainer - dependency injection
- UniTask - async programming
- Firebase - analytics, crash reporting, push notifications

**Platform Requirements:**
- iOS 13.0+ (Game Center integration)
- Android API 26+ (Google Play Games Services)
- Always-online (no offline mode)
- Touch-optimized controls only (no gamepad support initially)

**Infrastructure:**
- Regional server deployment (Asia-Pacific, North America, Europe)
- Auto-scaling for peak hours
- Graceful degradation (longer matchmaking vs server overload)

**MVP Critical Gaps:**
- Bot AI: 20% complete (random movement only)
- Character abilities: 15% complete (1 of 12 implemented)
- Win conditions: 50% complete (logic commented out)
- ~15 critical TODO items blocking gameplay

### Cross-Cutting Concerns Identified

1. **Networking Layer** - Affects every system; P2P host authority creates complexity for state sync, disconnection handling, and cheat prevention

2. **Server-Authoritative Validation** - Security requirement that impacts ability system, scoring, and game mode logic; must validate on host while preventing host cheating

3. **Bot AI Integration** - AI must work seamlessly with networking, fill slots dynamically, replace disconnected players, and maintain consistent state across all clients

4. **State Synchronization** - Match state, player progression, and economy must sync reliably; local backup with retry for cloud sync failures

5. **Host Management** - Smart host selection, host migration on disconnect, graceful handling of host advantage concerns

6. **Game Mode Variability** - Different win conditions, scoring rules, and mechanics per mode require flexible architecture

7. **Platform Abstraction** - iOS/Android differences for auth, payments, notifications, and store compliance

8. **Real-time Performance** - 30Hz tick rate, <100ms latency, smooth animations while maintaining competitive integrity

---

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block MVP Implementation):**

1. **Bot AI Architecture** - Determines how AI integrates with networking layer
2. **P2P Networking Patterns** - Host authority, selection, migration protocols
3. **Ability System Framework** - Server-authoritative validation pattern
4. **Game Mode Architecture** - Win condition abstraction and match state machine

**Important Decisions (Shape Implementation Quality):**

5. **State Synchronization Strategy** - Layered reliability for different data types
6. **Testing Infrastructure** - Validation patterns for AI, networking, balance

**Deferred Decisions (Post-MVP):**

- Advanced anti-cheat measures (beyond server-authoritative)
- Analytics pipeline architecture (Firebase basic setup sufficient)
- PC/Console cross-play infrastructure (v2.0)

---

### Decision 1: Bot AI Architecture - HOST-ONLY AI

**Status:** ✅ DECIDED

**Decision:** Bot AI runs exclusively on the host client using NGO networking patterns.

**Rationale:**
- Leverages existing NGO infrastructure (85% complete)
- Single source of truth prevents desync issues
- Fastest path to MVP (6-week timeline)
- Aligns with "no dedicated servers" cost constraint

**Implementation Pattern:**
```csharp
// Host runs AI decision cycle
if (IsHost)
{
    botDecisionEngine.Update(); // 10Hz tick rate
    BroadcastBotActionsClientRpc(actions);
}

// Clients receive and visualize
[ClientRpc]
void BroadcastBotActionsClientRpc(BotActionData actions)
{
    // Update bot visuals, no logic
}
```

**Host Migration Handling:**
When host disconnects → migrate authority → new host takes over AI computation seamlessly.

**Architecture Components:**
- `BotDecisionEngine` - Runs on host only
- `BotPerceptionSystem` - Detects stations, ingredients, orders
- `BotActionExecutor` - Translates decisions into NGO RPC calls
- `BotStateMachine` - IDLE, GATHERING, PREPARING, ASSEMBLING, SERVING

---

### Decision 2: P2P Networking Architecture - NGO HOST AUTHORITY

**Status:** ✅ DECIDED

**Decision:** Standard NGO host authority with custom host selection algorithm and migration protocol.

**Key Components:**

**A. Host Authority Model**
- Host makes authoritative decisions for gameplay state
- Clients predict locally, server reconciles
- 30Hz state sync tick rate (NGO NetworkVariables)

**B. Smart Host Selection Algorithm**
```csharp
public class HostSelector
{
    public Client SelectBestHost(List<Client> candidates)
    {
        return candidates
            .OrderBy(c => c.AveragePing)
            .ThenBy(c => c.ConnectionQuality)
            .ThenBy(c => c.DeviceCapability)
            .First();
    }
}
```

**C. Host Migration Protocol**
- **Trigger:** Current host disconnects or latency >200ms for 10+ seconds
- **Process:** Pause match (3 sec) → select new host → transfer authority → resume
- **State Preservation:** Full match state transferred via NetworkVariables

**D. Server-Authoritative Validation**
- All ability activations validated by host using [ServerRpc]
- Score submissions include replay hash for verification
- Client-side prediction + server reconciliation for smooth feel

**Network State Categories:**
- **Critical State** (Reliable): Match phase, scores, order expiration
- **Gameplay State** (30Hz): Player positions, station states, ability effects
- **Cosmetic State** (Unreliable): Animations, particles, UI feedback

---

### Decision 3: Ability System Framework - HOST-AUTHORITATIVE

**Status:** ✅ DECIDED

**Decision:** Host-authoritative ability system with 3 effect type categories.

**Core Pattern:**
```csharp
// Client Request → Host Validation → Broadcast Effect
[ServerRpc]
void ActivateAbilityServerRpc(AbilityType type, Vector3 target)
{
    // Host validates cooldown, range, state
    if (!CanActivate(type)) return;
    
    // Apply effect based on type
    ApplyAbilityEffect(type, target);
    
    // Broadcast to all clients
    NotifyAbilityActivatedClientRpc(type, target);
}
```

**Effect Types:**

1. **Immediate** (Push, InstantCook)
   - Host applies instantly
   - Single RPC call
   
2. **Duration** (SpeedBoost, FreezeTime)
   - Host manages duration timer
   - Start/End RPCs bracket effect period
   
3. **Passive** (Magnet)
   - Host checks conditions each frame
   - NetworkVariable for state sync

**Cooldown Management:**
- Host tracks all cooldowns authoritatively
- Competitive integrity prioritized over minimal network overhead
- Cooldown state syncs via NetworkVariables

**Network Sync Strategy:**
- Ability activation: Reliable RPC (must arrive)
- Effect updates: NetworkVariables (automatic sync)
- Visual effects: Client-side only (non-authoritative)

---

### Decision 4: Game Mode Framework - STRATEGY PATTERN + STATE MACHINE

**Status:** ✅ DECIDED

**Decision:** Strategy Pattern for win conditions + Hierarchical State Machine for match phases + Data-Driven Scoring Rules

**Win Condition Architecture:**
```csharp
public interface IWinConditionStrategy
{
    WinCheckResult CheckWinCondition(MatchState state);
    void Initialize(GameModeConfig config);
    void OnMatchTick(MatchState state, float deltaTime);
}

// Classic Mode: Score-based when time expires
public class ScoreBasedWinCondition : IWinConditionStrategy
{
    public WinCheckResult CheckWinCondition(MatchState state)
    {
        if (state.RemainingTime <= 0)
            return WinCheckResult.TimeExpired(state.GetWinningTeamByScore());
        
        if (state.GetMaxScore() >= state.Config.TargetScore)
            return WinCheckResult.TargetReached(state.GetWinningTeamByScore());
        
        return WinCheckResult.Continue();
    }
}
```

**Match Phase State Machine (Host-Authoritative):**
```csharp
public enum MatchPhase
{
    Lobby,      // Players joining, bots filling
    Countdown,  // 3-2-1 GO
    Active,     // Gameplay running
    Ended,      // Win condition triggered
    Results     // Final scores, rewards
}

public class MatchStateMachine : NetworkBehaviour
{
    private NetworkVariable<MatchPhase> currentPhase = new(MatchPhase.Lobby);
    
    public void TransitionTo(MatchPhase newPhase)
    {
        if (!IsHost) return;
        
        ExitPhase(currentPhase.Value);
        currentPhase.Value = newPhase;
        EnterPhase(newPhase);
        
        BroadcastPhaseChangeClientRpc(newPhase);
    }
}
```

**Data-Driven Scoring System:**
```csharp
[CreateAssetMenu(fileName = "GameModeConfig", menuName = "RecipeRage/Game Mode Config")]
public class GameModeConfig : ScriptableObject
{
    public string ModeName;
    public WinConditionType WinCondition;
    public float MatchDuration;
    public int TargetScore;
    
    public ScoringRuleSet ScoringRules;
    public BotDifficulty DefaultBotDifficulty;
}
```

**Game Mode Manager:**
```csharp
public class GameModeManager : NetworkBehaviour
{
    private IWinConditionStrategy winConditionStrategy;
    
    public void InitializeMode(GameModeType modeType)
    {
        var config = GetConfig(modeType);
        winConditionStrategy = WinConditionFactory.Create(config.WinCondition);
        winConditionStrategy.Initialize(config);
    }
    
    private void Update()
    {
        if (!IsHost || currentPhase.Value != MatchPhase.Active) return;
        
        var result = winConditionStrategy.CheckWinCondition(matchState);
        if (result.IsComplete)
        {
            stateMachine.TransitionTo(MatchPhase.Ended);
            DeclareWinnerClientRpc(result.WinningTeam);
        }
    }
}
```

---

### Decision 5: State Synchronization Strategy - LAYERED RELIABILITY

**Status:** ✅ DECIDED

**Decision:** Layered state sync with appropriate reliability per category.

**State Categories:**

**A. Critical State (Reliable Sync)**
- Match phase, scores, order expiration
- NetworkVariables with reliable delivery
- Immediate reconciliation on mismatch

**B. Gameplay State (30Hz Sync)**
- Player positions, station states, ability effects
- NGO's built-in NetworkTransform + NetworkVariables
- Client prediction + server reconciliation

**C. Cosmetic State (Unreliable Sync)**
- Animations, particle effects, UI feedback
- Fire-and-forget ClientRpc
- Visual-only, doesn't affect gameplay

**Player Progression Sync:**
```csharp
public class PlayerProgression : NetworkBehaviour
{
    private NetworkVariable<int> experience = new(0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    [ServerRpc]
    public void AwardMatchRewardsServerRpc(int earnedXP, int earnedCoins)
    {
        // Host validates and applies
        experience.Value += earnedXP;
        
        // Sync to cloud immediately
        CloudSyncService.SyncPlayerData(OwnerClientId, experience.Value);
    }
}
```

**Economy State Consistency:**
- **Client Authority (Immediate):** Visual coin updates, UI feedback
- **Host Validation (Authoritative):** Purchase validation, unlock checks
- **Cloud Persistence (Backup):** Player progression survives reinstalls

---

### Decision 6: Testing & Validation Infrastructure

**Status:** ✅ DECIDED

**Decision:** Unity Test Framework + Network Scenario Tests + Automated Balance Validation

**A. Unity Test Framework Setup**
```csharp
// Bot AI Behavior Tests
[Test]
public void Bot_Prioritizes_Urgent_Orders()
{
    var bot = new BotDecisionEngine();
    var orders = new[]
    {
        new Order(OrderType.Simple, urgency: 0.3f),
        new Order(OrderType.Complex, urgency: 0.9f)
    };
    
    var decision = bot.SelectTargetOrder(orders);
    Assert.AreEqual(OrderType.Complex, decision.Type);
}
```

**B. Network Scenario Tests**
```csharp
// Host Migration Test
[Test]
public void HostMigration_Preserves_MatchState()
{
    var match = CreateMatchWithBots();
    var originalScore = match.GetScore(Team.Red);
    
    match.SimulateHostDisconnect();
    
    Assert.IsTrue(match.HasNewHost());
    Assert.AreEqual(originalScore, match.GetScore(Team.Red));
}
```

**C. Ability Balance Validation**
```csharp
// Automated balance check
[Test]
public void SpeedBoost_Not_Overpowered()
{
    var stats = SimulateMatches(1000, ability: AbilityType.SpeedBoost);
    
    Assert.IsTrue(stats.WinRate > 0.45f && stats.WinRate < 0.55f);
    Assert.IsTrue(stats.AvgAbilityUsagePerMatch > 2f);
}
```

---

### Decision Impact Analysis

**Implementation Sequence:**

1. **Week 1-2:** Bot AI Architecture (biggest blocker)
   - Implement BotDecisionEngine
   - Integrate with NGO networking
   - Add perception and action systems

2. **Week 1-2:** Game Mode Framework (parallel)
   - Implement win condition strategies
   - Fix ClassicGameModeLogic
   - Add match state machine

3. **Week 3-4:** Ability System Framework
   - Implement 5 MVP abilities
   - Add server-authoritative validation
   - Balance testing

4. **Week 5-6:** Polish & Testing Infrastructure
   - Host migration testing
   - Balance validation
   - Performance optimization

**Cross-Component Dependencies:**

```
Bot AI → Networking Layer (runs on host)
Ability System → Networking Layer (validation)
Game Mode → Bot AI (bot filling)
Game Mode → Ability System (PvP modes)
Progression → Networking (state sync)
All Systems → Host Authority (validation)
```

---

### Architectural Decisions Summary Table

| Decision | Pattern | Impact | Status |
|----------|---------|--------|--------|
| **Bot AI Architecture** | Host-Only AI | MVP Blocker | ✅ Decided |
| **P2P Networking** | NGO Host Authority | Core Differentiator | ✅ Decided |
| **Ability System** | Host-Authoritative | Competitive Integrity | ✅ Decided |
| **Game Mode Framework** | Strategy + State Machine | Extensibility | ✅ Decided |
| **State Sync** | Layered Reliability | Performance | ✅ Decided |
| **Testing** | UTF + Scenarios | Quality Assurance | ✅ Decided |

*Core architectural decisions documented. Ready for implementation patterns.*

---

## Implementation Patterns & Consistency Rules

### Pattern Categories Defined

**Critical Conflict Points Identified:** 7 areas where AI agents could make different choices

---

### Pattern 1: Naming Conventions (C# / Unity)

**Files:** PascalCase + descriptive suffix
- ✅ `BotDecisionEngine.cs`
- ✅ `AbilitySystemManager.cs`
- ❌ `botAI.cs`, `ability_manager.cs`

**Classes:** PascalCase, descriptive suffixes
- ✅ `BotController`, `OrderManager`, `MatchStateMachine`
- ❌ `Bot`, `Orders`, `Match`

**Methods:** PascalCase (C# standard)
- ✅ `ActivateAbility()`, `CheckWinCondition()`
- ❌ `activateAbility()`, `check_win_condition()`

**Variables:** camelCase
- ✅ `botCount`, `matchDuration`, `isHost`
- ❌ `BotCount`, `match_duration`

**Constants:** UPPER_SNAKE_CASE
- ✅ `MAX_BOT_COUNT`, `DEFAULT_MATCH_DURATION`
- ❌ `MaxBotCount`, `defaultMatchDuration`

**Network RPCs:** Verb + Action + Rpc suffix
- ✅ `ActivateAbilityServerRpc()`, `BroadcastBotActionsClientRpc()`
- ❌ `AbilityActivated()`, `BotActionsBroadcast()`

**NetworkVariables:** network + PascalCase
- ✅ `networkMatchTimer`, `networkTeamScore`
- ❌ `matchTimer`, `NetworkMatchTimer`

---

### Pattern 2: NGO Networking Patterns

**Server-Authoritative Actions:**
```csharp
// ALWAYS use ServerRpc for gameplay-affecting actions
[ServerRpc(RequireOwnership = false)]
public void ActivateAbilityServerRpc(AbilityType type, Vector3 target)
{
    // Host validates and executes
    if (!CanActivate(type)) return;
    ExecuteAbility(type, target);
    NotifyClientsClientRpc(type, target);
}

// Clients receive via ClientRpc
[ClientRpc]
void NotifyClientsClientRpc(AbilityType type, Vector3 target)
{
    // Visual feedback only on clients
    PlayAbilityVFX(type, target);
}
```

**NetworkVariables for State:**
```csharp
// Use NetworkVariables for automatically-synced state
private NetworkVariable<int> matchScore = new NetworkVariable<int>(0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server); // Only host writes
```

**NEVER direct client authority for gameplay state:**
- ❌ Client changing score directly
- ❌ Client moving other players
- ❌ Client validating its own ability usage

---

### Pattern 3: Bot AI Implementation Pattern

**Architecture:**
```csharp
// All Bot AI runs on host ONLY
public class BotController : NetworkBehaviour
{
    private BotDecisionEngine decisionEngine; // Host-only instance
    
    private void Update()
    {
        if (!IsHost) return; // CRITICAL: Only host runs AI
        
        // 10Hz decision tick (performance optimization)
        if (Time.time >= nextDecisionTime)
        {
            var action = decisionEngine.DecideAction();
            ExecuteBotAction(action);
            nextDecisionTime = Time.time + 0.1f;
        }
    }
    
    [ClientRpc]
    void BroadcastBotActionClientRpc(BotActionData action)
    {
        // Clients visualize, don't execute logic
        VisualizeBotAction(action);
    }
}
```

**Decision Engine Pattern:**
```csharp
public class BotDecisionEngine
{
    // Perception → Decision → Action pipeline
    public BotAction DecideAction()
    {
        var perception = GatherPerception();    // What's visible?
        var goal = SelectGoal(perception);      // What to do?
        var action = PlanAction(goal);          // How to do it?
        return action;
    }
}
```

**Bot State Machine:**
```csharp
public enum BotState
{
    IDLE,        // Waiting for orders
    GATHERING,   // Getting ingredients
    PREPARING,   // Cutting/cooking
    ASSEMBLING,  // Putting on plates
    SERVING      // Delivering orders
}
```

---

### Pattern 4: Ability System Pattern

**All abilities implement IAbility interface:**
```csharp
public interface IAbility
{
    AbilityType Type { get; }
    float Cooldown { get; }
    float Duration { get; }
    
    bool CanActivate(PlayerController player);
    void OnActivated(PlayerController player);
    void OnEffectStart(PlayerController player);
    void OnEffectEnd(PlayerController player);
}

// Concrete implementation
public class SpeedBoostAbility : IAbility
{
    public AbilityType Type => AbilityType.SpeedBoost;
    public float Cooldown => 20f;
    public float Duration => 5f;
    
    public void OnActivated(PlayerController player)
    {
        player.ApplySpeedMultiplier(2f);
    }
    
    public void OnEffectEnd(PlayerController player)
    {
        player.ResetSpeed();
    }
}
```

**Ability Manager Pattern:**
```csharp
public class AbilitySystem : NetworkBehaviour
{
    private Dictionary<AbilityType, IAbility> abilities = new();
    private Dictionary<AbilityType, float> cooldowns = new();
    
    [ServerRpc]
    public void RequestAbilityActivationServerRpc(AbilityType type)
    {
        if (!IsHost) return;
        
        var ability = abilities[type];
        if (cooldowns[type] <= 0 && ability.CanActivate(player))
        {
            ability.OnActivated(player);
            cooldowns[type] = ability.Cooldown;
            BroadcastAbilityActivationClientRpc(type);
        }
    }
}
```

---

### Pattern 5: Error Handling Patterns

**Network Errors:**
```csharp
// Graceful degradation, never crash
public async void FindMatch()
{
    try
    {
        var result = await EOS.Matchmaking.FindMatchAsync();
        StartMatch(result);
    }
    catch (EOSException ex)
    {
        Debug.LogError($"[Networking] Matchmaking failed: {ex.Message}");
        // Fallback: Create bot-only match
        CreateBotMatch();
    }
}
```

**Bot AI Errors:**
```csharp
// Bots should never break the match
public void ExecuteBotAction(BotAction action)
{
    try
    {
        action.Execute();
    }
    catch (Exception ex)
    {
        Debug.LogError($"[BotAI] Action failed for bot {botId}: {ex.Message}");
        // Fail-safe: Return to IDLE state
        botState.TransitionTo(BotState.IDLE);
    }
}
```

**Validation Errors:**
```csharp
[ServerRpc]
void ActivateAbilityServerRpc(AbilityType type)
{
    if (!CanActivate(type))
    {
        Debug.LogWarning($"[Validation] Ability {type} rejected for player {OwnerClientId}");
        return; // Silently reject, client will predict anyway
    }
    // ... proceed with activation
}
```

---

### Pattern 6: Project Structure

**Folder Organization:**
```
Assets/
├── Scripts/
│   ├── Core/              # Singletons, managers (don't touch without discussion)
│   │   ├── GameManager.cs
│   │   ├── NetworkManager.cs
│   │   └── GameModeManager.cs
│   ├── Networking/        # NGO wrappers, RPCs
│   │   ├── NetworkPlayer.cs
│   │   ├── NetworkBot.cs
│   │   └── NetworkStations.cs
│   ├── BotAI/            # All AI-related code
│   │   ├── BotController.cs
│   │   ├── BotDecisionEngine.cs
│   │   ├── BotPerception.cs
│   │   └── Actions/
│   │       ├── GatherIngredientsAction.cs
│   │       ├── ProcessAtStationAction.cs
│   │       └── ServeOrderAction.cs
│   ├── Abilities/          # Ability system
│   │   ├── AbilitySystem.cs
│   │   ├── IAbility.cs
│   │   └── Implementations/
│   │       ├── SpeedBoostAbility.cs
│   │       ├── PushAbility.cs
│   │       └── MagnetAbility.cs
│   ├── GameModes/        # Win conditions, scoring
│   │   ├── MatchStateMachine.cs
│   │   ├── WinConditions/
│   │   │   ├── IWinConditionStrategy.cs
│   │   │   ├── ScoreBasedWinCondition.cs
│   │   │   └── OrderCountWinCondition.cs
│   │   └── Scoring/
│   ├── Stations/          # Kitchen stations
│   ├── Orders/            # Order management
│   └── UI/               # UI controllers
├── Prefabs/
├── Scenes/
├── Resources/
│   └── GameModeConfigs/   # ScriptableObject configs
│       ├── ClassicMode.asset
│       └── TimeAttack.asset
└── Tests/                 # Unity Test Framework
    ├── BotAITests/
    ├── NetworkingTests/
    └── AbilityTests/
```

---

### Pattern 7: State Synchronization Format

**NetworkVariable Naming:**
```csharp
// Match state
private NetworkVariable<float> networkMatchTimer;
private NetworkVariable<int> networkTeamRedScore;
private NetworkVariable<int> networkTeamBlueScore;
private NetworkVariable<MatchPhase> networkCurrentPhase;

// Player state
private NetworkVariable<Vector3> networkPlayerPosition;
private NetworkVariable<bool> networkIsCarryingItem;
private NetworkVariable<int> networkHeldItemId;
```

**RPC Payloads:**
```csharp
// Keep payloads minimal for performance
public struct AbilityActivationData : INetworkSerializable
{
    public AbilityType AbilityType;
    public Vector3 TargetPosition;
    public float ActivationTime; // For replay/sync verification
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref AbilityType);
        serializer.SerializeValue(ref TargetPosition);
        serializer.SerializeValue(ref ActivationTime);
    }
}
```

**Event Naming:**
- ✅ `OnOrderCompleted`, `OnMatchEnded`, `OnPlayerDisconnected`
- ❌ `OrderComplete`, `match_end`, `playerDisconnect`

---

### Enforcement Guidelines - ALL AI Agents MUST:

1. **Follow C# naming conventions** exactly as specified
2. **Use ServerRpc/ClientRpc patterns** for all networked gameplay
3. **Never run Bot AI on clients** - host-only execution with `if (!IsHost) return;`
4. **Implement abilities via IAbility interface** for consistency
5. **Use graceful error handling** - never crash the match
6. **Organize code in correct folders** per component type
7. **Minimize NetworkVariable churn** - batch updates when possible
8. **Document deviations** with comments explaining why pattern was broken

---

### Pattern Examples

**Good Example - Bot AI:**
```csharp
public class BotController : NetworkBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private BotDifficulty difficulty = BotDifficulty.Normal;
    
    private BotDecisionEngine decisionEngine;
    private NetworkVariable<BotState> networkBotState = new(BotState.IDLE);
    
    private void Update()
    {
        if (!IsHost) return; // Critical: Host-only AI
        
        if (Time.time >= nextDecisionTime)
        {
            var action = decisionEngine.DecideAction();
            ExecuteAction(action);
            nextDecisionTime = Time.time + 0.1f;
        }
    }
}
```

**Anti-Pattern - Avoid:**
```csharp
// ❌ DON'T: Client running AI logic
public class BotController : NetworkBehaviour
{
    private void Update()
    {
        // No host check! AI runs on all clients - DESYNC!
        var action = DecideAction();
        ExecuteAction(action);
    }
}
```

**Good Example - Ability Implementation:**
```csharp
public class PushAbility : IAbility
{
    public AbilityType Type => AbilityType.Push;
    public float Cooldown => 15f;
    
    public void OnActivated(PlayerController player)
    {
        // Apply push force to nearby players
        var nearbyPlayers = GetPlayersInRange(player.Position, 3f);
        foreach (var target in nearbyPlayers)
        {
            if (target != player)
            {
                target.ApplyPushForce(player.Forward * 10f);
            }
        }
    }
}
```

**Anti-Pattern - Avoid:**
```csharp
// ❌ DON'T: Client validating its own ability
public void ActivateAbility(AbilityType type)
{
    if (cooldowns[type] <= 0) // Client check - can be hacked!
    {
        // Execute immediately - no server validation
        ExecuteAbility(type);
    }
}
```

---

## Existing Codebase Integration Strategy (CRITICAL)

### Philosophy: Extract Domain, Keep Gameplay

**RecipeRage has a 55% production-ready codebase. DO NOT start from scratch. Instead: Extract pure C# logic into Domain layer, keep Unity-specific code in Gameplay layer.**

### Current State Analysis

**Working Systems (KEEP & EXTEND):**
| System | Location | Completion | Action |
|--------|----------|------------|--------|
| **PlayerController** | `Gameplay/Characters/` | 90% ✅ | Extend for abilities |
| **StationSystem** | `Gameplay/Stations/` | 85% ✅ | Add Bot AI interaction |
| **OrderManager** | `Gameplay/Cooking/` | 80% ✅ | Bot AI uses this |
| **Matchmaking** | `Core/Networking/` | 85% ✅ | Add bot filling |
| **SpeedBoost Ability** | `Gameplay/Characters/` | 100% ✅ | Template for 4 more |

**Gap Systems (EXTRACT DOMAIN + COMPLETE):**
| System | Location | Completion | Action |
|--------|----------|------------|--------|
| **Bot AI** | `Gameplay/Networking/Bot/BotController.cs` | 20% ⬜ | Extract to Domain, complete |
| **Win Conditions** | `Gameplay/GameModes/Logic/ClassicGameModeLogic.cs` | 50% ⬜ | Fix commented logic |
| **4 Abilities** | `Gameplay/Characters/` | 15% ⬜ | Add Push, Magnet, InstantCook, FreezeTime |

### Refactoring Strategy: "Evolution, Not Revolution"

**DON'T:**
- ❌ Move all files around
- ❌ Rename existing working classes
- ❌ Break 55% that's working
- ❌ Refactor everything at once

**DO:**
- ✅ Create `Domain/` folder alongside existing structure
- ✅ Extract pure logic from Gameplay into Domain
- ✅ Keep Unity-specific code in Gameplay as "adapters"
- ✅ Gradual refactoring while implementing MVP
- ✅ New features: Domain first, then Gameplay wrapper

---

### Step-by-Step Refactoring Plan

#### Phase 1: Create Domain Folder Structure (Week 1)

**NEW folders to create:**
```
Assets/Scripts/
├── Domain/                          # NEW - Pure C# (no Unity!)
│   ├── BotAI/
│   ├── Abilities/
│   ├── GameLogic/
│   └── Common/
│
├── Core/                           # KEEP - Infrastructure
│   ├── UI/
│   ├── Auth/
│   └── Input/
│
├── Gameplay/                       # KEEP - Refactor to use Domain
│   ├── Characters/
│   ├── BotAI/
│   ├── GameModes/
│   └── ...
```

---

#### Phase 2: Refactor Bot AI (Week 1-2 - MVP Critical)

**CURRENT (Gameplay/Networking/Bot/BotController.cs):**
```csharp
// Has good foundation but needs extraction
public class BotController : NetworkBehaviour
{
    private void Update()
    {
        if (!IsServer) return;  // ✅ Good: Host-only
        
        // Simple random movement - needs real AI
        if (Time.time >= _nextActionTime)
        {
            _targetPosition = GetRandomPosition();  // ⬜ Replace with Domain logic
        }
    }
}
```

**REFACTOR TO:**

**A. Create Domain Layer (Pure C#):**
```csharp
// NEW: Domain/BotAI/BotDecisionEngine.cs
using System.Collections.Generic;

namespace RecipeRage.Domain.BotAI
{
    public class BotDecisionEngine
    {
        private readonly BotPerception _perception;
        private readonly BotGoalSelector _goalSelector;
        private readonly BotActionPlanner _actionPlanner;
        
        public BotDecisionEngine(BotDifficulty difficulty)
        {
            _perception = new BotPerception();
            _goalSelector = new BotGoalSelector(difficulty);
            _actionPlanner = new BotActionPlanner();
        }
        
        public BotAction DecideAction(BotContext context)
        {
            // 1. Perception - What do I know?
            var knowledge = _perception.GatherKnowledge(context);
            
            // 2. Decision - What should I do?
            var goal = _goalSelector.SelectGoal(knowledge);
            
            // 3. Action - How do I achieve it?
            var action = _actionPlanner.PlanAction(goal, knowledge);
            
            return action;
        }
    }
    
    public class BotAction
    {
        public BotActionType Type { get; set; }
        public int TargetStationId { get; set; }
        public int TargetOrderId { get; set; }
        public float Priority { get; set; }
    }
    
    public class BotContext
    {
        public Vector3Data BotPosition { get; set; }  // Pure data, not Unity Vector3
        public List<StationData> VisibleStations { get; set; }
        public List<OrderData> ActiveOrders { get; set; }
        public List<ItemData> CarriedItems { get; set; }
    }
}
```

**B. Refactor Gameplay Layer (Unity Adapter):**
```csharp
// REFACTOR: Gameplay/Networking/Bot/BotController.cs
// Keep file location, modify internals

using UnityEngine;
using Unity.Netcode;
using RecipeRage.Domain.BotAI;  // NEW: Use Domain

namespace Gameplay.Networking.Bot
{
    public class BotController : NetworkBehaviour
    {
        [Header("Bot Settings")]
        [SerializeField] private BotDifficulty difficulty;
        
        // NEW: Domain logic instance
        private BotDecisionEngine _decisionEngine;
        private BotContext _context;
        
        // Unity components
        private NavMeshAgent _navAgent;
        private PlayerController _playerController;
        
        public override void OnNetworkSpawn()
        {
            // Initialize Domain logic
            _decisionEngine = new BotDecisionEngine(difficulty);
            _context = new BotContext();
            
            // Initialize Unity components
            _navAgent = GetComponent<NavMeshAgent>();
            _playerController = GetComponent<PlayerController>();
        }
        
        private void Update()
        {
            // CRITICAL: Keep existing host-only pattern
            if (!IsServer) return;
            
            // 10Hz decision tick (performance)
            if (Time.time >= _nextDecisionTime)
            {
                // Update context from Unity world
                UpdateBotContext();
                
                // NEW: Delegate to pure Domain logic
                var action = _decisionEngine.DecideAction(_context);
                
                // Execute in Unity world
                ExecuteAction(action);
                
                // Broadcast to clients
                BroadcastBotActionClientRpc(action);
                
                _nextDecisionTime = Time.time + 0.1f;
            }
        }
        
        private void UpdateBotContext()
        {
            // Adapt Unity world to Domain model
            _context.BotPosition = transform.position.ToData();  // Convert to pure data
            _context.VisibleStations = GetVisibleStations();
            _context.ActiveOrders = GetActiveOrdersFromManager();
            _context.CarriedItems = _playerController.GetCarriedItemsData();
        }
        
        private void ExecuteAction(BotAction action)
        {
            // Unity-specific execution
            switch (action.Type)
            {
                case BotActionType.MoveToStation:
                    var station = GetStationById(action.TargetStationId);
                    _navAgent.SetDestination(station.Position);
                    _playerController.PlayAnimation("Walk");
                    break;
                    
                case BotActionType.PickUpIngredient:
                    station.Interact(_playerController);
                    _playerController.PlayAnimation("Interact");
                    break;
                    
                case BotActionType.PrepareOrder:
                    // Process at station
                    break;
                    
                case BotActionType.ServeOrder:
                    // Deliver to serving station
                    break;
            }
        }
        
        [ClientRpc]
        private void BroadcastBotActionClientRpc(BotActionData actionData)
        {
            // Clients visualize only (no logic)
            if (IsServer) return;
            
            VisualizeAction(actionData);
        }
    }
}
```

**Result:**
- ✅ Domain: Pure C# AI logic (testable without Unity)
- ✅ Gameplay: Unity adapter (navmesh, animations, networking)
- ✅ Existing BotController kept, just enhanced internally

---

#### Phase 3: Refactor Win Conditions (Week 2)

**CURRENT (Gameplay/GameModes/Logic/ClassicGameModeLogic.cs):**
```csharp
public void Update(float deltaTime)
{
    // Lines 32-35 are COMMENTED - needs fixing!
    // if (_scoreManager != null)
    // {
    //     _scoreManager.SetTargetScore(_config.TargetScore);
    // }
    
    if (_currentPhase == GamePhase.Playing && _scoreManager != null)
    {
        if (_config.HasScoreLimit && CheckScoreWinCondition())
        {
            OnMatchEnd();
        }
    }
}
```

**REFACTOR TO:**

**A. Create Domain Win Condition:**
```csharp
// NEW: Domain/GameLogic/WinConditions/ScoreBasedWinCondition.cs

namespace RecipeRage.Domain.GameLogic
{
    public interface IWinConditionStrategy
    {
        WinCheckResult CheckWinCondition(MatchStateData state);
        void Initialize(GameModeConfig config);
    }
    
    public class ScoreBasedWinCondition : IWinConditionStrategy
    {
        private GameModeConfig _config;
        
        public void Initialize(GameModeConfig config)
        {
            _config = config;
        }
        
        public WinCheckResult CheckWinCondition(MatchStateData state)
        {
            // Time expired - compare team scores
            if (state.RemainingTime <= 0)
            {
                var winningTeam = GetWinningTeamByScore(state);
                return WinCheckResult.TimeExpired(winningTeam);
            }
            
            // Target score reached
            if (_config.HasScoreLimit && state.MaxScore >= _config.TargetScore)
            {
                var winningTeam = GetWinningTeamByScore(state);
                return WinCheckResult.TargetReached(winningTeam);
            }
            
            return WinCheckResult.Continue();
        }
        
        private Team GetWinningTeamByScore(MatchStateData state)
        {
            if (state.RedScore > state.BlueScore) return Team.Red;
            if (state.BlueScore > state.RedScore) return Team.Blue;
            return Team.None; // Tie
        }
    }
    
    public class MatchStateData
    {
        public float RemainingTime { get; set; }
        public int RedScore { get; set; }
        public int BlueScore { get; set; }
        public int MaxScore => Mathf.Max(RedScore, BlueScore);
        public bool HasScoreLimit { get; set; }
        public int TargetScore { get; set; }
    }
}
```

**B. Refactor Gameplay to Use Domain:**
```csharp
// REFACTOR: Gameplay/GameModes/Logic/ClassicGameModeLogic.cs

using RecipeRage.Domain.GameLogic;  // NEW

public class ClassicGameModeLogic : IGameModeLogic
{
    // NEW: Domain win condition
    private IWinConditionStrategy _winCondition;
    
    public void Initialize(GameMode config, OrderManager orderManager, ScoreManager scoreManager)
    {
        _config = config;
        _orderManager = orderManager;
        _scoreManager = scoreManager;
        
        // NEW: Create Domain win condition
        _winCondition = new ScoreBasedWinCondition();
        _winCondition.Initialize(config);
        
        // FIX: Uncomment and use config target score
        if (_scoreManager != null && config.HasScoreLimit)
        {
            _scoreManager.SetTargetScore(config.TargetScore);
        }
    }
    
    public void Update(float deltaTime)
    {
        // Build state data from Unity components
        var stateData = new MatchStateData
        {
            RemainingTime = TimeRemaining,
            RedScore = _scoreManager.GetScore(Team.Red),
            BlueScore = _scoreManager.GetScore(Team.Blue),
            HasScoreLimit = _config.HasScoreLimit,
            TargetScore = _config.TargetScore
        };
        
        // NEW: Delegate to Domain logic
        var result = _winCondition.CheckWinCondition(stateData);
        
        if (result.IsComplete)
        {
            GameLogger.Log($"Win condition met: {result.Reason}");
            OnMatchEnd(result.WinningTeam);
        }
    }
}
```

---

#### Phase 4: Refactor Abilities (Week 3-4)

**CURRENT (Gameplay/Characters/):**
```csharp
// CharacterAbility.cs - Has SpeedBoost, needs 4 more
```

**REFACTOR TO:**

**A. Create Domain Abilities:**
```csharp
// NEW: Domain/Abilities/IAbilityLogic.cs

namespace RecipeRage.Domain.Abilities
{
    public interface IAbilityLogic
    {
        AbilityType Type { get; }
        float Cooldown { get; }
        float Duration { get; }
        
        bool CanActivate(PlayerStatsData stats);
        void ApplyEffect(PlayerStatsData stats);
        void RemoveEffect(PlayerStatsData stats);
    }
    
    // NEW: Domain/Abilities/SpeedBoostLogic.cs
    public class SpeedBoostLogic : IAbilityLogic
    {
        public AbilityType Type => AbilityType.SpeedBoost;
        public float Cooldown => 20f;
        public float Duration => 5f;
        
        public void ApplyEffect(PlayerStatsData stats)
        {
            stats.SpeedMultiplier = 2f;  // Pure math
        }
        
        public void RemoveEffect(PlayerStatsData stats)
        {
            stats.SpeedMultiplier = 1f;
        }
    }
    
    // NEW: Domain/Abilities/PushLogic.cs
    public class PushLogic : IAbilityLogic
    {
        public AbilityType Type => AbilityType.Push;
        public float Cooldown => 15f;
        public float Duration => 0f;  // Instant
        
        public void ApplyEffect(PlayerStatsData stats)
        {
            var nearbyPlayers = stats.GetPlayersInRadius(3f);
            foreach (var target in nearbyPlayers)
            {
                if (target.Team != stats.Team)
                {
                    var pushDir = (target.Position - stats.Position).normalized;
                    target.ApplyPushForce(pushDir * 10f);
                }
            }
        }
    }
    
    // NEW: Domain/Abilities/MagnetLogic.cs
    public class MagnetLogic : IAbilityLogic
    {
        public AbilityType Type => AbilityType.Magnet;
        public float Cooldown => 12f;
        public float Duration => 8f;
        
        public void ApplyEffect(PlayerStatsData stats)
        {
            stats.HasMagnetEffect = true;
            stats.MagnetRadius = 5f;
        }
        
        public void RemoveEffect(PlayerStatsData stats)
        {
            stats.HasMagnetEffect = false;
        }
    }
}
```

**B. Refactor Gameplay Ability System:**
```csharp
// REFACTOR: Gameplay/Characters/AbilitySystem.cs

using RecipeRage.Domain.Abilities;  // NEW

public class AbilitySystem : NetworkBehaviour
{
    // NEW: Dictionary of Domain abilities
    private Dictionary<AbilityType, IAbilityLogic> _abilityLogics;
    private Dictionary<AbilityType, float> _cooldowns;
    
    private void Awake()
    {
        // Initialize Domain abilities
        _abilityLogics = new Dictionary<AbilityType, IAbilityLogic>
        {
            { AbilityType.SpeedBoost, new SpeedBoostLogic() },  // ✅ Keep existing
            { AbilityType.Push, new PushLogic() },              // ⬜ NEW
            { AbilityType.Magnet, new MagnetLogic() },        // ⬜ NEW
            { AbilityType.InstantCook, new InstantCookLogic() }, // ⬜ NEW
            { AbilityType.FreezeTime, new FreezeTimeLogic() }  // ⬜ NEW
        };
        
        _cooldowns = new Dictionary<AbilityType, float>();
    }
    
    [ServerRpc]
    public void ActivateAbilityServerRpc(AbilityType type)
    {
        if (!IsServer) return;
        
        var ability = _abilityLogics[type];
        var playerStats = GetComponent<PlayerStats>();
        
        // NEW: Delegate to Domain for validation
        if (_cooldowns[type] <= 0 && ability.CanActivate(playerStats.ToData()))
        {
            // Apply Domain logic
            ability.ApplyEffect(playerStats.ToData());
            _cooldowns[type] = ability.Cooldown;
            
            // Unity-specific effects
            PlayAbilityVFX(type);
            PlayAbilitySFX(type);
            
            // Schedule removal if duration > 0
            if (ability.Duration > 0)
            {
                StartCoroutine(RemoveEffectAfter(ability.Duration, type));
            }
            
            // Broadcast to clients
            BroadcastAbilityActivationClientRpc(type);
        }
    }
    
    private IEnumerator RemoveEffectAfter(float duration, AbilityType type)
    {
        yield return new WaitForSeconds(duration);
        
        var ability = _abilityLogics[type];
        var playerStats = GetComponent<PlayerStats>();
        
        ability.RemoveEffect(playerStats.ToData());
    }
}
```

---

### Benefits of This Approach

**For RecipeRage:**
1. **Testability** - Can test Bot AI without Unity
2. **Balance Tuning** - Change ability numbers in pure C#
3. **Future Ports** - Same logic works in Unreal/Godot
4. **Dedicated Servers** - Domain works on backend
5. **MVP Timeline** - Refactor as you implement, no delay

**For AI Agents:**
1. **Clear Boundaries** - Domain = algorithms, Gameplay = Unity
2. **Existing Codebase** - Don't break 55% that's working
3. **Gradual Migration** - Refactor incrementally
4. **Testable Logic** - Unit test 70% of code

---

### AI Agent Implementation Rules for Refactoring

**When working on ANY feature:**

**1. Check existing code first**
```bash
# Search for similar patterns
grep -r "class.*Controller" Assets/Scripts/Gameplay/
grep -r "IsServer" Assets/Scripts/Gameplay/
```

**2. Ask: "Should this be Domain or Gameplay?"**
- Math, decisions, rules → Domain (Pure C#)
- Network, render, input → Gameplay (Unity)

**3. Pattern for new features:**
```
1. Create Domain logic first (interface + implementation)
2. Test Domain logic without Unity (unit tests)
3. Create/extend Gameplay wrapper (MonoBehaviour)
4. Gameplay calls Domain, applies Unity-specifics
5. Integration test with Unity
```

**4. Pattern for refactoring existing:**
```
1. Keep existing file location
2. Extract pure logic to Domain
3. Existing file becomes thin wrapper
4. Keep all Unity-specific code in place
5. Map between Domain models and Unity components
```

---

### Testing Strategy

**Domain Tests (No Unity - Fast):**
```csharp
[Test]
public void BotDecisionEngine_Prioritizes_Urgent_Orders()
{
    var engine = new BotDecisionEngine(BotDifficulty.Normal);
    var context = new BotContext
    {
        ActiveOrders = new[]
        {
            new OrderData { Type = OrderType.Simple, Urgency = 0.3f },
            new OrderData { Type = OrderType.Complex, Urgency = 0.9f }
        }
    };
    
    var action = engine.DecideAction(context);
    
    Assert.AreEqual(BotActionType.PrepareOrder, action.Type);
    Assert.AreEqual(OrderType.Complex, action.TargetOrderType);
}

[Test]
public void SpeedBoostLogic_Doubles_Speed()
{
    var logic = new SpeedBoostLogic();
    var stats = new PlayerStatsData { SpeedMultiplier = 1f };
    
    logic.ApplyEffect(stats);
    
    Assert.AreEqual(2f, stats.SpeedMultiplier);
}
```

**Integration Tests (With Unity):**
```csharp
[Test]
public void BotController_Moves_To_Station()
{
    var bot = CreateBotWithNavMesh();
    var station = CreateStationAt(Vector3.forward * 10);
    
    bot.ExecuteAction(new BotAction
    {
        Type = BotActionType.MoveToStation,
        TargetStationId = station.Id
    });
    
    Assert.IsTrue(bot.IsMoving);
    Assert.AreEqual(station.Position, bot.NavAgent.destination);
}
```

---

## Project Structure & Boundaries

### Complete Unity Project Directory Structure

```
RecipeRage/
├── .github/
│   └── workflows/
│       ├── build-android.yml          # Android CI/CD
│       ├── build-ios.yml              # iOS CI/CD
│       └── run-tests.yml              # Automated testing
│
├── Assets/
│   ├── Scripts/                       # All C# source code
│   │   ├── Core/                      # Critical singletons
│   │   │   ├── GameManager.cs
│   │   │   ├── NetworkManager.cs
│   │   │   ├── GameModeManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   └── ProgressionManager.cs
│   │   │
│   │   ├── Networking/                # NGO wrappers
│   │   │   ├── NetworkPlayer.cs
│   │   │   ├── NetworkBot.cs
│   │   │   ├── NetworkStation.cs
│   │   │   ├── NetworkOrder.cs
│   │   │   ├── NetworkMatch.cs
│   │   │   ├── HostSelector.cs
│   │   │   ├── HostMigration.cs
│   │   │   ├── ServerValidation.cs
│   │   │   └── RpcPayloads/
│   │   │       ├── AbilityActivationData.cs
│   │   │       ├── BotActionData.cs
│   │   │       └── MatchStateData.cs
│   │   │
│   │   ├── BotAI/                     # Bot AI system (MVP CRITICAL)
│   │   │   ├── BotController.cs
│   │   │   ├── BotDecisionEngine.cs
│   │   │   ├── BotPerception.cs
│   │   │   ├── BotMemory.cs
│   │   │   ├── BotStateMachine.cs
│   │   │   ├── BotPathfinding.cs
│   │   │   ├── BotActionExecutor.cs
│   │   │   ├── BotDifficulty.cs
│   │   │   └── Actions/
│   │   │       ├── GatherIngredientsAction.cs
│   │   │       ├── ProcessAtStationAction.cs
│   │   │       ├── AssembleOrderAction.cs
│   │   │       └── ServeOrderAction.cs
│   │   │
│   │   ├── Abilities/                 # Ability system
│   │   │   ├── AbilitySystem.cs
│   │   │   ├── IAbility.cs
│   │   │   ├── AbilityCooldown.cs
│   │   │   ├── AbilityEffects/
│   │   │   └── Implementations/
│   │   │       ├── SpeedBoostAbility.cs    ✅
│   │   │       ├── PushAbility.cs          ⬜ MVP
│   │   │       ├── MagnetAbility.cs        ⬜ MVP
│   │   │       ├── InstantCookAbility.cs   ⬜ MVP
│   │   │       └── FreezeTimeAbility.cs    ⬜ MVP
│   │   │
│   │   ├── GameModes/                 # Win conditions
│   │   │   ├── MatchStateMachine.cs
│   │   │   ├── IWinConditionStrategy.cs
│   │   │   ├── WinConditions/
│   │   │   │   ├── ScoreBasedWinCondition.cs      ⬜ MVP
│   │   │   │   ├── OrderCountWinCondition.cs      ⬜ v1.3
│   │   │   │   └── TargetScoreWinCondition.cs     ⬜ v1.3
│   │   │   ├── Scoring/
│   │   │   │   ├── ScoreManager.cs
│   │   │   │   ├── ComboSystem.cs
│   │   │   │   └── QualityBonus.cs
│   │   │   └── Configs/
│   │   │       └── ClassicMode.asset
│   │   │
│   │   ├── Stations/                  # Kitchen stations
│   │   │   ├── StationController.cs
│   │   │   ├── IngredientCrate.cs
│   │   │   ├── CuttingStation.cs
│   │   │   ├── CookingStation.cs
│   │   │   ├── AssemblyStation.cs
│   │   │   └── ServingStation.cs
│   │   │
│   │   ├── Orders/                    # Order management
│   │   │   ├── OrderManager.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderGenerator.cs
│   │   │   ├── OrderTimer.cs
│   │   │   └── OrderPool.cs
│   │   │
│   │   ├── Characters/                # Player & characters
│   │   │   ├── PlayerController.cs    ✅
│   │   │   ├── PlayerMovement.cs
│   │   │   ├── PlayerInventory.cs
│   │   │   ├── CharacterData.cs
│   │   │   └── CharacterDatabase.cs
│   │   │
│   │   ├── Matchmaking/               # Lobby & matchmaking
│   │   │   ├── MatchmakingService.cs
│   │   │   ├── LobbyManager.cs
│   │   │   ├── BotFiller.cs
│   │   │   ├── QuickPlay.cs
│   │   │   └── InviteSystem.cs
│   │   │
│   │   ├── Social/                    # Friends & chat
│   │   │   ├── FriendsManager.cs
│   │   │   ├── OnlineStatus.cs
│   │   │   ├── ChatSystem.cs
│   │   │   └── BlockList.cs
│   │   │
│   │   ├── UI/                        # User interface
│   │   │   ├── MainMenu/
│   │   │   ├── Lobby/
│   │   │   ├── HUD/
│   │   │   ├── MatchResults/
│   │   │   ├── Shop/
│   │   │   ├── Settings/
│   │   │   └── Friends/
│   │   │
│   │   ├── Economy/                   # Coins, XP
│   │   │   ├── CoinManager.cs
│   │   │   ├── XPManager.cs
│   │   │   ├── UnlockSystem.cs
│   │   │   ├── ShopManager.cs
│   │   │   ├── SeasonPass.cs
│   │   │   └── DailyRewards.cs
│   │   │
│   │   ├── Authentication/            # Login
│   │   │   ├── AuthManager.cs
│   │   │   ├── GuestAccount.cs
│   │   │   ├── AccountLinker.cs
│   │   │   └── ProfileManager.cs
│   │   │
│   │   └── Utils/                     # Utilities
│   │       ├── ObjectPool.cs
│   │       ├── Logger.cs
│   │       └── Constants.cs
│   │
│   ├── Resources/
│   │   └── GameModeConfigs/
│   │       ├── ClassicMode.asset
│   │       ├── TimeAttack.asset
│   │       └── TeamBattle.asset
│   │
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Audio/
│   ├── Sprites/
│   ├── Materials/
│   └── Plugins/
│       ├── EOS/
│       ├── Firebase/
│       └── VContainer/
│
├── Tests/
│   ├── BotAITests/
│   ├── NetworkingTests/
│   ├── AbilityTests/
│   ├── GameModeTests/
│   └── TestUtilities/
│
├── Packages/
├── ProjectSettings/
├── docs/
├── .gitignore
└── README.md
```

### Team-Based Game Mode Support

**Architecture supports:**
- **2-team modes** (Classic, Team Battle) - Red vs Blue
- **Balanced bot filling** per team (2-4 players per team)
- **Team-based scoring** with multiple win conditions
- **PvP mechanics** between teams (sabotage, stealing)

**Team Data Structure:**
```csharp
public enum Team { None, Red, Blue }

public class MatchState
{
    private NetworkVariable<int> networkTeamRedScore = new(0);
    private NetworkVariable<int> networkTeamBlueScore = new(0);
    
    public int GetScore(Team team) => team switch {
        Team.Red => networkTeamRedScore.Value,
        Team.Blue => networkTeamBlueScore.Value,
        _ => 0
    };
}
```

### Requirements to Structure Mapping

| FR Category | Primary Directory | Key Files |
|-------------|-------------------|-----------|
| Matchmaking | `Scripts/Matchmaking/` | MatchmakingService.cs, BotFiller.cs |
| Abilities | `Scripts/Abilities/` | IAbility.cs, *Ability.cs |
| Bot AI | `Scripts/BotAI/` | BotController.cs, BotDecisionEngine.cs |
| Game Modes | `Scripts/GameModes/` | MatchStateMachine.cs, *WinCondition.cs |
| Orders | `Scripts/Orders/` | OrderManager.cs, OrderPool.cs |
| Progression | `Scripts/Economy/` | XPManager.cs, CoinManager.cs |

---

*Project structure and boundaries defined. Architecture complete.*

---

## Architecture Completion Summary

### Document Status: ✅ COMPLETE

**Steps Completed:** [1, 2, 3, 4, 5, 6]

### Decisions Documented:

1. ✅ **Established Technical Foundation** - Unity 6.0 + NGO + EOS locked
2. ✅ **Project Context Analysis** - 55 FRs analyzed, 8 cross-cutting concerns identified
3. ✅ **Core Architectural Decisions** - 6 critical decisions with implementation patterns
4. ✅ **Implementation Patterns** - Naming, networking, AI, abilities, error handling
5. ✅ **Project Structure** - Complete Unity directory tree with team-based mode support

### Key Architecture Highlights:

- **Host-Only Bot AI** - Runs on host client, broadcasts actions via NGO
- **Server-Authoritative Abilities** - All gameplay-affecting actions validated by host
- **Strategy Pattern Game Modes** - Flexible win conditions, data-driven scoring
- **Layered State Sync** - Critical (reliable), Gameplay (30Hz), Cosmetic (unreliable)
- **Team-Based Support** - 2-team modes (Red/Blue) with balanced bot filling

### MVP Critical Path:

1. **Week 1-2:** Bot AI Implementation (20% → 70%)
2. **Week 1-2:** Win Conditions (fix ClassicGameModeLogic)
3. **Week 3-4:** 4 Missing Abilities (Push, Magnet, InstantCook, FreezeTime)
4. **Week 5-6:** Testing & Polish

### AI Agent Implementation Ready

This architecture document provides:
- ✅ Clear patterns for consistent implementation
- ✅ File locations for all 55 functional requirements
- ✅ Network synchronization patterns
- ✅ Naming conventions and code organization
- ✅ Team-based game mode support

**Architecture is complete and ready for AI agent implementation.**

---

## Clean Architecture Layer Separation (CRITICAL)

### Philosophy: Domain vs. Gameplay

**This architecture follows Clean Architecture principles separating pure game logic from Unity-specific implementation.**

**Why This Matters:**
- ✅ 70% of code becomes **unit testable** without Unity
- ✅ **Portable** to other engines (Unreal, Godot) or dedicated servers
- ✅ **Clear boundaries** - Domain = rules, Gameplay = technology
- ✅ **AI agents** know exactly what belongs where

---

### Layer 1: Domain Layer (Pure C# - No Unity)

**Location:** `Assets/Scripts/Domain/`

**Rules:**
- ❌ NO `using UnityEngine;`
- ❌ NO `MonoBehaviour`
- ❌ NO `NetworkBehaviour`
- ✅ Pure C# classes, interfaces, logic
- ✅ 100% unit testable

**Responsibilities:**
- Game rules and algorithms
- AI decision logic
- Scoring calculations
- State machines (logic only)
- Math and calculations

**Example Structure:**
```
Domain/
├── GameLogic/
│   ├── OrderLogic.cs              # Order generation rules
│   ├── ScoringLogic.cs            # Point calculations
│   ├── WinConditionLogic.cs       # Victory condition checks
│   ├── ComboLogic.cs              # Combo multiplier math
│   └── GameRules.cs               # Core game constants
│
├── BotAI/
│   ├── BotDecisionEngine.cs       # Perception → Decision → Action
│   ├── BotGoalSelector.cs         # Priority selection logic
│   ├── BotActionPlanner.cs        # Action sequence planning
│   ├── BotPerceptionLogic.cs      # What bot can "see"
│   └── BotMemory.cs               # Bot's knowledge model
│
├── Abilities/
│   ├── IAbilityLogic.cs           # Pure ability interface
│   ├── CooldownLogic.cs           # Cooldown math
│   ├── EffectCalculator.cs        # Effect calculations
│   └── Implementations/
│       ├── SpeedBoostLogic.cs
│       ├── PushLogic.cs
│       └── MagnetLogic.cs
│
├── Common/
│   ├── StateMachine.cs            # Generic state machine
│   ├── PriorityQueue.cs           # For AI pathfinding
│   ├── EventBus.cs               # Decoupled event system
│   └── MathUtils.cs              # Game math helpers
│
└── Models/
    ├── PlayerStats.cs            # Pure data - no Unity
    ├── OrderData.cs              # Order data structure
    ├── MatchStateData.cs         # Match state snapshot
    └── AbilityState.cs           # Ability state data
```

**Example Domain Code:**
```csharp
// Domain/BotAI/BotDecisionEngine.cs
// PURE C# - No Unity dependencies

using System.Collections.Generic;
using RecipeRage.Domain.Models;

namespace RecipeRage.Domain.BotAI
{
    public class BotDecisionEngine
    {
        private readonly BotPerceptionLogic _perception;
        private readonly BotGoalSelector _goalSelector;
        private readonly BotActionPlanner _actionPlanner;
        
        public BotDecisionEngine(BotDifficulty difficulty)
        {
            _perception = new BotPerceptionLogic();
            _goalSelector = new BotGoalSelector(difficulty);
            _actionPlanner = new BotActionPlanner();
        }
        
        public BotAction DecideAction(BotContext context)
        {
            // 1. Perception - What does bot know?
            var knowledge = _perception.GatherKnowledge(context);
            
            // 2. Decision - What should bot do?
            var goal = _goalSelector.SelectGoal(knowledge);
            
            // 3. Action - How to achieve goal?
            var action = _actionPlanner.PlanAction(goal, knowledge);
            
            return action;
        }
    }
    
    public class BotAction
    {
        public BotActionType Type { get; set; }
        public int TargetStationId { get; set; }
        public int TargetIngredientId { get; set; }
        public int Priority { get; set; }
    }
}
```

---

### Layer 2: Gameplay Layer (Unity-Specific)

**Location:** `Assets/Scripts/Gameplay/`

**Rules:**
- ✅ Uses `UnityEngine`
- ✅ Uses `Unity.Netcode`
- ✅ Uses `MonoBehaviour` and `NetworkBehaviour`
- ✅ Thin layer - delegates to Domain
- ✅ Handles: networking, rendering, input, physics

**Responsibilities:**
- Network synchronization
- Visual representation
- Audio/FX
- Input handling
- Unity component integration
- Adapts Domain models to Unity world

**Example Structure:**
```
Gameplay/
├── BotAI/
│   ├── BotController.cs           # NetworkBehaviour wrapper
│   │   └── Uses: BotDecisionEngine (Domain)
│   ├── BotMovement.cs              # Unity NavMeshAgent
│   ├── BotPerceptionAdapter.cs     # Unity physics raycasts
│   ├── BotAnimation.cs             # Unity Animator
│   └── BotVFX.cs                   # Unity particles
│
├── Abilities/
│   ├── AbilitySystem.cs            # NetworkBehaviour
│   │   └── Uses: CooldownLogic (Domain)
│   ├── AbilityInputHandler.cs      # Input → ServerRpc
│   ├── AbilityVFX.cs               # Visual effects
│   ├── AbilitySFX.cs               # Sound effects
│   └── Implementations/
│       ├── SpeedBoostAbility.cs    # NetworkBehaviour
│       │   └── Uses: SpeedBoostLogic (Domain)
│       ├── PushAbility.cs
│       └── MagnetAbility.cs
│
├── Stations/
│   ├── StationController.cs        # NetworkBehaviour
│   │   └── Uses: StationLogic (Domain)
│   ├── StationInteraction.cs       # Player interaction
│   ├── StationProgressBar.cs       # UI
│   └── StationVFX.cs               # Cooking effects
│
├── Orders/
│   ├── OrderManager.cs             # NetworkBehaviour
│   │   └── Uses: OrderLogic (Domain)
│   ├── OrderVisual.cs              # Order UI/3D
│   └── OrderTimerUI.cs             # Expiration countdown
│
└── Common/
    ├── NetworkAdapter.cs           # Domain → Network sync
    ├── UnityEventBus.cs            # Unity-specific events
    └── PoolManager.cs              # Unity object pooling
```

**Example Gameplay Code:**
```csharp
// Gameplay/BotAI/BotController.cs
// Unity-specific - NetworkBehaviour wrapper

using UnityEngine;
using Unity.Netcode;
using RecipeRage.Domain.BotAI;
using RecipeRage.Domain.Models;

namespace RecipeRage.Gameplay.BotAI
{
    public class BotController : NetworkBehaviour
    {
        [Header("Bot Settings")]
        [SerializeField] private BotDifficulty difficulty;
        
        // Domain logic instance
        private BotDecisionEngine _decisionEngine;
        private BotContext _context;
        
        // Unity components
        private NavMeshAgent _navAgent;
        private BotAnimation _animation;
        private BotPerceptionAdapter _perceptionAdapter;
        
        public override void OnNetworkSpawn()
        {
            // Initialize pure C# domain logic
            _decisionEngine = new BotDecisionEngine(difficulty);
            _context = new BotContext();
            
            // Initialize Unity components
            _navAgent = GetComponent<NavMeshAgent>();
            _animation = GetComponent<BotAnimation>();
            _perceptionAdapter = new BotPerceptionAdapter(transform);
        }
        
        private void Update()
        {
            // CRITICAL: Only host runs AI logic
            if (!IsHost) return;
            
            // 10Hz decision tick
            if (Time.time >= _nextDecisionTime)
            {
                // Update context from Unity world
                UpdateBotContext();
                
                // Delegate to pure C# domain logic
                var action = _decisionEngine.DecideAction(_context);
                
                // Execute in Unity world
                ExecuteAction(action);
                
                // Broadcast to clients
                BroadcastBotActionClientRpc(action);
                
                _nextDecisionTime = Time.time + 0.1f;
            }
        }
        
        private void UpdateBotContext()
        {
            // Adapt Unity world to Domain model
            _context.BotPosition = transform.position;
            _context.VisibleStations = _perceptionAdapter.GetVisibleStations();
            _context.ActiveOrders = GetActiveOrdersFromOrderManager();
            _context.CarriedItems = GetCarriedItemsFromInventory();
        }
        
        private void ExecuteAction(BotAction action)
        {
            // Unity-specific execution
            switch (action.Type)
            {
                case BotActionType.MoveToStation:
                    var station = GetStationById(action.TargetStationId);
                    _navAgent.SetDestination(station.Position);
                    _animation.PlayWalk();
                    break;
                    
                case BotActionType.PickUpIngredient:
                    // Interact with station
                    station.Interact(this);
                    _animation.PlayInteract();
                    break;
                    
                // ... other actions
            }
        }
        
        [ClientRpc]
        private void BroadcastBotActionClientRpc(BotActionData actionData)
        {
            // Clients visualize (no logic)
            if (IsHost) return;
            
            VisualizeAction(actionData);
        }
    }
}
```

---

### Migration Strategy: Existing Codebase

**EXISTING Code (Mixed - 55% Complete):**
```csharp
// Current SpeedBoost - Mixed Unity + Logic
public class SpeedBoostAbility : NetworkBehaviour
{
    private float cooldown = 20f;
    
    [ServerRpc]
    public void Activate()
    {
        player.moveSpeed *= 2f;  // Unity + logic mixed
        StartCoroutine(ResetSpeed());
    }
}
```

**REFACTOR To:**

**1. Domain Layer (Pure C#):**
```csharp
// Domain/Abilities/SpeedBoostLogic.cs
public class SpeedBoostLogic : IAbilityLogic
{
    public float Cooldown => 20f;
    public float Duration => 5f;
    public float SpeedMultiplier => 2f;
    
    public void ApplyEffect(PlayerStats stats)
    {
        stats.SpeedMultiplier = SpeedMultiplier;
    }
    
    public void RemoveEffect(PlayerStats stats)
    {
        stats.SpeedMultiplier = 1f;
    }
}
```

**2. Gameplay Layer (Unity):**
```csharp
// Gameplay/Abilities/SpeedBoostAbility.cs
public class SpeedBoostAbility : NetworkBehaviour
{
    private SpeedBoostLogic _logic = new();
    private PlayerController _player;
    
    [ServerRpc]
    public void ActivateServerRpc()
    {
        if (!CanActivate()) return;
        
        // Delegate to pure logic
        _logic.ApplyEffect(_player.Stats);
        
        // Unity-specific
        PlayVFX();
        PlaySFX();
        
        // Schedule removal
        Invoke(nameof(RemoveEffect), _logic.Duration);
    }
    
    private void RemoveEffect()
    {
        _logic.RemoveEffect(_player.Stats);
    }
}
```

---

### Benefits of This Separation

**For RecipeRage:**
- ✅ Bot AI logic can run on dedicated server (future)
- ✅ Can unit test 70% of code without Unity
- ✅ Easy to balance abilities with pure math tests
- ✅ Win conditions testable without full game

**For AI Agents:**
- ✅ Clear boundaries - "Is this Unity-specific?"
- ✅ Domain code = algorithms, Gameplay code = adapters
- ✅ Easier to write tests for Domain layer
- ✅ Can mock Domain in Gameplay tests

**For Future Ports:**
- ✅ Same `BotDecisionEngine` works in Unreal
- ✅ Same `ScoringLogic` works on backend server
- ✅ Only Gameplay layer changes per platform

---

### AI Agent Implementation Rules

**When implementing ANY feature:**

**1. Ask: "Is this game logic or Unity tech?"**
- Game logic → Domain layer
- Unity tech → Gameplay layer

**2. Domain Layer Can:**
- Calculate, decide, validate, store state
- Use: C# collections, math, algorithms

**3. Domain Layer Cannot:**
- Use Unity types (Vector3, GameObject, MonoBehaviour)
- Use networking (RPCs, NetworkVariables)
- Access rendering/physics/input

**4. Gameplay Layer Must:**
- Adapt Domain to Unity world
- Handle networking synchronization
- Manage visual/audio presentation

**5. Communication:**
- Gameplay creates Domain instances
- Gameplay calls Domain methods
- Domain returns data/results
- Gameplay applies to Unity world

---

### Testing Strategy

**Domain Tests (Fast, No Unity):**
```csharp
[Test]
public void BotDecisionEngine_Selects_Urgent_Order()
{
    var engine = new BotDecisionEngine(BotDifficulty.Normal);
    var context = new BotContext
    {
        ActiveOrders = new[]
        {
            new OrderData { Type = OrderType.Simple, Urgency = 0.3f },
            new OrderData { Type = OrderType.Complex, Urgency = 0.9f }
        }
    };
    
    var action = engine.DecideAction(context);
    
    Assert.AreEqual(BotActionType.PrepareOrder, action.Type);
    Assert.AreEqual(OrderType.Complex, action.TargetOrderType);
}
```

**Gameplay Tests (Integration, With Unity):**
```csharp
[Test]
public void BotController_Moves_To_Station()
{
    var bot = CreateBotWithNavMesh();
    var station = CreateStationAt(Vector3.forward * 10);
    
    bot.ExecuteAction(new BotAction
    {
        Type = BotActionType.MoveToStation,
        TargetStationId = station.Id
    });
    
    Assert.IsTrue(bot.IsMoving);
    Assert.AreEqual(station.Position, bot.NavAgent.destination);
}
```

---

### Directory Structure (Updated)

```
Assets/Scripts/
├── Domain/                    # PURE C# - No Unity
│   ├── GameLogic/
│   ├── BotAI/
│   ├── Abilities/
│   ├── Common/
│   └── Models/
│
└── Gameplay/                  # Unity-Specific
    ├── BotAI/
    ├── Abilities/
    ├── Stations/
    ├── Orders/
    ├── Matchmaking/
    ├── Social/
    ├── UI/
    └── Utils/
```

---

### Migration Priority

**Week 1-2 (MVP Critical):**
1. Create `Domain/` folder structure
2. Extract `BotDecisionEngine` from existing BotController
3. Keep existing working code - wrap with Domain calls
4. New Bot AI features → Domain first

**Week 3-4 (Abilities):**
1. Create `AbilityLogic` interfaces in Domain
2. Refactor SpeedBoost → Domain + Gameplay split
3. Implement new abilities following same pattern

**Week 5-6 (Polish):**
1. Unit tests for Domain layer
2. Integration tests for Gameplay layer
3. Document patterns for future features

---

*Clean Architecture Layer Separation documented. AI agents should follow Domain/Gameplay separation for testability, portability, and maintainability.*

---

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:**
All 6 architectural decisions work together seamlessly:
- **Host-Only Bot AI** ↔ **NGO Host Authority** - Perfect alignment; bots run on host leveraging existing NGO infrastructure
- **Server-Authoritative Abilities** ↔ **P2P Networking** - Consistent host-validation model
- **Strategy Pattern Game Modes** ↔ **State Machine** - Flexible, extensible design
- **Layered State Sync** ↔ **Performance Requirements** - Appropriate reliability levels for 30Hz tick rate

**Pattern Consistency:**
- Naming conventions established across all 7 pattern categories
- NGO networking patterns support server-authoritative decisions
- Bot AI patterns align with host-only architecture
- Error handling patterns support reliability requirements

**Structure Alignment:**
- Complete Unity directory tree supports all architectural decisions
- Clean Architecture separation (Domain/Gameplay) enables testability
- Team-based structure supports Red vs Blue game modes
- Integration points properly mapped

### Requirements Coverage Validation ✅

**Functional Requirements Coverage (55 FRs across 10 categories):**

| FR Category | Status | Architectural Support |
|-------------|--------|---------------------|
| Matchmaking & Sessions | ✅ | HostSelector, HostMigration, BotFiller |
| Character & Abilities | ✅ | IAbility interface, 5 MVP abilities planned |
| Bot AI System | ✅ | BotDecisionEngine, BotStateMachine, host-only pattern |
| Cooking & Stations | ✅ | StationController integration points defined |
| Order & Scoring | ✅ | OrderManager, ScoreManager, ComboSystem |
| Social & Friends | ✅ | FriendsManager, OnlineStatus, ChatSystem |
| Progression & Unlocks | ✅ | XPManager, CoinManager, UnlockSystem |
| Account & Auth | ✅ | AuthManager, GuestAccount, EOS integration |
| Communication | ✅ | Quick chat, post-match lobby architecture |
| Game Mode System | ✅ | Strategy pattern for multiple modes |

**Non-Functional Requirements Coverage (36 NFRs):**
- ✅ **Network Performance**: P2P <100ms latency, 30Hz sync, client prediction
- ✅ **Game Performance**: 30+ FPS target, <200MB RAM, optimization patterns
- ✅ **Reliability**: Host migration, reconnection window, graceful degradation
- ✅ **Security**: Server-authoritative validation, anti-cheat architecture

### Implementation Readiness Validation ✅

**Decision Completeness:**
- ✅ All 6 critical decisions documented with rationale and code examples
- ✅ Technology versions specified (Unity 6.0, NGO, EOS, VContainer, UniTask)
- ✅ Implementation patterns include concrete code examples
- ✅ Consistency rules clearly stated with DO/DON'T examples

**Structure Completeness:**
- ✅ Complete Unity project directory structure (170+ files mapped)
- ✅ Component boundaries defined (Domain vs Gameplay layers)
- ✅ Integration points specified (RPCs, NetworkVariables, Events)
- ✅ Requirements-to-structure mapping complete

**Pattern Completeness:**
- ✅ 7 pattern categories fully specified:
  1. Naming Conventions (C# standards)
  2. NGO Networking Patterns (ServerRpc/ClientRpc)
  3. Bot AI Implementation Pattern
  4. Ability System Pattern
  5. Error Handling Patterns
  6. Project Structure
  7. State Synchronization Format

### Gap Analysis Results

**Critical Gaps:** None identified ✅

**Important Gaps:** None identified ✅

**Nice-to-Have Enhancements (Optional):**
- CI/CD pipeline details (GitHub Actions mentioned but not detailed)
- Specific analytics event schema
- Detailed Firebase integration patterns
- Localization architecture (if multi-language support needed later)

### Validation Issues Addressed

No critical or important validation issues were identified. The architecture is comprehensive and ready for implementation.

### Architecture Completeness Checklist

**✅ Requirements Analysis**

- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed (HIGH complexity identified)
- [x] Technical constraints identified (MVP 6-week timeline)
- [x] Cross-cutting concerns mapped (8 concerns identified)

**✅ Architectural Decisions**

- [x] All 6 critical decisions documented with versions
- [x] Technology stack fully specified (Unity 6.0 + NGO + EOS)
- [x] Integration patterns defined (RPCs, NetworkVariables)
- [x] Performance considerations addressed (30Hz, <100ms latency)

**✅ Implementation Patterns**

- [x] Naming conventions established (PascalCase, camelCase, etc.)
- [x] Structure patterns defined (Domain/Gameplay separation)
- [x] Communication patterns specified (Server-authoritative)
- [x] Process patterns documented (error handling, state sync)

**✅ Project Structure**

- [x] Complete directory structure defined (Scripts/, Tests/, Resources/)
- [x] Component boundaries established (Core, Gameplay, Domain)
- [x] Integration points mapped (RPC payloads, NetworkVariables)
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** ✅ **READY FOR IMPLEMENTATION**

**Confidence Level:** **HIGH**

**Key Strengths:**
1. **Comprehensive Coverage** - All 55 FRs and 36 NFRs addressed
2. **Clear Patterns** - 7 pattern categories with concrete examples
3. **Clean Architecture** - Domain/Gameplay separation enables testing
4. **Technology Alignment** - Decisions leverage existing 85% complete infrastructure
5. **AI-Agent Ready** - Specific rules to prevent implementation conflicts

**Areas for Future Enhancement (Post-MVP):**
- Advanced anti-cheat beyond server-authoritative
- Dedicated server architecture (if scaling beyond P2P)
- Analytics pipeline detailed schema
- Localization framework

### Implementation Handoff

**AI Agent Guidelines:**

- Follow all 6 architectural decisions exactly as documented
- Implement Clean Architecture: Pure C# Domain + Unity Gameplay layers
- Use established naming conventions and patterns consistently
- Respect host-only AI and server-authoritative validation
- Refer to this document for all architectural questions

**MVP Critical Path:**
1. **Week 1-2**: Bot AI (20% → 70%) - Domain extraction + Gameplay wrapper
2. **Week 1-2**: Win Conditions - Fix ClassicGameModeLogic commented code
3. **Week 3-4**: 4 Missing Abilities (Push, Magnet, InstantCook, FreezeTime)
4. **Week 5-6**: Testing & Polish

---

*Architecture Decision Document - RecipeRage - Version 1.0*  
*Date: 2026-02-04*  
*Status: Complete - Validated and Ready for Implementation*
