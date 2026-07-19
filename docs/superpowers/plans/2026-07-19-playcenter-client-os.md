# Playcenter Client OS Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Evolve in-repo Playcenter into a production-grade multi-game Client OS (connectivity-first boot, wallet ports, hardened P2P lobby/net session, DesignSystem shell UI, input/settings, live-ops ports) while keeping KitchenClash as the first title adapter.

**Architecture:** Approach A — raise weak modules behind stable Playcenter ports; ROOT → SESSION → MATCH scopes; `IAppFlow` sole navigator; pure `Playcenter.Services` uses `System.Threading.Tasks.Task` only (no UniTask/UnityEngine). Game IP stays in KitchenClash. Spec: `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md` (commit `80d917cd`).

**Tech Stack:** Unity 6, VContainer, NGO, EOS P2P (`EOSTransport` sample pattern), UI Toolkit + DesignSystem.uss, UniTask (Infrastructure/Presentation only), NUnit EditMode tests, `dotnet build` / `dotnet test RecipeRage.Tests.EditMode.csproj`.

## Global Constraints

- Spec is law: `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md`
- Connectivity is **boot step 1** before NTP / RC / force-update / maintenance / auth / session
- Pure Services ports: `Task` not `UniTask`; no `UnityEngine` in `Playcenter.Services`
- Session install law: `ISessionScopeInstaller` + shared `MenuSessionRegistrations` (not bare `CreateChild`)
- MATCH never owns wallet writes; Results posts via SESSION `IWalletLedger`
- Reconnect v1: no host migration; window then forfeit/end
- UI redesign uses **DesignSystem.uss only** (not theme.uss Hot Kitchen dual brand)
- Brawl MP: party on Home → PLAY → MM → match lobby/VS → `INetSession` → match → results → Home (party kept, match lobby destroyed)
- Do **not** commit unrelated WIP (maps, combat, fonts, packages-lock, UserSettings)
- Commit trailer: `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- Wiki updates only in Phase 7 (or after each phase if drift requires Option B)
- Prefer small focused files; follow existing assembly boundaries
- Test naming: `MethodName_Condition_ExpectedResult`
- Build/test: `dotnet build <csproj> -nologo` then `dotnet test RecipeRage.Tests.EditMode.csproj --filter="..." -nologo` (rebuild if needed)

---

## File Map

### Create

| Path | Responsibility |
|------|----------------|
| `Assets/Playcenter/Services/Runtime/Wallet/CurrencyId.cs` | Strongly-typed currency id |
| `Assets/Playcenter/Services/Runtime/Wallet/IWallet.cs` | Read balances |
| `Assets/Playcenter/Services/Runtime/Wallet/IWalletLedger.cs` | Mutate balances (SESSION) |
| `Assets/Playcenter/Services/Runtime/Wallet/IWalletStore.cs` | Load/save wallet snapshot |
| `Assets/Playcenter/Services/Runtime/Wallet/WalletSnapshot.cs` | DTO for persistence |
| `Assets/Playcenter/Services/Runtime/Net/NetRole.cs` | Host/Client enum |
| `Assets/Playcenter/Services/Runtime/Net/INetSession.cs` | Start/stop P2P session port |
| `Assets/Playcenter/Services/Runtime/Net/INetTransportConfigurator.cs` | Bind transport before start |
| `Assets/Playcenter/Services/Runtime/Settings/ISettingsService.cs` | Settings load/save/apply |
| `Assets/Playcenter/Services/Runtime/Settings/GameSettings.cs` | Settings DTO |
| `Assets/Playcenter/Services/Runtime/Input/IGameplayInput.cs` | Mobile+keyboard input port |
| `Assets/Playcenter/Shell/Runtime/Session/ISessionModuleInstaller.cs` | Doc/marker only (optional) |
| `Assets/_KitchenClash/Infrastructure/Economy/EconomyWalletBridge.cs` | `IEconomyService` ↔ wallet ports |
| `Assets/_KitchenClash/Infrastructure/Economy/SaveServiceWalletStore.cs` | `IWalletStore` via `ISaveService` |
| `Assets/_KitchenClash/Infrastructure/Network/NgoEosNetSession.cs` | `INetSession` NGO+EOS adapter |
| `Assets/_KitchenClash/Infrastructure/Network/EosTransportConfigurator.cs` | Wire EOSTransport |
| `Assets/_KitchenClash/Infrastructure/Settings/PlayerPrefsSettingsService.cs` | Settings adapter |
| `Assets/_KitchenClash/Infrastructure/Input/GameplayInputService.cs` | Input adapter |
| `Assets/Scripts/Tests/EditMode/Playcenter/Wallet/*` | Wallet unit tests |
| `Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceConnectivityTests.cs` | Boot gate tests |
| `Assets/Scripts/Tests/EditMode/Gameplay/EconomyWalletBridgeTests.cs` | Bridge tests |
| `Assets/Scripts/Tests/EditMode/Gameplay/NetSessionTests.cs` | Net session fakes/tests |
| `Assets/_KitchenClash/UI/Components/*.uxml` + USS | Shell components (P4) |

### Modify

| Path | Change |
|------|--------|
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs` | Connectivity step 1 |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/NoConnectionPhase.cs` | Boot-aware retry (re-run boot vs session only) |
| `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | Wire BootSequence + connectivity |
| `Assets/_KitchenClash/Composition/MenuSessionRegistrations.cs` | Register wallet + net + settings |
| `Assets/_KitchenClash/Application/Services/EconomyService.cs` | Implement wallet ports or thin façade |
| `Assets/_KitchenClash/Infrastructure/Services/MatchRewardHandler.cs` | Use `IWalletLedger` only |
| `Assets/_KitchenClash/Infrastructure/Network/GameStarter.cs` | Delegate start to `INetSession` |
| `Assets/_KitchenClash/Infrastructure/EOS/EOSLobbyService.cs` | Party vs match lobby harden |
| `Assets/_KitchenClash/UI/Screens/*.uxml` | DesignSystem shell redesign (P4) |
| `wiki/Technical.md`, `wiki/LLM-Rules.md`, `wiki/log.md` | Phase 7 |

### Reference only (do not ship sample UI)

- `Assets/Samples/.../EOSTransport.cs`, `EOSLobbyManager.cs`, `EOSPeer2PeerManager.cs`

---

### Task 1: Connectivity-first BootSequence

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs`
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` (ctor wiring if needed)
- Test: `Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceConnectivityTests.cs`
- Create test doubles under `Assets/Scripts/Tests/EditMode/Gameplay/Fakes/` if missing

**Interfaces:**
- Consumes: `Playcenter.Shell.IConnectivityService` (`bool IsOnline`), `IAppFlow.EnterSidePhase(FlowPhaseId.NoConnection)`, existing BootSequence deps
- Produces: Boot halts on offline before NTP; online path unchanged order after gate

- [x] **Step 1: Write the failing test**

Create `Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceConnectivityTests.cs`:

```csharp
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KitchenClash.Infrastructure.Flow.Handlers;
using NUnit.Framework;
using Playcenter.GameFlow;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Tests.EditMode.Gameplay
{
    public sealed class BootSequenceConnectivityTests
    {
        private sealed class FakeConnectivity : IConnectivityService
        {
            public bool IsOnline { get; set; } = true;
            public ConnectivityState CurrentState =>
                IsOnline ? ConnectivityState.Online : ConnectivityState.OfflineMenu;
            public event System.Action<bool> OnConnectivityChanged;
            public event System.Action<bool> OnConnectionStatusChanged;
            public event System.Action<ConnectivityState> OnStateChanged;
            public void NotifyMatchStarted() { }
            public void NotifyMatchEnded() { }
            public void NotifyHostDropped() { }
        }

        private sealed class SpyAppFlow : IAppFlow
        {
            public FlowPhaseId? EnteredSidePhase { get; private set; }
            public bool BootCompleteNotified { get; private set; }
            // Implement remaining IAppFlow members as no-ops / defaults
            public void EnterSidePhase(FlowPhaseId phase) => EnteredSidePhase = phase;
            public void NotifyBootComplete() => BootCompleteNotified = true;
            public void CompleteSidePhase() { }
            public void StartColdBoot() { }
            public void RequestPlay() { }
            public void ReturnHome() { }
            public void RequestPlayAgain() { }
            // Add any other interface members required by current IAppFlow
        }

        private sealed class CountingNtp : INTPTimeService
        {
            public int SyncCalls { get; private set; }
            public Task<bool> SyncTime() { SyncCalls++; return Task.FromResult(true); }
            // stub other members if required
        }

        [Test]
        public async Task RunAsync_WhenOffline_EntersNoConnection_AndSkipsNtp()
        {
            var connectivity = new FakeConnectivity { IsOnline = false };
            var appFlow = new SpyAppFlow();
            var ntp = new CountingNtp();
            // Construct BootSequence with fakes for all deps; null-safe stubs for unused services
            var boot = CreateBoot(connectivity, appFlow, ntp, /* other fakes */);

            await boot.RunAsync(CancellationToken.None);

            Assert.AreEqual(FlowPhaseId.NoConnection, appFlow.EnteredSidePhase);
            Assert.AreEqual(0, ntp.SyncCalls);
            Assert.IsFalse(appFlow.BootCompleteNotified);
        }

        [Test]
        public async Task RunAsync_WhenOnline_ProceedsPastConnectivity_CallsNtp()
        {
            var connectivity = new FakeConnectivity { IsOnline = true };
            var appFlow = new SpyAppFlow();
            var ntp = new CountingNtp();
            var boot = CreateBoot(connectivity, appFlow, ntp, /* force update false, auth empty → Login */);

            await boot.RunAsync(CancellationToken.None);

            Assert.GreaterOrEqual(ntp.SyncCalls, 1);
            Assert.AreNotEqual(FlowPhaseId.NoConnection, appFlow.EnteredSidePhase);
        }

        // CreateBoot: mirror RootLifetimeScope wiring — inject IConnectivityService as first gate dep
        private static BootSequence CreateBoot(
            IConnectivityService connectivity,
            IAppFlow appFlow,
            INTPTimeService ntp
            /* + IRemoteConfigService, IForceUpdateChecker, IMaintenanceService, IAuthService, SessionLoader stubs */)
        {
            // Implement with minimal stubs that succeed quickly; auth unauthenticated to exit early after NTP/RC
            throw new System.NotImplementedException("Wire to BootSequence ctor after Step 3 adds connectivity param");
        }
    }
}
```

**Note:** Open `IAppFlow`, `INTPTimeService`, `IConnectivityService`, and current `BootSequence` ctor and complete fakes so the test compiles. Prefer existing fakes in `Assets/Scripts/Tests/EditMode/Gameplay/Fakes/` if present.

- [x] **Step 2: Run test to verify it fails**

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~BootSequenceConnectivityTests" -nologo
```

Expected: FAIL (BootSequence has no connectivity gate / CreateBoot not wired) or compile error on missing ctor param.

- [x] **Step 3: Implement connectivity gate in BootSequence**

Update ctor to take `IConnectivityService connectivity` (required). At start of `RunAsync`:

```csharp
// 0. Connectivity gate (spec: boot step 1)
if (_connectivity == null || !_connectivity.IsOnline)
{
    GameLogger.LogInfo("[BootSequence] Offline — entering NoConnection.");
    _appFlow?.EnterSidePhase(FlowPhaseId.NoConnection);
    return;
}
```

Then keep existing steps renumbered in comments: 1 NTP, 2 RC init, 3 RC refresh, 4 force update, 5 maintenance, 6 auth, 7 session.

Wire in `RootLifetimeScope` where `new BootSequence(...)` is constructed — pass the already-registered `IConnectivityService` instance.

- [x] **Step 4: Fix NoConnectionPhase boot retry**

When offline at cold boot, Retry must not only `SessionLoader.LoadAsync` (session may never have been created). Options (pick one, document in code):

**Preferred:** Inject a `Func<CancellationToken, UniTask> retryBoot` or `BootSequence` and on Retry call `await _bootSequence.RunAsync(ct)` then if still active and online path completed side phase via `NotifyBootComplete` / `CompleteSidePhase` as today.

Minimal change if BootSequence is root-owned:

```csharp
// NoConnectionPhase.OnRetry
await _bootSequence.RunAsync(CancellationToken.None);
// BootSequence either NotifyBootComplete, EnterSidePhase(Login|ForceUpdate|...), or re-enters NoConnection
```

Avoid double-Enter of NoConnection without Exit — BootSequence should call EnterSidePhase only when still offline; phase host should be re-entrant safe (Exit then Enter already in `Enter()`).

- [x] **Step 5: Complete CreateBoot fakes and run tests**

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~BootSequenceConnectivityTests" -nologo
```

Expected: PASS

- [x] **Step 6: Commit**

```bash
git add Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs \
  Assets/_KitchenClash/Infrastructure/Flow/Handlers/NoConnectionPhase.cs \
  Assets/_KitchenClash/Composition/RootLifetimeScope.cs \
  Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceConnectivityTests.cs \
  Assets/Scripts/Tests/EditMode/Gameplay/Fakes/
git commit -m "$(cat <<'EOF'
feat(boot): gate cold boot on connectivity before NTP

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 2: Wallet ports in Playcenter.Services

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/Wallet/CurrencyId.cs`
- Create: `Assets/Playcenter/Services/Runtime/Wallet/WalletSnapshot.cs`
- Create: `Assets/Playcenter/Services/Runtime/Wallet/IWallet.cs`
- Create: `Assets/Playcenter/Services/Runtime/Wallet/IWalletLedger.cs`
- Create: `Assets/Playcenter/Services/Runtime/Wallet/IWalletStore.cs`
- Test: `Assets/Scripts/Tests/EditMode/Playcenter/Wallet/WalletPortContractTests.cs`

**Interfaces:**
- Consumes: nothing (pure ports)
- Produces: ports below (exact signatures)

```csharp
namespace Playcenter.Services
{
    public readonly struct CurrencyId : System.IEquatable<CurrencyId>
    {
        public string Value { get; }
        public CurrencyId(string value) => Value = value ?? string.Empty;
        public static CurrencyId Coins { get; } = new("coins");
        public static CurrencyId Gems { get; } = new("gems");
        public bool Equals(CurrencyId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is CurrencyId c && Equals(c);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override string ToString() => Value;
        public static bool operator ==(CurrencyId a, CurrencyId b) => a.Equals(b);
        public static bool operator !=(CurrencyId a, CurrencyId b) => !a.Equals(b);
    }

    public sealed class WalletSnapshot
    {
        public int Coins { get; set; }
        public int Gems { get; set; }
        public string[] OwnedItemIds { get; set; }
    }

    public interface IWallet
    {
        int GetBalance(CurrencyId currency);
        bool HasItem(string itemId);
    }

    public interface IWalletLedger
    {
        bool TryDebit(CurrencyId currency, int amount, string reason);
        void Credit(CurrencyId currency, int amount, string reason);
        bool TryPurchase(string itemId, CurrencyId currency, int cost, string reason);
    }

    public interface IWalletStore
    {
        System.Threading.Tasks.Task<WalletSnapshot> LoadAsync(System.Threading.CancellationToken ct = default);
        System.Threading.Tasks.Task SaveAsync(WalletSnapshot snapshot, System.Threading.CancellationToken ct = default);
    }
}
```

- [x] **Step 1: Write contract test (compiles against ports)**

```csharp
using NUnit.Framework;
using Playcenter.Services;

namespace KitchenClash.Tests.EditMode.Playcenter.Wallet
{
    public sealed class WalletPortContractTests
    {
        private sealed class MemStore : IWalletStore
        {
            public WalletSnapshot Snap = new WalletSnapshot { Coins = 100, Gems = 0 };
            public System.Threading.Tasks.Task<WalletSnapshot> LoadAsync(System.Threading.CancellationToken ct = default)
                => System.Threading.Tasks.Task.FromResult(Snap);
            public System.Threading.Tasks.Task SaveAsync(WalletSnapshot snapshot, System.Threading.CancellationToken ct = default)
            {
                Snap = snapshot;
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

        private sealed class MemWallet : IWallet, IWalletLedger
        {
            private int _coins = 100, _gems;
            private readonly System.Collections.Generic.HashSet<string> _items = new();
            public int GetBalance(CurrencyId c) => c.Equals(CurrencyId.Gems) ? _gems : _coins;
            public bool HasItem(string id) => _items.Contains(id);
            public bool TryDebit(CurrencyId c, int amount, string reason)
            {
                if (amount < 0) return false;
                if (c.Equals(CurrencyId.Gems)) { if (_gems < amount) return false; _gems -= amount; return true; }
                if (_coins < amount) return false; _coins -= amount; return true;
            }
            public void Credit(CurrencyId c, int amount, string reason)
            {
                if (amount <= 0) return;
                if (c.Equals(CurrencyId.Gems)) _gems += amount; else _coins += amount;
            }
            public bool TryPurchase(string itemId, CurrencyId currency, int cost, string reason)
            {
                if (HasItem(itemId)) return false;
                if (!TryDebit(currency, cost, reason)) return false;
                _items.Add(itemId);
                return true;
            }
        }

        [Test]
        public void TryDebit_Insufficient_ReturnsFalse()
        {
            var w = new MemWallet();
            Assert.IsFalse(w.TryDebit(CurrencyId.Coins, 9999, "test"));
            Assert.AreEqual(100, w.GetBalance(CurrencyId.Coins));
        }

        [Test]
        public void Credit_IncreasesBalance()
        {
            var w = new MemWallet();
            w.Credit(CurrencyId.Coins, 50, "reward");
            Assert.AreEqual(150, w.GetBalance(CurrencyId.Coins));
        }

        [Test]
        public void CurrencyId_CoinsAndGems_AreDistinct()
        {
            Assert.AreNotEqual(CurrencyId.Coins, CurrencyId.Gems);
            Assert.AreEqual("coins", CurrencyId.Coins.Value);
        }
    }
}
```

- [x] **Step 2: Run test — expect compile fail (types missing)**

```bash
dotnet build Playcenter.Services.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: CS0246 on `IWallet` / `CurrencyId`.

- [x] **Step 3: Add port files under `Assets/Playcenter/Services/Runtime/Wallet/`**

Use exact signatures from Interfaces block. No UnityEngine. No UniTask.

Unity regenerates csproj Compile includes — if build misses new files, open Unity once or ensure asmdef folder inclusion (`Playcenter.Services.asmdef` already covers Runtime/**).

- [x] **Step 4: Build + test pass**

```bash
dotnet build Playcenter.Services.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~WalletPortContractTests" -nologo
```

Expected: PASS

- [x] **Step 5: Commit**

```bash
git add Assets/Playcenter/Services/Runtime/Wallet/ \
  Assets/Scripts/Tests/EditMode/Playcenter/Wallet/
git commit -m "$(cat <<'EOF'
feat(services): add IWallet / IWalletLedger / IWalletStore ports

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 3: EconomyWalletBridge + session registration

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Economy/SaveServiceWalletStore.cs`
- Create: `Assets/_KitchenClash/Infrastructure/Economy/EconomyWalletBridge.cs` (optional if EconomyService implements ports directly)
- Modify: `Assets/_KitchenClash/Application/Services/EconomyService.cs` — implement `IWallet` + `IWalletLedger`
- Modify: `Assets/_KitchenClash/Composition/MenuSessionRegistrations.cs` — register `IWallet`, `IWalletLedger`, `IWalletStore` → same EconomyService instance
- Modify: `Assets/_KitchenClash/Infrastructure/Services/MatchRewardHandler.cs` — depend on `IWalletLedger`
- Test: `Assets/Scripts/Tests/EditMode/Gameplay/EconomyWalletBridgeTests.cs`

**Interfaces:**
- Consumes: Task 2 ports; existing `IEconomyService`, `ISaveService`, `EconomyKeys`
- Produces: SESSION resolves `IWallet`/`IWalletLedger`/`IEconomyService` as one object; match rewards credit via ledger

- [x] **Step 1: Failing tests for EconomyService as ledger**

```csharp
[Test]
public void TryDebit_MapsToTrySpendCoins()
{
    var economy = new EconomyService(new SpyEventBus(), new NullSaveService());
    economy.Initialize();
    int before = economy.Coins;
    Assert.IsTrue(((IWalletLedger)economy).TryDebit(CurrencyId.Coins, 10, "shop"));
    Assert.AreEqual(before - 10, economy.Coins);
}

[Test]
public void Credit_MatchReward_DoesNotTouchMatchScope()
{
    var economy = new EconomyService(new SpyEventBus(), new NullSaveService());
    economy.Initialize();
    ((IWalletLedger)economy).Credit(CurrencyId.Coins, 50, "match_win");
    Assert.AreEqual(EconomyService.StarterCoins + 50, economy.GetBalance(CurrencyId.Coins));
}
```

Use existing `SpyEventBus` / save fakes from EditMode tests.

- [x] **Step 2: Run — fail (EconomyService does not implement IWalletLedger)**

- [x] **Step 3: Implement on EconomyService**

```csharp
public sealed class EconomyService : IEconomyService, IWallet, IWalletLedger
{
    public int GetBalance(CurrencyId currency) =>
        currency.Equals(CurrencyId.Gems) ? _gems : _coins;

    public bool TryDebit(CurrencyId currency, int amount, string reason) =>
        currency.Equals(CurrencyId.Gems) ? TrySpendGems(amount) : TrySpendCoins(amount);

    public void Credit(CurrencyId currency, int amount, string reason)
    {
        if (currency.Equals(CurrencyId.Gems)) AddGems(amount);
        else AddCoins(amount);
    }

    public bool TryPurchase(string itemId, CurrencyId currency, int cost, string reason) =>
        Purchase(itemId, cost, currency.Equals(CurrencyId.Gems) ? EconomyKeys.CurrencyGems : EconomyKeys.CurrencyCoins);
}
```

Keep `IEconomyService` methods for existing UI until Presentation migrates.

`SaveServiceWalletStore` (optional this task): wrap Load/Save of `EconomySaveData` ↔ `WalletSnapshot` if you split store from service; otherwise EconomyService continues using `ISaveService` internally and register:

```csharp
// MenuSessionRegistrations
builder.Register<EconomyService>(Lifetime.Singleton)
    .As<IEconomyService>()
    .As<IWallet>()
    .As<IWalletLedger>();
```

- [x] **Step 4: MatchRewardHandler uses IWalletLedger**

```csharp
// Inject IWalletLedger _ledger
_ledger.Credit(CurrencyId.Coins, reward, won ? "match_win" : "match_loss");
```

Do **not** resolve economy from match scope. Handler must be session-scoped or receive ledger from session parent.

- [x] **Step 5: Build + test**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter="FullyQualifiedName~EconomyWalletBridgeTests|FullyQualifiedName~WalletPort" -nologo
```

Expected: PASS. Manual smoke: login path no longer throws missing `IEconomyService` (session installer already registers economy).

- [x] **Step 6: Commit**

```bash
git add Assets/_KitchenClash/Application/Services/EconomyService.cs \
  Assets/_KitchenClash/Composition/MenuSessionRegistrations.cs \
  Assets/_KitchenClash/Infrastructure/Services/MatchRewardHandler.cs \
  Assets/_KitchenClash/Infrastructure/Economy/ \
  Assets/Scripts/Tests/EditMode/Gameplay/EconomyWalletBridgeTests.cs
git commit -m "$(cat <<'EOF'
feat(economy): bridge IEconomyService to IWallet/IWalletLedger on session

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 4: Session installer law hardening (docs + guard)

**Files:**
- Modify: `Assets/_KitchenClash/Application/Interfaces/ISessionScopeInstaller.cs` (XML docs)
- Modify: `Assets/_KitchenClash/Infrastructure/DI/SessionManager.cs` — throw if installer null
- Optional create: `Assets/Playcenter/Shell/Runtime/Session/ISessionModuleInstaller.cs` marker with docs pointing to KC installer
- Test: extend existing session DI test or add `SessionManagerInstallerTests.cs`

**Interfaces:**
- Consumes: `ISessionScopeInstaller.Install(IContainerBuilder)`
- Produces: `CreateSession` never builds empty child scope

- [x] **Step 1: Test**

```csharp
[Test]
public void CreateSession_WhenInstallerMissing_ThrowsInvalidOperationException()
{
    // Construct SessionManager with null installer if ctor allows, or unset
    Assert.Throws<InvalidOperationException>(() => sessionManager.CreateSession());
}
```

- [x] **Step 2: Implement guard in SessionManager.CreateSession**

```csharp
if (_sessionScopeInstaller == null)
    throw new InvalidOperationException(
        "ISessionScopeInstaller is required. Register MenuSessionScopeInstaller at root.");
```

- [x] **Step 3: Build + test + commit**

```bash
git commit -m "$(cat <<'EOF'
fix(session): require ISessionScopeInstaller for CreateSession

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 5: Lobby harden — party vs match (EOS sample-grade)

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/EOS/EOSLobbyService.cs`
- Modify: `Assets/Playcenter/Services/Runtime/Session/ILobbyManager.cs` only if API gaps (prefer extend without break)
- Modify: matchmaking / home flow handlers that create lobbies
- Test: `Assets/Scripts/Tests/EditMode/Gameplay/LobbyRoleTests.cs` (fake lobby manager)

**Interfaces:**
- Consumes: `ILobbyManager`, `LobbyType` (Party vs Match), `LobbyOpResult`
- Produces: Party lobby survives match end; match lobby destroyed on return Home; all public methods return `LobbyOpResult` or Task\<LobbyOpResult\> (no raw EOS `Result` in Application/Presentation)

- [x] **Step 1: Document current dual-use in test of fake**

```csharp
public enum LobbyRole { None, Party, Match }

[Test]
public void DestroyMatchLobby_KeepsParty()
{
    var fake = new FakeLobbyManager();
    fake.Create(LobbyType.Party);
    fake.Create(LobbyType.Match);
    fake.Destroy(LobbyType.Match);
    Assert.IsTrue(fake.Has(LobbyType.Party));
    Assert.IsFalse(fake.Has(LobbyType.Match));
}
```

- [x] **Step 2: Implement dual-lobby tracking in EOSLobbyService**

- Keep separate lobby ids: `_partyLobbyId`, `_matchLobbyId`
- Map EOS callbacks → `LobbyOpResult.Ok()` / `Fail(code, message)` at boundary only
- On `ReturnHome` / results exit: leave/destroy match lobby only
- Invite/join party uses party lobby (Brawl Home party panel)

- [x] **Step 3: Align Home PLAY path**

Party exists on Home → PLAY starts matchmaking → on match found create/join **match** lobby → VS/intro → net start. Do not replace party lobby id with match lobby id.

- [x] **Step 4: Manual checklist (EditMode cannot fully cover EOS)**

1. Solo boot → Home  
2. Create party (or implicit solo party)  
3. PLAY → MM → match → results → Home  
4. Assert party still valid / can re-queue without full re-login  

- [x] **Step 5: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(lobby): separate party and match lobbies EOS-sample style

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 6: INetSession + INetTransportConfigurator ports

**Files:**
- Create: `Assets/Playcenter/Services/Runtime/Net/NetRole.cs`
- Create: `Assets/Playcenter/Services/Runtime/Net/INetSession.cs`
- Create: `Assets/Playcenter/Services/Runtime/Net/INetTransportConfigurator.cs`
- Test: `Assets/Scripts/Tests/EditMode/Playcenter/Net/NetSessionPortTests.cs`

**Interfaces:**
- Produces:

```csharp
namespace Playcenter.Services
{
    public enum NetRole { Host, Client }

    public interface INetTransportConfigurator
    {
        void ConfigureForSession(NetRole role, string sessionToken);
    }

    public interface INetSession
    {
        bool IsActive { get; }
        NetRole? ActiveRole { get; }
        System.Threading.Tasks.Task StartAsync(NetRole role, string sessionToken, System.Threading.CancellationToken ct = default);
        System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken ct = default);
    }
}
```

- [x] **Step 1–4:** Same TDD pattern as Task 2 (fake in-memory session: Start sets IsActive, Stop clears). Commit `feat(services): add INetSession ports`.

---

### Task 7: NgoEosNetSession adapter + GameStarter delegation

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Network/NgoEosNetSession.cs`
- Create: `Assets/_KitchenClash/Infrastructure/Network/EosTransportConfigurator.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/GameStarter.cs`
- Modify: `MenuSessionRegistrations` or match scope — register `INetSession` at SESSION (lives across match load) or MATCH if only needed in-match; **prefer SESSION** so results can Stop cleanly
- Reference: sample `EOSTransport.cs` for configure pattern — **copy needed logic into KitchenClash Infrastructure**, do not reference Samples assembly from production code if avoidable
- Test: `NetSessionTests` with fake NetworkManager wrapper if pure logic extracted; otherwise thin adapter + integration checklist

**Interfaces:**
- Consumes: Task 6 ports; NGO `NetworkManager`; EOS transport
- Produces: `StartAsync(Host)` ≡ former `StartHost` path; `StartAsync(Client)` ≡ `StartClient`; `StopAsync` shuts down NGO

- [x] **Step 1: Extract start/stop into NgoEosNetSession**

```csharp
public sealed class NgoEosNetSession : INetSession
{
    private readonly NetworkManager _nm; // injected from match context or setter when scene ready
    private readonly INetTransportConfigurator _transport;
    public bool IsActive { get; private set; }
    public NetRole? ActiveRole { get; private set; }

    public async Task StartAsync(NetRole role, string sessionToken, CancellationToken ct = default)
    {
        _transport.ConfigureForSession(role, sessionToken);
        bool ok = role == NetRole.Host ? _nm.StartHost() : _nm.StartClient();
        if (!ok) throw new InvalidOperationException("NGO start failed");
        IsActive = true;
        ActiveRole = role;
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_nm != null && _nm.IsListening) _nm.Shutdown();
        IsActive = false;
        ActiveRole = null;
        await Task.CompletedTask;
    }
}
```

Wire `NetworkManager` when match scene binder runs (setter or `IMatchContext`).

- [x] **Step 2: GameStarter calls `_netSession.StartAsync` instead of direct StartHost/StartClient**

Keep spawn/approval/bot logic in GameStarter after successful start.

- [x] **Step 3: Reconnect v1**

On `IConnectivityService.OnMatchForfeit` / host dropped timeout: `StopAsync` + flow to results/forfeit. No host migration.

- [x] **Step 4: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(net): NGO+EOS INetSession adapter and GameStarter delegation

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 8: Shell UI components on DesignSystem.uss

**Files:**
- Create: `Assets/_KitchenClash/UI/Components/PcButton.uxml`, `PcPanel.uxml`, `PcCurrencyChip.uxml`, `PcPartySlot.uxml` (+ USS under `UI/Components/` or extend DesignSystem.uss)
- Modify: `Assets/_KitchenClash/UI/Styles/DesignSystem.uss` — component classes only
- Modify: `HomeScreen.uxml`, matchmaking, results overlays as needed
- **Do not** edit `theme.uss` dual-brand tokens for this program

**Interfaces:**
- Consumes: existing `BaseUIScreen` / UI Toolkit
- Produces: reusable UXML templates referenced by screens

- [x] **Step 1: Inventory HomeScreen structure** — party row, PLAY CTA, currency chips
- [x] **Step 2: Extract components** with DesignSystem classes (`pc-btn`, `pc-panel`, `pc-chip`, …)
- [x] **Step 3: Rebuild HomeScreen.uxml** using components; keep binding names screens already query (`Q<Button>("play-button")` etc.) **or** update C# queries in same commit
- [x] **Step 4: Visual check in Editor Game view (manual)
- [x] **Step 5: Commit** `feat(ui): DesignSystem shell components and Home redesign` (`68993b1b`)

---

### Task 9: Match lobby / VS / results shell screens

**Files:**
- Modify/create UXML for match lobby, VS intro, results under `Assets/_KitchenClash/UI/Screens/`
- Modify corresponding `BaseUIScreen` subclasses for new element names
- Flow handlers already drive phase order — only presentation

- [x] **Step 1:** Match lobby shows teams from match lobby members (not party-only)
- [x] **Step 2:** VS splash uses DesignSystem motion-safe layout
- [x] **Step 3:** Results shows rewards from session wallet balances after ledger credit
- [x] **Step 4:** Commit `feat(ui): match lobby VS results on DesignSystem` (`61277f65`)

---

### Task 10: IGameplayInput + ISettingsService

**Files:**
- Create ports in `Playcenter.Services` (`IGameplayInput`, `ISettingsService`, `GameSettings`)
- Create KC adapters: `GameplayInputService`, `PlayerPrefsSettingsService`
- Register ROOT (settings) / MATCH or ROOT (input)
- Test: settings round-trip unit test; input mapping table test (keyboard WASD + mobile virtual stick normalized vector)

**Port sketches:**

```csharp
public sealed class GameSettings
{
    public float MasterVolume { get; set; } = 1f;
    public float MusicVolume { get; set; } = 1f;
    public float SfxVolume { get; set; } = 1f;
    public bool ReduceMotion { get; set; }
    public string LanguageCode { get; set; } = "en";
}

public interface ISettingsService
{
    GameSettings Current { get; }
    System.Threading.Tasks.Task LoadAsync(System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task SaveAsync(System.Threading.CancellationToken ct = default);
    void Apply(GameSettings settings);
}

public interface IGameplayInput
{
    UnityEngine.Vector2 Move { get; } // NOTE: if this forces UnityEngine into Services, put IGameplayInput in Playcenter.Shell instead
    bool InteractPressed { get; }
    bool AbilityPressed { get; }
}
```

**Critical:** If `Vector2` pulls UnityEngine into Services, define:

```csharp
// Playcenter.Services
public readonly struct InputAxis2 { public float X { get; } public float Y { get; } ... }
public interface IGameplayInput
{
    InputAxis2 Move { get; }
    bool InteractPressed { get; }
    bool AbilityPressed { get; }
}
```

- [x] TDD ports → adapters → wire player controller read path to `IGameplayInput` where local player moves
- [x] Commit `feat(input-settings): gameplay input and settings ports` (`864afeff`)

---

### Task 11: Live-ops ports polish (IAP / Ads / Analytics)

**Files:**
- Review existing `IIAPService`, `IAdsService`, `IAnalyticsService` in Playcenter.Services
- Ensure no game IP in ports; KC adapters only
- Add missing methods from spec §2 if any
- Emit analytics on: boot_gate_offline, login_success, match_start, match_end, wallet_credit, purchase_success/fail
- Presentation must not call EOS/NGO directly — grep and purge

```bash
rg -n "Epic\.|NetworkManager|EOSManager" Assets/_KitchenClash/Presentation --glob '*.cs'
```

Any hits → move to Infrastructure handlers/viewmodels already injected.

- [x] **Step 1:** Grep purge list + fix
- [x] **Step 2:** Analytics event name constants in Playcenter or KC Application
- [x] **Step 3:** Commit `refactor(liveops): analytics hooks and presentation purity` (`70e45110`)

---

### Task 12: Wiki + spec status + architecture memory

**Files:**
- Modify: `wiki/Technical.md`, `wiki/LLM-Rules.md`, `wiki/log.md`
- Modify: `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md` status → implemented / plan linked
- Modify: `Documentation/Architecture/PROJECT_MEMORY.md` if present and still used

- [x] **Step 1:** Document connectivity-first boot, wallet ports, party/match lobby, INetSession, DesignSystem-only UI rule
- [x] **Step 2:** Append `wiki/log.md` entry with date and summary
- [x] **Step 3:** Commit `docs(wiki): Playcenter Client OS runtime laws`

---

## Phase mapping (spec P1–P7)

| Spec phase | Tasks |
|------------|-------|
| P1 Boot + wallet bridge | 1, 2, 3, 4 |
| P2 Lobby harden | 5 |
| P3 INetSession + EOSTransport | 6, 7 |
| P4 UI shell | 8, 9 |
| P5 Input + settings | 10 |
| P6 IAP/ads/analytics + purity | 11 |
| P7 Wiki/docs | 12 |

---

## Self-Review (plan vs spec)

**1. Spec coverage**
- Connectivity-first boot → Task 1  
- Wallet ports + SESSION ledger / no MATCH writes → Tasks 2–3  
- Session installer law → Task 4  
- Party vs match lobby, Brawl flow → Task 5 (+ UI 8–9)  
- INetSession / transport / P2P / no host migration v1 → Tasks 6–7  
- DesignSystem.uss UI → Tasks 8–9  
- Input + settings → Task 10  
- Live-ops + presentation purity → Task 11  
- Wiki → Task 12  
- Multi-game reuse via Playcenter ports → all port tasks  
- Approach A evolve (no mega client) → respected  

**2. Placeholder scan**
- CreateBoot in Task 1 intentionally requires implementer to finish fakes against live `IAppFlow` surface — not a TBD feature; must be completed in Task 1 Step 5.  
- EOS manual checklist is unavoidable without PlayMode EOS harness.  

**3. Type consistency**
- `CurrencyId`, `IWallet`, `IWalletLedger`, `IWalletStore`, `WalletSnapshot` consistent Tasks 2–3  
- `NetRole`, `INetSession`, `INetTransportConfigurator` consistent Tasks 6–7  
- Async on Services = `Task` throughout  
- `ISessionScopeInstaller` remains KC law; Playcenter marker optional only  

---

## Execution notes for agents

1. Work task-by-task; do not skip TDD on port tasks.  
2. Never `git add -A` — stage only paths listed in the task commit step.  
3. If wiki contradicts an implementation choice mid-flight → DRIFT WARNING per `wiki/DRIFT-PROTOCOL.md`, wait for user.  
4. Economy DI fix (`ISessionScopeInstaller` / `MenuSessionRegistrations`) may already be in working tree — fold into Task 4 commit if not yet committed, still without unrelated WIP.  
5. After Task 1+3, verify in Editor: guest login → Home without `VContainerException IEconomyService`.  
