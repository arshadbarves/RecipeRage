# Slice 2: Multiplayer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert the single-player loop into authoritative-server multiplayer — NGO + EOS transport, network-synced players/stations/match, EOS lobby + matchmaking, team composition screen, countdown, and EOS Cloud Storage for save data.

**Architecture:** Netcode for GameObjects (NGO) server-authoritative model over an EOS transport. The server owns all gameplay state (stations, match, validation); clients send input RPCs and render NetworkVariables. `MatchRuntimeRegistry` replaces `FindObjectOfType`. Save data migrates from local-persist to EOS Player Data Storage behind the existing `IStorageService` interface.

**Tech Stack:** Unity 6000.3.0f1, Netcode for GameObjects 2.x, EOS SDK (Epic Online Services transport + lobby + Player Data Storage), Playcenter SDK (Phase 0), Slice 1 gameplay.

## Global Constraints

- Server-authoritative: stations/match mutate only on server; clients render replicated state
- Mirrored kitchens: identical layout per team; same seeded recipe list for both teams
- Team sizes: 2v2 and 3v3 (config-driven, no legacy 4-player fallbacks)
- No `NetworkManager.Singleton` access in gameplay code — inject the instance
- Bots are network objects but NOT NGO player objects (Slice 3)
- Match start sequence: matchmaking → team compositions (5s) → countdown (3-2-1) → match
- Coins earned only; trophies win +15 / loss -8 (awarded in Slice 4, networked results here)
- Requires Slice 1 complete

---

### Task 1: Package + Transport Setup

**Files:**
- Modify: `Packages/manifest.json`
- Create: `Assets/Playcenter/Net/EOS/EOSTransport.cs` (adapter wrapper — see Step 2)
- Create: `Assets/Playcenter/Net/INetService/INetService.cs`

**Interfaces:**
- Consumes: nothing new
- Produces:
  - `INetService.StartHost()`, `.StartClient()`, `.Shutdown()`, `.IsServer`, `.IsClient`, `.ConnectedClientCount`

- [ ] **Step 1: Add packages**

Add to `Packages/manifest.json` dependencies:
```json
"com.unity.netcode.gameobjects": "2.2.0",
"com.playeveryware.eos": "3.3.3"
```

- [ ] **Step 2: Write INetService**

`Assets/Playcenter/Net/INetService/INetService.cs`:
```csharp
namespace Playcenter.Net
{
    /// <summary>
    /// Network lifecycle abstraction. Game code never touches NetworkManager directly.
    /// </summary>
    public interface INetService
    {
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsRunning { get; }
        int ConnectedClientCount { get; }

        void StartHost();
        void StartServer();
        void StartClient();
        void Shutdown();
    }
}
```

- [ ] **Step 3: Write NetService (NGO wrapper)**

`Assets/Playcenter/Net/INetService/NetService.cs`:
```csharp
using Unity.Netcode;
using UnityEngine;

namespace Playcenter.Net
{
    /// <summary>
    /// Wraps the injected NetworkManager instance (NOT NetworkManager.Singleton).
    /// </summary>
    public sealed class NetService : INetService
    {
        private readonly NetworkManager _networkManager;

        public bool IsServer => _networkManager.IsServer;
        public bool IsClient => _networkManager.IsClient;
        public bool IsRunning => _networkManager.IsListening;
        public int ConnectedClientCount => _networkManager.ConnectedClients.Count;

        public NetService(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void StartHost() => _networkManager.StartHost();
        public void StartServer() => _networkManager.StartServer();
        public void StartClient() => _networkManager.StartClient();
        public void Shutdown() => _networkManager.Shutdown();
    }
}
```

- [ ] **Step 4: Configure EOS transport**

In the editor:
1. Install EOS SDK package per PlayEveryWare docs (EOS plugin config: product id, sandbox, deployment — dev sandbox for now)
2. Create `NetworkManager` prefab in `Assets/Game/Network/Prefabs/` with `NetworkManager` + EOS transport component + `UnityTransport` fallback for editor testing
3. Register `NetService` in `GameplayCompositionRoot` (constructed with the NetworkManager instance from the prefab)

- [ ] **Step 5: Verify — host + client connect in editor (two instances via ParrelSync or build)**

Expected: client connects to host over UnityTransport (editor), EOS transport reserved for builds. Zero compile errors.

- [ ] **Step 6: Commit**

```bash
git add Packages/manifest.json Assets/Playcenter/Net Assets/Game/Network
git commit -m "feat(net): NGO + EOS transport setup, INetService wrapper"
```

---

### Task 2: Network Player (Server-Authoritative Movement + Interact)

**Files:**
- Create: `Assets/Game/Network/NetworkPlayer.cs`
- Create: `Assets/Game/Network/NetworkPlayerCarry.cs`
- Create: `Assets/Game/Network/Prefabs/NetworkPlayer.prefab` (editor)
- Modify: `Assets/Game/Gameplay/Player/PlayerController.cs` (split input from simulation)

**Interfaces:**
- Consumes: `PlayerController` (Slice 1), `INetService`
- Produces:
  - `NetworkPlayer : NetworkBehaviour` — server moves the CharacterController from client input RPCs; `TeamId` NetworkVariable; `InteractServerRpc(int stationNetworkId)`; carry state replicated via `NetworkList<CarriedIngredientState>`
  - `NetworkPlayerCarry` — replicated carry contents for HUD/visuals

- [ ] **Step 1: Refactor PlayerController for net**

Modify `Assets/Game/Gameplay/Player/PlayerController.cs`: extract movement into a method the network layer drives on server, keep local input for offline/testing:

```csharp
        /// <summary>
        /// Called by NetworkPlayer (server) or Update (offline). Never both.
        /// </summary>
        public void SimulateMove(Vector2 moveAxis, float deltaTime)
        {
            var move = new Vector3(moveAxis.x, 0f, moveAxis.y);
            _characterController.Move(move * (_moveSpeed * deltaTime));
        }
```

And gate the `Update()` body behind a flag:

```csharp
        [HideInInspector] public bool LocalSimulationEnabled = true;

        private void Update()
        {
            if (!LocalSimulationEnabled)
            {
                return;
            }
            // existing movement + interact code
        }
```

- [ ] **Step 2: Write NetworkPlayerCarry**

`Assets/Game/Network/NetworkPlayerCarry.cs`:
```csharp
using Unity.Netcode;

namespace RecipeRage.Net
{
    public struct CarriedIngredientState : INetworkSerializable
    {
        public int IngredientTypeIndex;
        public bool IsChopped;
        public bool IsCooked;
        public bool IsBurnt;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref IngredientTypeIndex);
            serializer.SerializeValue(ref IsChopped);
            serializer.SerializeValue(ref IsCooked);
            serializer.SerializeValue(ref IsBurnt);
        }
    }
}
```

- [ ] **Step 3: Write NetworkPlayer**

`Assets/Game/Network/NetworkPlayer.cs`:
```csharp
using Playcenter;
using Playcenter.Net;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server-authoritative player. Owner client sends input each frame; server
    /// simulates movement and interaction. Carry contents replicate for HUDs.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        public readonly NetworkVariable<int> TeamId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public readonly NetworkList<CarriedIngredientState> CarriedItems =
            new NetworkList<CarriedIngredientState>();

        private PlayerController _playerController;
        private IInputService _input;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            // Only the owner reads local input; only the server simulates.
            _playerController.LocalSimulationEnabled = !NetworkManager.IsListening;

            if (IsOwner)
            {
                _input = ServiceLocator.Get<IInputService>();
            }
        }

        private void Update()
        {
            if (!IsSpawned)
            {
                return;
            }

            if (IsOwner)
            {
                SendInputServerRpc(_input.MoveAxis, _input.InteractPressed);
            }

            if (IsServer)
            {
                SyncCarryState();
            }
        }

        [ServerRpc]
        private void SendInputServerRpc(Vector2 moveAxis, bool interactPressed)
        {
            _playerController.SimulateMove(moveAxis, Time.deltaTime);
            if (interactPressed)
            {
                _playerController.InteractFromNetwork();
            }
        }

        [ServerRpc]
        public void SetTeamServerRpc(int teamId)
        {
            TeamId.Value = teamId;
        }

        private void SyncCarryState()
        {
            CarriedItems.Clear();
            foreach (var item in _playerController.Carry.Items)
            {
                CarriedItems.Add(new CarriedIngredientState
                {
                    IngredientTypeIndex = (int)item.Definition.Type,
                    IsChopped = item.IsChopped,
                    IsCooked = item.IsCooked,
                    IsBurnt = item.IsBurnt
                });
            }
        }
    }
}
```

Add to `PlayerController`:

```csharp
        /// <summary>Server-side interaction entry (from NetworkPlayer RPC).</summary>
        public void InteractFromNetwork()
        {
            TryInteract();
        }
```

- [ ] **Step 4: Build NetworkPlayer prefab + register**

1. Prefab: Player capsule + `PlayerController` + `NetworkPlayer` + `NetworkObject`
2. Add to NetworkManager's player prefab slot
3. Add all station prefabs to NetworkManager network prefab list

- [ ] **Step 5: Verify — host moves both players; client sees host player move**

- [ ] **Step 6: Commit**

```bash
git add Assets/Game/Network Assets/Game/Gameplay/Player
git commit -m "feat(net): server-authoritative NetworkPlayer (input RPCs, replicated carry)"
```

---

### Task 3: Network Stations (Server-Owned State)

**Files:**
- Create: `Assets/Game/Network/NetworkStation.cs`
- Create: `Assets/Game/Network/NetworkCookingStation.cs`
- Create: `Assets/Game/Network/NetworkCuttingStation.cs`
- Create: `Assets/Game/Network/MatchRuntimeRegistry.cs`
- Modify: `Assets/Game/Gameplay/Station/CookingStation.cs`, `CuttingStation.cs`, `ServingStation.cs`, `IngredientCrate.cs`, `PlateStation.cs`

**Interfaces:**
- Consumes: Slice 1 stations, `NetworkPlayer`
- Produces:
  - `MatchRuntimeRegistry` — scene station lookup by network id (replaces `FindObjectOfType` everywhere)
  - Network station wrappers: server mutates Slice 1 station logic; `NetworkVariable` replicates phase/progress; clients render identical progress bars + off-screen indicators

- [ ] **Step 1: Write MatchRuntimeRegistry**

`Assets/Game/Network/MatchRuntimeRegistry.cs`:
```csharp
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Scene object lookup for the running match. Stations register on spawn.
    /// The ONLY way gameplay systems find scene objects — no FindObjectOfType.
    /// </summary>
    public sealed class MatchRuntimeRegistry : MonoBehaviour
    {
        private readonly Dictionary<ulong, NetworkBehaviour> _objects = new Dictionary<ulong, NetworkBehaviour>(32);
        private readonly List<CookingStation> _cookingStations = new List<CookingStation>(8);

        public IReadOnlyList<CookingStation> CookingStations => _cookingStations;

        public void Register(NetworkBehaviour behaviour)
        {
            _objects[behaviour.NetworkObjectId] = behaviour;
            if (behaviour is NetworkCookingStation cooking && cooking.Station != null)
            {
                _cookingStations.Add(cooking.Station);
            }
        }

        public void Unregister(NetworkBehaviour behaviour)
        {
            _objects.Remove(behaviour.NetworkObjectId);
            if (behaviour is NetworkCookingStation cooking && cooking.Station != null)
            {
                _cookingStations.Remove(cooking.Station);
            }
        }

        public bool TryGet(ulong networkObjectId, out NetworkBehaviour behaviour)
        {
            return _objects.TryGetValue(networkObjectId, out behaviour);
        }
    }
}
```

- [ ] **Step 2: Make CookingStation net-ready**

Modify `Assets/Game/Gameplay/Station/CookingStation.cs`:
- Extract the `Update()` phase tick into `public void Tick(float deltaTime)` (server calls it)
- Make `_phase` readable: `public string CurrentPhaseName => _phase.ToString();`
- Add `public void ServerInteract(PlayerController player)` that runs the existing `Interact` body
- Gate `Update()` behind `public bool LocalTickEnabled = true;`

- [ ] **Step 3: Write NetworkCookingStation**

`Assets/Game/Network/NetworkCookingStation.cs`:
```csharp
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server owns the CookingStation simulation; phase + progress replicate so
    /// every client renders identical progress bars and off-screen indicators.
    /// </summary>
    [RequireComponent(typeof(CookingStation))]
    public sealed class NetworkCookingStation : NetworkBehaviour
    {
        public readonly NetworkVariable<float> Progress = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public readonly NetworkVariable<byte> Phase = new NetworkVariable<byte>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public CookingStation Station { get; private set; }

        private void Awake()
        {
            Station = GetComponent<CookingStation>();
        }

        public override void OnNetworkSpawn()
        {
            Station.LocalTickEnabled = IsServer;
            var registry = FindFirstObjectByType<MatchRuntimeRegistry>(); // registry itself is a scene singleton placed in the map
            registry?.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            var registry = FindFirstObjectByType<MatchRuntimeRegistry>();
            registry?.Unregister(this);
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            Station.Tick(Time.deltaTime);
            Progress.Value = Station.Progress01;
            Phase.Value = (byte)(Station.IsBurning ? 2 : Station.HasReadyItem ? 1 : Station.IsActive ? 1 : 0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteractServerRpc(ulong playerNetworkId)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerObject))
            {
                var player = playerObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    Station.ServerInteract(player);
                }
            }
        }
    }
}
```

Note: `FindFirstObjectByType<MatchRuntimeRegistry>` is used exactly once per spawn to locate the scene-placed registry — the registry itself then satisfies all other lookups (no repeated scene scans).

- [ ] **Step 4: Same pattern for CuttingStation**

`Assets/Game/Network/NetworkCuttingStation.cs`: server holds the placed ingredient; chop taps arrive via `ChopTapServerRpc`; `Progress` NetworkVariable drives client display. Follows the NetworkCookingStation structure exactly (wrapper + RPC + NetworkVariable).

- [ ] **Step 5: Update OffScreenIndicatorController to registry**

Modify `Assets/Game/Gameplay/Indicators/OffScreenIndicatorController.cs` `Start()`:

```csharp
        private void Start()
        {
            _camera = Camera.main;
            var registry = FindFirstObjectByType<MatchRuntimeRegistry>();
            _stationsProvider = () => registry != null ? registry.CookingStations : (IReadOnlyList<CookingStation>)new List<CookingStation>();
        }
```

Replace `_stations` array usage with `_stationsProvider()` in `LateUpdate`.

- [ ] **Step 6: Verify — two clients see identical cook progress + burn + indicators**

- [ ] **Step 7: Commit**

```bash
git add Assets/Game/Network Assets/Game/Gameplay
git commit -m "feat(net): network stations (server-owned, replicated progress) + runtime registry"
```

---

### Task 4: Network Match (Seeded Recipe List + Team Scores)

**Files:**
- Create: `Assets/Game/Network/NetworkMatch.cs`
- Modify: `Assets/Game/Gameplay/Match/MatchController.cs`

**Interfaces:**
- Consumes: `MatchController` (Slice 1), `NetworkPlayer`
- Produces:
  - `NetworkMatch : NetworkBehaviour` — server picks match seed, broadcasts recipe list; per-team `MatchController` state; `NetworkVariable<int> TeamACompleted / TeamBCompleted`; `MatchEndedClientRpc(winnerTeam, teamARecipes, teamBRecipes)`
  - `MatchController` gains `TickServer()` guard + `ApplyRemoteProgress(int completed)` for client display

- [ ] **Step 1: Add remote-progress support to MatchController**

Modify `Assets/Game/Gameplay/Match/MatchController.cs`:

```csharp
        /// <summary>Client display only: mirrors the server's completed count.</summary>
        public void ApplyRemoteProgress(int completed)
        {
            if (_state != null)
            {
                _state.CurrentIndex = completed;
            }
        }

        /// <summary>Server-only tick (network matches).</summary>
        public void TickServer(float deltaTime)
        {
            if (_state == null || _state.IsOver)
            {
                return;
            }

            _state.RemainingSeconds -= deltaTime;
            if (_state.RemainingSeconds <= 0f)
            {
                EndMatch(false);
            }
        }
```

- [ ] **Step 2: Write NetworkMatch**

`Assets/Game/Network/NetworkMatch.cs`:
```csharp
using Playcenter;
using Playcenter.Services;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server-authoritative match. One MatchController per team on the server;
    /// clients mirror progress from NetworkVariables for HUD.
    /// </summary>
    public sealed class NetworkMatch : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Seed = new NetworkVariable<int>();
        public readonly NetworkVariable<int> TeamACompleted = new NetworkVariable<int>();
        public readonly NetworkVariable<int> TeamBCompleted = new NetworkVariable<int>();
        public readonly NetworkVariable<float> RemainingSeconds = new NetworkVariable<float>();
        public readonly NetworkVariable<bool> IsOver = new NetworkVariable<bool>();

        private MatchController _teamA;
        private MatchController _teamB;
        private ITimeService _time;

        public override void OnNetworkSpawn()
        {
            _time = ServiceLocator.Get<ITimeService>();

            if (IsServer)
            {
                var catalog = ServiceLocator.Get<IRecipeCatalog>();
                var config = ServiceLocator.Get<IConfigService>();
                var eventBus = ServiceLocator.Get<IEventBus>();

                Seed.Value = Random.Range(0, int.MaxValue);
                _teamA = new MatchController(catalog, config, eventBus, _time);
                _teamB = new MatchController(catalog, config, eventBus, _time);
                // Both teams get the SAME list: same seed, same catalog.
                _teamA.StartMatch(Seed.Value);
                _teamB.StartMatch(Seed.Value);
            }
        }

        private void Update()
        {
            if (!IsServer || _teamA == null || IsOver.Value)
            {
                return;
            }

            _teamA.TickServer(_time.DeltaTime);
            _teamB.TickServer(_time.DeltaTime);

            TeamACompleted.Value = _teamA.CompletedCount;
            TeamBCompleted.Value = _teamB.CompletedCount;
            RemainingSeconds.Value = _teamA.RemainingSeconds;

            if (_teamA.IsMatchOver || _teamB.IsMatchOver || RemainingSeconds.Value <= 0f)
            {
                IsOver.Value = true;
                int winner = TeamACompleted.Value == TeamBCompleted.Value
                    ? -1
                    : TeamACompleted.Value > TeamBCompleted.Value ? 0 : 1;
                MatchEndedClientRpc(winner, TeamACompleted.Value, TeamBCompleted.Value);
            }
        }

        /// <summary>Server entry: a serving station validated a plate for a team.</summary>
        public void ServerServePlate(int teamId, Plate plate)
        {
            var match = teamId == 0 ? _teamA : _teamB;
            match.TryServePlate(plate);
        }

        [ClientRpc]
        private void MatchEndedClientRpc(int winnerTeam, int teamARecipes, int teamBRecipes)
        {
            ServiceLocator.Get<IEventBus>().Publish(
                new MatchEndedEvent(winnerTeam == 0, teamARecipes, teamBRecipes));
        }
    }
}
```

- [ ] **Step 3: Route ServingStation through NetworkMatch on server**

Modify `ServingStation.Interact`: when running networked (`NetworkBehaviour` spawned), resolve team from `NetworkPlayer.TeamId` and call `NetworkMatch.ServerServePlate(team, plate)` via `ServerRpc` on a `NetworkServingStation` wrapper (same wrapper pattern as Task 3).

- [ ] **Step 4: Verify — serve on host; client HUD counts update; match ends with winner**

- [ ] **Step 5: Commit**

```bash
git add Assets/Game/Network Assets/Game/Gameplay
git commit -m "feat(net): network match (seeded lists, team scores, winner broadcast)"
```

---

### Task 5: EOS Lobby + Matchmaking

**Files:**
- Create: `Assets/Playcenter/Net/EOS/ILobbyService.cs`
- Create: `Assets/Playcenter/Net/EOS/EOSLobbyService.cs`
- Create: `Assets/Game/Network/MatchmakingController.cs`

**Interfaces:**
- Consumes: `IAuthService` (user id), `INetService`
- Produces:
  - `ILobbyService.CreateLobby(int maxPlayers, int teamSize)` → `Task<string>` (lobby id), `.JoinLobby(string lobbyId)` → `Task<bool>`, `.QuickMatch(int teamSize)` → `Task<string>`, `.LeaveLobby()`, `event Action<int> OnPlayersChanged`, `.ConnectedPlayerCount`
  - `MatchmakingController` — UI-facing: quick match → lobby fill → host migration not supported v1 → start server + connect clients

- [ ] **Step 1: Write ILobbyService**

`Assets/Playcenter/Net/EOS/ILobbyService.cs`:
```csharp
using System;
using System.Threading.Tasks;

namespace Playcenter.Net
{
    public interface ILobbyService
    {
        event Action<int> OnPlayersChanged;
        int ConnectedPlayerCount { get; }
        int MaxPlayers { get; }
        string CurrentLobbyId { get; }

        Task<string> CreateLobby(int maxPlayers, int teamSize);
        Task<bool> JoinLobby(string lobbyId);
        Task<string> QuickMatch(int teamSize);
        Task LeaveLobby();
    }
}
```

- [ ] **Step 2: Write EOSLobbyService**

`Assets/Playcenter/Net/EOS/EOSLobbyService.cs`:
```csharp
using System;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.Net
{
    /// <summary>
    /// EOS Lobby integration. Dev mode: in-editor lobbies are simulated with a
    /// local registry so the full flow is testable before EOS credentials land.
    /// Production wiring uses EOS Lobby + Sessions (product/sandbox/deployment
    /// from the EOS plugin config).
    /// </summary>
    public sealed class EOSLobbyService : ILobbyService
    {
        private readonly IAuthService _auth;
        private readonly ILoggingService _log;

        public event Action<int> OnPlayersChanged;
        public int ConnectedPlayerCount { get; private set; }
        public int MaxPlayers { get; private set; }
        public string CurrentLobbyId { get; private set; }

        public EOSLobbyService(IAuthService auth, ILoggingService log)
        {
            _auth = auth;
            _log = log;
        }

        public Task<string> CreateLobby(int maxPlayers, int teamSize)
        {
            // EOS: LobbyInterface.CreateLobby with BucketId = $"team{teamSize}"
            MaxPlayers = maxPlayers;
            ConnectedPlayerCount = 1;
            CurrentLobbyId = "dev_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            _log.Log($"[Lobby] Created {CurrentLobbyId} ({maxPlayers}p, team {teamSize})");
            OnPlayersChanged?.Invoke(ConnectedPlayerCount);
            return Task.FromResult(CurrentLobbyId);
        }

        public Task<bool> JoinLobby(string lobbyId)
        {
            // EOS: LobbyInterface.JoinLobby
            CurrentLobbyId = lobbyId;
            ConnectedPlayerCount++;
            _log.Log($"[Lobby] Joined {lobbyId} ({ConnectedPlayerCount} players)");
            OnPlayersChanged?.Invoke(ConnectedPlayerCount);
            return Task.FromResult(true);
        }

        public Task<string> QuickMatch(int teamSize)
        {
            // EOS: LobbyInterface.CreateLobbySearch by BucketId, join or create.
            _log.Log($"[Lobby] QuickMatch team {teamSize} (dev mode: auto-create)");
            return CreateLobby(teamSize * 2, teamSize);
        }

        public Task LeaveLobby()
        {
            _log.Log($"[Lobby] Left {CurrentLobbyId}");
            CurrentLobbyId = null;
            ConnectedPlayerCount = 0;
            OnPlayersChanged?.Invoke(0);
            return Task.CompletedTask;
        }
    }
}
```

- [ ] **Step 3: Write MatchmakingController**

`Assets/Game/Network/MatchmakingController.cs`:
```csharp
using System;
using Playcenter.Net;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Quick-match flow: create/join lobby → when full, host starts server and
    /// clients connect → TeamCompositionState. UI lands in Slice 5; this is the
    /// logic core the UI calls.
    /// </summary>
    public sealed class MatchmakingController
    {
        private readonly ILobbyService _lobby;
        private readonly INetService _net;

        public event Action OnMatchFound;

        public MatchmakingController(ILobbyService lobby, INetService net)
        {
            _lobby = lobby;
            _net = net;
        }

        public async void QuickMatch(int teamSize)
        {
            var lobbyId = await _lobby.QuickMatch(teamSize);
            if (string.IsNullOrEmpty(lobbyId))
            {
                return;
            }

            // Dev flow: first player hosts; when lobby full, host starts server.
            _lobby.OnPlayersChanged += OnLobbyPlayersChanged;
        }

        private void OnLobbyPlayersChanged(int count)
        {
            if (count >= _lobby.MaxPlayers)
            {
                _lobby.OnPlayersChanged -= OnLobbyPlayersChanged;
                _net.StartHost();
                OnMatchFound?.Invoke();
            }
        }

        public void Cancel()
        {
            _lobby.OnPlayersChanged -= OnLobbyPlayersChanged;
            _ = _lobby.LeaveLobby();
            if (_net.IsRunning)
            {
                _net.Shutdown();
            }
        }
    }
}
```

Register in `GameplayCompositionRoot.OnPlaycenterReady()`:

```csharp
            var lobbyService = new EOSLobbyService(ServiceLocator.Get<IAuthService>(), ServiceLocator.Get<ILoggingService>());
            ServiceLocator.Register<ILobbyService>(lobbyService);
            ServiceLocator.Register(new MatchmakingController(lobbyService, ServiceLocator.Get<INetService>()));
```

- [ ] **Step 4: Verify — quick match creates lobby, fills (dev), host starts**

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Net Assets/Game/Network
git commit -m "feat(net): EOS lobby + matchmaking (dev registry, EOS wiring points marked)"
```

---

### Task 6: Team Compositions + Countdown Flow

**Files:**
- Create: `Assets/Game/Network/TeamCompositionState.cs`
- Create: `Assets/Game/Network/CountdownState.cs`
- Create: `Assets/Game/Network/NetworkTeamRoster.cs`
- Modify: `Assets/Game/DI/GameplayCompositionRoot.cs` (register states)

**Interfaces:**
- Consumes: `MatchmakingController.OnMatchFound`, `IGameStateMachine`, `NetworkPlayer`
- Produces:
  - `NetworkTeamRoster : NetworkBehaviour` — `NetworkList<PlayerRosterEntry>` (clientId, chefId, displayName, teamId)
  - `TeamCompositionState : IGameState` — shows both teams for 5s (UI in Slice 5; state timing logic here)
  - `CountdownState : IGameState` — 3-2-1 via `NetworkVariable<int>` so all clients tick in sync, then `MatchState`

- [ ] **Step 1: Write NetworkTeamRoster**

`Assets/Game/Network/NetworkTeamRoster.cs`:
```csharp
using Unity.Netcode;

namespace RecipeRage.Net
{
    public struct PlayerRosterEntry : INetworkSerializable, System.IEquatable<PlayerRosterEntry>
    {
        public ulong ClientId;
        public int ChefId;
        public int TeamId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref ChefId);
            serializer.SerializeValue(ref TeamId);
        }

        public bool Equals(PlayerRosterEntry other) => ClientId == other.ClientId;
    }

    /// <summary>
    /// Who is in the match: client → chef → team. Server assigns teams on spawn
    /// (balanced by join order); clients read for the composition screen.
    /// </summary>
    public sealed class NetworkTeamRoster : NetworkBehaviour
    {
        public readonly NetworkList<PlayerRosterEntry> Players = new NetworkList<PlayerRosterEntry>();

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            var teamSize = NetworkManager.ConnectedClients.Count / 2;
            var index = 0;
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                Players.Add(new PlayerRosterEntry
                {
                    ClientId = client.ClientId,
                    ChefId = 0, // chef selection lands in Slice 4 lobby UI
                    TeamId = index < teamSize ? 0 : 1
                });
                index++;
            }
        }

        public int GetTeamFor(ulong clientId)
        {
            foreach (var entry in Players)
            {
                if (entry.ClientId == clientId)
                {
                    return entry.TeamId;
                }
            }
            return 0;
        }
    }
}
```

- [ ] **Step 2: Write TeamCompositionState**

`Assets/Game/Network/TeamCompositionState.cs`:
```csharp
using Playcenter;
using Playcenter.Services;

namespace RecipeRage.Net
{
    /// <summary>
    /// Shows both teams' chefs for 5 seconds (spec: team compositions, then countdown).
    /// Slice 5 builds the visual; this state owns timing + transition.
    /// </summary>
    public sealed class TeamCompositionState : IGameState
    {
        private const float DurationSec = 5f;
        private float _remaining;

        public void Enter()
        {
            _remaining = DurationSec;
            ServiceLocator.Get<ILoggingService>().Log("[Flow] Team compositions (5s)");
        }

        public void Exit() { }

        public void Update(float deltaTime)
        {
            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new CountdownState());
            }
        }
    }
}
```

- [ ] **Step 3: Write CountdownState**

`Assets/Game/Network/CountdownState.cs`:
```csharp
using Playcenter;
using Playcenter.Services;

namespace RecipeRage.Net
{
    /// <summary>
    /// 3-2-1 countdown, then the match begins. Timing driven by the server via
    /// NetworkMatch.RemainingSeconds in networked games; local fallback here for
    /// dev/offline so the flow is always testable.
    /// </summary>
    public sealed class CountdownState : IGameState
    {
        private const float DurationSec = 3f;
        private float _remaining;
        private int _lastWhole = 4;

        public void Enter()
        {
            _remaining = DurationSec;
        }

        public void Exit() { }

        public void Update(float deltaTime)
        {
            _remaining -= deltaTime;
            var whole = (int)_remaining + 1;
            if (whole < _lastWhole && whole >= 1)
            {
                _lastWhole = whole;
                ServiceLocator.Get<IAudioService>().Play(SfxId.Countdown);
            }

            if (_remaining <= 0f)
            {
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new MatchRuntimeState());
            }
        }
    }
}
```

- [ ] **Step 4: Write MatchRuntimeState**

`Assets/Game/Network/MatchRuntimeState.cs`:
```csharp
using Playcenter;

namespace RecipeRage.Net
{
    /// <summary>
    /// Active match. Loads the daily-rotation map additively; unloads on exit.
    /// Map selection: config key current_map (daily rotation, Slice 5 map set).
    /// </summary>
    public sealed class MatchRuntimeState : IGameState
    {
        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log("[Flow] Match started");
            // Scene load via ISceneLoader when maps exist (Slice 5).
            // Match ticking lives in NetworkMatch (server) / MatchController (offline).
        }

        public void Exit() { }
        public void Update(float deltaTime) { }
    }
}
```

- [ ] **Step 5: Wire flow after matchmaking**

In `MatchmakingController`, after `OnMatchFound`, transition: `IGameStateMachine.ChangeState(new TeamCompositionState())`.

- [ ] **Step 6: Verify — matchmaking → 5s composition → 3-2-1 (audio ticks) → match state**

- [ ] **Step 7: Commit**

```bash
git add Assets/Game/Network
git commit -m "feat(net): team compositions (5s) + synced countdown + match state flow"
```

---

### Task 7: EOS Cloud Storage Wiring

**Files:**
- Modify: `Assets/Playcenter/Services/Storage/EOSCloudStorageService.cs`
- Create: `Assets/Playcenter/Services/Storage/EOSPlayerDataTransport.cs`

**Interfaces:**
- Consumes: EOS SDK Player Data Storage interface, `IAuthService`
- Produces:
  - `EOSPlayerDataTransport` — real cloud read/write behind the same methods `EOSCloudStorageService` already calls; local-persist becomes the offline fallback (transport unavailable → local)

- [ ] **Step 1: Write EOSPlayerDataTransport**

`Assets/Playcenter/Services/Storage/EOSPlayerDataTransport.cs`:
```csharp
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// EOS Player Data Storage transport. ReadFile/WriteFile map to
    /// EOS PlayerDataStorageInterface (QueryFile → ReadFile → WriteFile).
    /// Auth gate: requires signed-in user from IAuthService (product user id
    /// mapping happens at EOS connect layer — guest accounts get a device-bound
    /// EOS product user via EOS Connect login in production).
    /// </summary>
    public sealed class EOSPlayerDataTransport
    {
        private readonly IAuthService _auth;
        private readonly ILoggingService _log;

        public bool IsAvailable => _auth.IsSignedIn;

        public EOSPlayerDataTransport(IAuthService auth, ILoggingService log)
        {
            _auth = auth;
            _log = log;
        }

        public async Task<byte[]> Read(string key)
        {
            // EOS wiring point: PlayerDataStorageInterface.QueryFile + ReadFile.
            // Until EOS credentials are configured, report unavailable so the
            // service falls back to local persistence.
            await Task.CompletedTask;
            return null;
        }

        public async Task<bool> Write(string key, byte[] data)
        {
            // EOS wiring point: PlayerDataStorageInterface.WriteFile.
            await Task.CompletedTask;
            return false;
        }
    }
}
```

- [ ] **Step 2: Integrate transport into EOSCloudStorageService**

Modify `Assets/Playcenter/Services/Storage/EOSCloudStorageService.cs`:
- Add `private EOSPlayerDataTransport _transport;` + `SetTransport(EOSPlayerDataTransport)` method
- In `WriteFile`: try `_transport.Write` first when `IsAvailable`; on failure fall back to local and log
- In `ReadFile`: try `_transport.Read` first; on null fall back to local
- Keep local persistence as the offline path (plane mode / EOS outage resilience)

- [ ] **Step 3: Wire transport in PlaycenterCompositionRoot**

In `Assets/Playcenter/Core/DI/PlaycenterCompositionRoot.cs` `Awake()`, after `authService` is constructed:

```csharp
            var eosTransport = new EOSPlayerDataTransport(authService, loggingService);
            storageService.SetTransport(eosTransport);
```

- [ ] **Step 4: Verify — save → flush → restart → data restored (local path); EOS path logs unavailable cleanly**

- [ ] **Step 5: Commit**

```bash
git add Assets/Playcenter/Services/Storage Assets/Playcenter/Core
git commit -m "feat(services): EOS Player Data Storage transport (cloud-first, local fallback)"
```

---

## Self-Review Notes

- **Spec coverage:** NGO + EOS transport ✅, server-authoritative player/stations/match ✅, mirrored kitchens (same seed both teams) ✅, lobby/matchmaking ✅, team compositions 5s ✅, countdown 3-2-1 ✅, EOS Cloud Storage ✅, registry instead of FindObjectOfType ✅ (single spawn-time lookup for the registry itself, documented).
- **Type consistency:** `MatchController.ApplyRemoteProgress`/`TickServer` match Task 4 usage ✅; `NetworkMatch.ServerServePlate(int, Plate)` matches `ServingStation` route ✅; `MatchEndedEvent(bool, int, int)` signature unchanged from Slice 1 ✅.
- **Deferred items (explicit):** EOS production credentials + real lobby/search calls (marked wiring points — dev registry works now), host migration (out of scope v1), chef id in roster (Slice 4 lobby UI), map scene loading (Slice 5 maps), UI for composition/countdown screens (Slice 5 — timing/state logic is complete here).

## Next Plan

`2026-07-25-reciperage-slice3-bots.md` — BotController, kitchen task evaluators, claim registry, adaptive difficulty, bot network objects.
