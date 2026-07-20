# RecipeRage Codebase Analysis Index

**Generated:** June 1, 2026  
**Analysis Scope:** Build system, testing, linting, architecture, DI, networking, state machines, service patterns

---

## 📚 Documentation Files Created

### 1. **CODEBASE_ANALYSIS.md** (30 KB, 1006 lines)
**Comprehensive reference guide covering all subsystems**

- **Section 1:** Build System & Commands (dotnet build, csproj structure)
- **Section 2:** Testing Framework & Commands (NUnit, EditMode/PlayMode tests)
- **Section 3:** Linting & Formatting Tools (EditorConfig rules)
- **Section 4:** High-Level Architecture (Two-Bucket, State-Driven design)
- **Section 5:** Key Interfaces & Service Patterns (DI patterns, event bus, state machine)
- **Section 6:** Entry Points & Directory Structure (Assets/ layout, scenes, prefabs)
- **Section 7:** Dependency Injection (VContainer) (Root/Session/Match scopes)
- **Section 8:** Networking Architecture (NGO + EOS integration)
- **Section 9:** State Machine Architecture (IState, BaseState, GameStateManager)
- **Section 10:** Runtime Verification Status (Phase 2 gaps, TODOs)
- **Section 11:** Important Patterns & Conventions (naming, async, event bus usage)
- **Section 12:** Key Documentation Files (source of truth hierarchy)
- **Section 13:** Quick Reference (key classes table)
- **Section 14:** Common Tasks (add state, add service, add UI screen, write test)

### 2. **QUICK_REFERENCE.md** (7.2 KB, 270 lines)
**Fast lookup guide for common operations**

- Build commands (dotnet build, dotnet test)
- Architecture layers diagram (Root, Session, Match, Gameplay, UI)
- State flow diagram
- DI container lifetimes and scopes
- Key service interfaces table
- Code pattern templates (Event Bus, State, Network Object, Testing)
- Entry points
- Task-specific instructions (add state, add service, add screen, write test)
- Source of truth hierarchy
- Code style rules
- Common patterns (async, scene access, service injection)
- Troubleshooting guide
- Performance tips
- Deployment checklist

### 3. **ANALYSIS_INDEX.md** (this file)
**Navigation guide to analysis documents**

---

## 🎯 Quick Start (Choose Your Path)

### I want to understand the overall architecture
→ Read **CODEBASE_ANALYSIS.md** section 4 (High-Level Architecture)

### I want to know how to build and test
→ Read **CODEBASE_ANALYSIS.md** sections 1-2, or **QUICK_REFERENCE.md** top section

### I want to understand dependency injection
→ Read **CODEBASE_ANALYSIS.md** section 7 (Dependency Injection)

### I want to add a new feature
→ Read **CODEBASE_ANALYSIS.md** section 14 (Common Tasks), or **QUICK_REFERENCE.md** "Key Files by Task"

### I want to understand state machines
→ Read **CODEBASE_ANALYSIS.md** section 9 (State Machine Architecture)

### I want to understand networking
→ Read **CODEBASE_ANALYSIS.md** section 8 (Networking Architecture)

### I need a quick reference while coding
→ Use **QUICK_REFERENCE.md** (search by topic)

### I'm writing a test
→ See **CODEBASE_ANALYSIS.md** section 2 (Testing), or **QUICK_REFERENCE.md** "Testing Pattern"

---

## 🏗️ Architecture Overview

### Layers
```
UI Layer (UIService, Screens)
    ↓
App Layer (GameStateManager, IGameStateManager)
    ↓
Root Scope (RootLifetimeScope) - Singletons
    ├─ State Machine
    ├─ Auth, Config, Logging
    ├─ Networking primitives
    └─ Root services
    ↓
Session Scope (SessionLifetimeScope)
    ├─ Matchmaking, Lobby, Team
    └─ (No networking primitives!)
    ↓
Match Scope (MatchLifetimeScope)
    ├─ Score, Orders, Abilities, Hazards
    └─ MatchContext + Scene Bridge
    ↓
Gameplay Scene Layer (Game.unity + additive maps)
    └─ PlayerController, Stations, Orders, Score
```

### State Flow
```
BootstrapState
  → LoginState or SessionLoadingState
  → MainMenuState
  → MatchmakingState
  → GameplayState
  → GameOverState
```

### Key Technologies
| Component | Technology | Purpose |
|-----------|-----------|---------|
| DI | VContainer | Dependency injection, service scopes |
| State Machine | Custom IState | App state transitions |
| Networking | Netcode for GameObjects (NGO) | Multiplayer |
| Auth/Services | Epic Online Services (EOS) | Auth, lobbies, P2P |
| UI | UI Toolkit | Screen system |
| Async | UniTask | Async/await with cancellation |
| Testing | NUnit | Unit tests |
| Logging | Custom | Debug output |
| Style | EditorConfig | Code style enforcement |

---

## 📋 Source of Truth Hierarchy

When you have architecture questions, consult in this order:

1. **Current code** (inspect actual implementation)
2. `Documentation/Architecture/PROJECT_MEMORY.md` (current decisions)
3. `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md` (implementation audit)
4. `Documentation/Guides/gameplay-scene-setup.md` (scene wiring)
5. `KitchenClash_GDD_v3.md` (game design)
6. `conductor/tech-stack.md` (tech decisions)
7. Older archive docs (context only)

---

## 🚀 Development Commands Cheat Sheet

```bash
# Build
dotnet build RecipeRage.Gameplay.csproj -nologo

# Test all (EditMode)
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo

# Test single
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ClassName" --no-build -nologo

# CI mode
CI=true dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
```

---

## 🎓 Key Patterns

### Dependency Injection
```csharp
public class MyService
{
    private readonly IDependency _dep;
    
    public MyService(IDependency dep) // VContainer injects
    {
        _dep = dep;
    }
}
```

### Event Bus
```csharp
_eventBus.Subscribe<MyEvent>(evt => Handle(evt));
_eventBus.Publish(new MyEvent { Data = value });
_eventBus.Unsubscribe<MyEvent>(handler);
```

### State Machine
```csharp
public class MyState : BaseState
{
    public override void Enter() { base.Enter(); /* init */ }
    public override void Update() { if (done) _stateManager.ChangeState<NextState>(); }
    public override void Exit() { base.Exit(); /* cleanup */ }
}
```

### Network Object
```csharp
public class MyNetworkObject : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        // Server/owner logic
    }
    
    [Rpc]
    public void MyRpc(NetworkBehaviourSerialisationStream stream) { }
}
```

### Test
```csharp
public class MyServiceTests
{
    [SetUp]
    public void SetUp() { /* create test doubles */ }
    
    [Test]
    public void Method_Condition_Result() { /* assert */ }
}
```

---

## 📁 Key Directories

| Path | Purpose |
|------|---------|
| `Assets/Scenes/` | Bootstrap, MainMenu, Game scenes |
| `Assets/Prefabs/General/` | GameBootstrap.prefab (root DI) |
| `Assets/_KitchenClash/Composition/` | LifetimeScopes (DI setup) |
| `Assets/_KitchenClash/Application/` | Business logic, interfaces |
| `Assets/_KitchenClash/Domain/` | Domain models, events |
| `Assets/_KitchenClash/Infrastructure/States/` | Game states |
| `Assets/_KitchenClash/Infrastructure/Network/` | Networking, player controllers |
| `Assets/_KitchenClash/Presentation/` | UI, screens, view models |
| `Assets/Scripts/Tests/EditMode/` | Unit tests |
| `Documentation/Architecture/` | Architecture docs |
| `conductor/` | Project workflow, product vision |

---

## ⚡ Critical Rules

1. **Current code is truth** - When docs conflict with code, trust the code
2. **Always inject dependencies** - Never use Singleton or FindObjectOfType()
3. **Publish events** - Couple systems via event bus, not direct calls
4. **Session scope limitations** - Only player-session services, NOT match services
5. **Update PROJECT_MEMORY.md** - When you change architecture
6. **Use UniTask** - Not Task, for async operations
7. **Scoping rules** - Root > Session > Match > Transient

---

## 🔍 Analysis Methodology

This analysis was generated through:

1. **Build System Inspection**
   - Found .csproj files and MSBuild setup
   - Identified dotnet CLI usage
   - Located build configuration

2. **Test Framework Investigation**
   - Located test projects and frameworks
   - Reviewed test patterns and structure
   - Identified test naming conventions

3. **Linting & Code Style**
   - Found EditorConfig rules
   - Reviewed coding standards
   - Identified static analysis tools

4. **Architecture Deep Dive**
   - Analyzed DI setup (VContainer scopes)
   - Reviewed state machine implementation
   - Examined service patterns
   - Investigated networking (NGO + EOS)
   - Reviewed UI system design

5. **Code Pattern Recognition**
   - Extracted common patterns from existing code
   - Identified best practices
   - Documented conventions

6. **Documentation Synthesis**
   - Cross-referenced official docs
   - Identified source-of-truth hierarchy
   - Documented entry points
   - Created reference tables

---

## 📞 Quick Navigation

**Need to...**

- Understand overall design? → CODEBASE_ANALYSIS.md §4
- Set up build? → CODEBASE_ANALYSIS.md §1, QUICK_REFERENCE.md top
- Write tests? → CODEBASE_ANALYSIS.md §2, QUICK_REFERENCE.md "Testing Pattern"
- Add a state? → CODEBASE_ANALYSIS.md §14 or QUICK_REFERENCE.md "Add a new state"
- Add a service? → CODEBASE_ANALYSIS.md §14 or QUICK_REFERENCE.md "Add a new service"
- Add a UI screen? → CODEBASE_ANALYSIS.md §14 or QUICK_REFERENCE.md "Add a new UI screen"
- Understand DI? → CODEBASE_ANALYSIS.md §7
- Understand networking? → CODEBASE_ANALYSIS.md §8
- Understand state machines? → CODEBASE_ANALYSIS.md §9
- Understand patterns? → CODEBASE_ANALYSIS.md §5 & §11, QUICK_REFERENCE.md patterns section
- Quick reference? → QUICK_REFERENCE.md (any section)
- Troubleshoot? → QUICK_REFERENCE.md "Troubleshooting"

---

## 📊 Analysis Statistics

| Metric | Value |
|--------|-------|
| Total lines in analysis | 1,276 |
| CODEBASE_ANALYSIS.md | 1,006 lines |
| QUICK_REFERENCE.md | 270 lines |
| Major sections | 18 |
| Code examples | 50+ |
| Quick reference tables | 15+ |
| Diagrams/flows | 5 |
| Common tasks documented | 4 |
| Troubleshooting tips | 6 |
| Performance tips | 4 |

---

## 🎯 Success Criteria

After reading this analysis, you should be able to:

- ✅ Build the project from command line
- ✅ Run tests and understand test patterns
- ✅ Understand the overall architecture
- ✅ Navigate the codebase directory structure
- ✅ Add a new game state
- ✅ Add a new service
- ✅ Write tests following project conventions
- ✅ Understand DI scopes and service lifetimes
- ✅ Understand state transitions
- ✅ Understand networking integration
- ✅ Know where to find answers (source of truth)
- ✅ Know common patterns and conventions

---

## 📝 Notes

- All file paths are relative to `/Users/arshadbarves/MyProject/Projects/RecipeRage/`
- Analysis assumes Unity 6.0 (6000.3.0f1) and C# .NET 4.7.1
- Current development phase: Phase 2 (Runtime Verification & Stabilization)
- Next phases: Phase 3 (Architecture cleanup), Phase 4+ (advanced features)

---

**For questions or clarifications, refer to the detailed CODEBASE_ANALYSIS.md or QUICK_REFERENCE.md sections listed above.**
