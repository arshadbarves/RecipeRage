using System.Collections;
using System.Collections.Generic;
using KitchenClash.Application.Config;
using KitchenClash.Domain;
using KitchenClash.Domain.Enums;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// v2 combat subsystem attached to the same GameObject as PlayerController.
    ///
    /// Responsibilities:
    ///   • HP tracking (RC-keyed per archetype)
    ///   • KO detection and respawn (3s default via ko_respawn_sec)
    ///   • On KO: drop 100 % of carried items as LootPickup objects (server-only)
    ///   • Knockback impulse replication
    ///
    /// Server-authoritative. All mutating calls require IsServer.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerCombatController : NetworkBehaviour
    {
        // ── Inspector ──
        [Header("KO Settings")]
        [SerializeField] private GameObject _koFxPrefab;
        [SerializeField] private GameObject _respawnFxPrefab;

        [Header("Loot Drop")]
        [SerializeField] private GameObject _lootPickupPrefab;   // LootPickup prefab registered in NetworkObjectPool

        // ── Injected ──
        [Inject] private IConfigService _cfg;
        [Inject] private IEventBus      _eventBus;
        [Inject] private INetworkObjectPool _pool;

        // ── Sibling ref ──
        private PlayerController _player;

        // ── Network state ──
        private readonly NetworkVariable<int>  _currentHp =
            new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int>  _maxHp =
            new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _isDead =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Server-only melee cadence (not replicated — clients only request attacks).
        private float _nextMeleeTime;

        // ── Public accessors ──
        public int  CurrentHp  => _currentHp.Value;
        public int  MaxHp      => _maxHp.Value;
        public bool IsDead     => _isDead.Value;
        public bool IsAlive    => !_isDead.Value;

        // ─────────────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────────────

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _player = GetComponent<PlayerController>();

            _currentHp.OnValueChanged += (_, hp) => OnHpChanged(hp);
            _isDead.OnValueChanged    += (_, dead) => OnDeadChanged(dead);

            if (IsServer)
                InitializeHp();
        }

        // ─────────────────────────────────────────────────────────────────
        // HP initialisation (server-only, called after chef archetype is set)
        // ─────────────────────────────────────────────────────────────────

        public void InitializeHp()
        {
            // Default to 100; archetype-specific values set via SetArchetypeHp after chef selection
            int hp = _cfg.Get(RemoteConfigKeys.RusherHpBase, RemoteConfigKeys.Defaults.RusherHpBase);
            _maxHp.Value     = hp;
            _currentHp.Value = hp;
        }

        /// <summary>
        /// Called by the chef-selection flow after archetype is confirmed (server-only).
        /// </summary>
        public void SetArchetypeHp(ChefArchetype archetype)
        {
            if (!IsServer) return;

            int hp = archetype switch
            {
                ChefArchetype.Rusher     => _cfg.Get(RemoteConfigKeys.RusherHpBase,     RemoteConfigKeys.Defaults.RusherHpBase),
                ChefArchetype.Cook       => _cfg.Get(RemoteConfigKeys.CookHpBase,       RemoteConfigKeys.Defaults.CookHpBase),
                ChefArchetype.Controller => _cfg.Get(RemoteConfigKeys.ControllerHpBase, RemoteConfigKeys.Defaults.ControllerHpBase),
                ChefArchetype.Disruptor  => _cfg.Get(RemoteConfigKeys.DisruptorHpBase,  RemoteConfigKeys.Defaults.DisruptorHpBase),
                _                        => RemoteConfigKeys.Defaults.RusherHpBase,
            };

            _maxHp.Value     = hp;
            _currentHp.Value = hp;
        }

        // ─────────────────────────────────────────────────────────────────
        // Damage API (server-only)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Apply damage. Triggers KO sequence when HP reaches 0.
        /// </summary>
        public void TakeDamage(int amount, ulong attackerClientId = ulong.MaxValue)
        {
            if (!IsServer) return;
            if (_isDead.Value) return;
            if (amount <= 0) return;

            _currentHp.Value = Mathf.Max(0, _currentHp.Value - amount);

            if (_currentHp.Value == 0)
                StartKo(attackerClientId);
        }

        /// <summary>
        /// Local-player entry: request a basic melee swing (server-authoritative).
        /// Bound to Attack input (LMB / J / mobile Attack button).
        /// </summary>
        public void RequestMeleeAttack()
        {
            if (!IsOwner || _isDead.Value)
            {
                return;
            }

            MeleeAttackServerRpc();
        }

        [ServerRpc]
        private void MeleeAttackServerRpc()
        {
            TryMeleeAttackServer();
        }

        /// <summary>
        /// Server-side basic melee: nearest living enemy within melee_attack_range_tiles.
        /// Uses IPlayerNetworkManager when available (no scene-wide FindObjects).
        /// </summary>
        public void TryMeleeAttackServer()
        {
            if (!IsServer || _player == null || _isDead.Value)
            {
                return;
            }

            float cooldown = _cfg != null
                ? _cfg.Get(RemoteConfigKeys.MeleeAttackCooldown, RemoteConfigKeys.Defaults.MeleeAttackCooldown)
                : RemoteConfigKeys.Defaults.MeleeAttackCooldown;

            if (Time.time < _nextMeleeTime)
            {
                return;
            }

            float range = _cfg != null
                ? _cfg.Get(RemoteConfigKeys.MeleeAttackRange, RemoteConfigKeys.Defaults.MeleeAttackRange)
                : RemoteConfigKeys.Defaults.MeleeAttackRange;

            int damage = _cfg != null
                ? _cfg.Get(RemoteConfigKeys.MeleeAttackDamage, RemoteConfigKeys.Defaults.MeleeAttackDamage)
                : RemoteConfigKeys.Defaults.MeleeAttackDamage;

            PlayerCombatController target = FindNearestEnemyCombat(range);
            _nextMeleeTime = Time.time + Mathf.Max(0.05f, cooldown);

            if (target == null)
            {
                return;
            }

            target.TakeDamage(damage, _player.OwnerClientId);
            GameLogger.Log(
                $"[CombatController] Melee hit client {target.OwnerClientId} for {damage} dmg " +
                $"(range={range:F2})");
        }

        private PlayerCombatController FindNearestEnemyCombat(float range)
        {
            PlayerCombatController nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 origin = transform.position;

            // Prefer registered players (bots that are NGO player objects + humans).
            IPlayerNetworkManager playerManager = ResolvePlayerNetworkManager();
            if (playerManager != null)
            {
                IReadOnlyList<IPlayerController> players = playerManager.GetAllPlayers();
                for (int i = 0; i < players.Count; i++)
                {
                    IPlayerController other = players[i];
                    if (other == null || other.OwnerClientId == _player.OwnerClientId)
                    {
                        continue;
                    }

                    if (other.TeamId == _player.TeamId)
                    {
                        continue;
                    }

                    if (other is not PlayerController pc)
                    {
                        continue;
                    }

                    PlayerCombatController combat = pc.GetComponent<PlayerCombatController>();
                    if (combat == null || combat.IsDead)
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(origin, pc.transform.position);
                    if (dist <= range && dist < nearestDist)
                    {
                        nearest = combat;
                        nearestDist = dist;
                    }
                }
            }

            // Bots may not be NGO player objects — also scan spawned combat controllers
            // under the same NetworkManager without FindObjectsByType.
            if (NetworkManager?.SpawnManager?.SpawnedObjects != null)
            {
                foreach (KeyValuePair<ulong, NetworkObject> kvp in NetworkManager.SpawnManager.SpawnedObjects)
                {
                    NetworkObject netObj = kvp.Value;
                    if (netObj == null || !netObj.TryGetComponent(out PlayerCombatController combat))
                    {
                        continue;
                    }

                    if (combat == this || combat.IsDead)
                    {
                        continue;
                    }

                    PlayerController otherPlayer = combat.GetComponent<PlayerController>();
                    if (otherPlayer == null || otherPlayer.TeamId == _player.TeamId)
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(origin, combat.transform.position);
                    if (dist <= range && dist < nearestDist)
                    {
                        nearest = combat;
                        nearestDist = dist;
                    }
                }
            }

            return nearest;
        }

        private IPlayerNetworkManager ResolvePlayerNetworkManager()
        {
            try
            {
                var scope = VContainer.Unity.LifetimeScope.Find<VContainer.Unity.LifetimeScope>();
                if (scope?.Container != null &&
                    scope.Container.TryResolve(out IPlayerNetworkManager manager))
                {
                    return manager;
                }
            }
            catch
            {
                // Session scope may not be active during teardown.
            }

            return null;
        }

        /// <summary>
        /// Apply knockback impulse (server-side authority; replicated via ClientRpc).
        /// </summary>
        public void ApplyKnockback(Vector3 direction, float force)
        {
            if (!IsServer) return;
            ApplyKnockbackClientRpc(direction, force);
        }

        // ─────────────────────────────────────────────────────────────────
        // KO flow (server-only)
        // ─────────────────────────────────────────────────────────────────

        private void StartKo(ulong attackerClientId)
        {
            _isDead.Value = true;

            // Drop ALL carried items as loot
            DropAllCarriedItemsAsLoot();

            // Disable player input / physics
            DisablePlayerClientRpc();
            SpawnKoFxClientRpc(transform.position);

            // Publish domain event
            _eventBus?.Publish(new PlayerKoEvent(_player.OwnerClientId, attackerClientId, _player.TeamId));

            // Schedule respawn
            float respawnSec = _cfg.Get(RemoteConfigKeys.KoRespawnSec, RemoteConfigKeys.Defaults.KoRespawnSec);
            StartCoroutine(RespawnAfter(respawnSec));
        }

        private IEnumerator RespawnAfter(float delay)
        {
            yield return new WaitForSeconds(delay);

            _currentHp.Value = _maxHp.Value;
            _isDead.Value    = false;

            // Teleport to team spawn point
            Vector3 spawnPos = FindSpawnPosition();
            TeleportClientRpc(spawnPos);

            EnablePlayerClientRpc();
            SpawnRespawnFxClientRpc(spawnPos);

            _eventBus?.Publish(new PlayerRespawnedEvent(_player.OwnerClientId, _player.TeamId));
        }

        private Vector3 FindSpawnPosition()
        {
            // Resolve from MatchRuntimeSceneBinder via IMatchContext
            var scope = VContainer.Unity.LifetimeScope.Find<VContainer.Unity.LifetimeScope>(gameObject.scene);
            if (scope != null)
            {
                try
                {
                    var ctx = scope.Container.Resolve<IMatchContext>();
                    return ctx.SpawnManager?.GetRespawnPosition(_player.TeamId) ?? transform.position;
                }
                catch { /* match scope may not be active */ }
            }
            return transform.position;
        }

        // ─────────────────────────────────────────────────────────────────
        // Loot drop (server-only)
        // ─────────────────────────────────────────────────────────────────

        private void DropAllCarriedItemsAsLoot()
        {
            if (!IsServer) return;

            float dropPct     = _cfg.Get(RemoteConfigKeys.KoLootDropPct,    RemoteConfigKeys.Defaults.KoLootDropPct);
            float despawnSec  = _cfg.Get(RemoteConfigKeys.KoLootDespawnSec, RemoteConfigKeys.Defaults.KoLootDespawnSec);

            List<CarriedItemData> items = _player.GetAndClearCarriedItems();
            if (items == null || items.Count == 0) return;

            int dropCount = Mathf.CeilToInt(items.Count * dropPct);

            for (int i = 0; i < dropCount && i < items.Count; i++)
            {
                SpawnLootPickup(items[i], despawnSec);
            }

            GameLogger.Log($"[CombatController] Dropped {dropCount}/{items.Count} items on KO");
        }

        private void SpawnLootPickup(CarriedItemData item, float despawnSec)
        {
            if (_lootPickupPrefab == null)
            {
                GameLogger.LogWarning("[CombatController] LootPickup prefab is not assigned — KO loot will not spawn");
                return;
            }

            Vector3 scatterPos = transform.position + Random.insideUnitSphere * 0.5f;
            scatterPos.y = transform.position.y;

            NetworkObject netObj = null;
            if (_pool != null)
            {
                netObj = _pool.Get(_lootPickupPrefab, scatterPos, Quaternion.identity);
            }

            if (netObj == null)
            {
                GameObject go = Instantiate(_lootPickupPrefab, scatterPos, Quaternion.identity);
                netObj = go.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                {
                    netObj.Spawn(destroyWithScene: true);
                }
            }

            if (netObj == null)
            {
                GameLogger.LogError("[CombatController] Failed to spawn LootPickup NetworkObject");
                return;
            }

            if (netObj.TryGetComponent(out LootPickup pickup))
            {
                // Initialize is spawn-safe: applies NetworkVariables after OnNetworkSpawn if needed.
                pickup.Initialize(item.IngredientType, item.RecipeTier, _player.TeamId, despawnSec);
            }
            else
            {
                GameLogger.LogError("[CombatController] LootPickup component missing on loot prefab");
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Client-side callbacks
        // ─────────────────────────────────────────────────────────────────

        private void OnHpChanged(int hp)
        {
            // Update HUD via event bus (client + server)
            _eventBus?.Publish(new PlayerHpChangedEvent(_player.OwnerClientId, hp, _maxHp.Value));
        }

        private void OnDeadChanged(bool dead)
        {
            // Visuals: hide/show player model
            if (_player != null)
                _player.SetVisible(!dead);
        }

        // ─────────────────────────────────────────────────────────────────
        // ClientRPCs
        // ─────────────────────────────────────────────────────────────────

        [ClientRpc]
        private void DisablePlayerClientRpc()
        {
            if (_player != null) _player.SetInputEnabled(false);
        }

        [ClientRpc]
        private void EnablePlayerClientRpc()
        {
            if (_player != null) _player.SetInputEnabled(true);
        }

        [ClientRpc]
        private void TeleportClientRpc(Vector3 position)
        {
            transform.position = position;
        }

        [ClientRpc]
        private void ApplyKnockbackClientRpc(Vector3 direction, float force)
        {
            if (!IsOwner) return;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb?.AddForce(direction.normalized * force, ForceMode.Impulse);
        }

        [ClientRpc]
        private void SpawnKoFxClientRpc(Vector3 position)
        {
            if (_koFxPrefab != null)
                Instantiate(_koFxPrefab, position, Quaternion.identity);
        }

        [ClientRpc]
        private void SpawnRespawnFxClientRpc(Vector3 position)
        {
            if (_respawnFxPrefab != null)
                Instantiate(_respawnFxPrefab, position, Quaternion.identity);
        }
    }

    // ── Supporting data ────────────────────────────────────────────────

    /// <summary>Data snapshot of a single carried item at the time of KO drop.</summary>
    public readonly struct CarriedItemData
    {
        public readonly IngredientType IngredientType;
        public readonly int            RecipeTier;

        public CarriedItemData(IngredientType type, int tier)
        {
            IngredientType = type;
            RecipeTier     = tier;
        }
    }
}
