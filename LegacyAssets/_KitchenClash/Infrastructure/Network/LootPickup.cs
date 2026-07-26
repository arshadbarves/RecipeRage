using System.Collections;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Domain.Enums;
using Unity.Netcode;
using UnityEngine;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// v2 floor loot pickup — spawned on server when a player is KO'd.
    ///
    /// Rules (wiki/GameplayDesign.md § Combat Model):
    ///   • Anyone can pick it up (including the victim after respawn)
    ///   • Auto-despawns after ko_loot_despawn_sec (default 8s)
    ///   • Picking it up gives the ingredient/dish to the player's carry slot
    ///
    /// Server-authoritative. IInteractable routes through InteractServerRpc.
    /// </summary>
    public sealed class LootPickup : NetworkBehaviour, IInteractable
    {
        [Header("Visuals")]
        [SerializeField] private GameObject _tier1Visual;
        [SerializeField] private GameObject _tier2Visual;
        [SerializeField] private GameObject _tier3Visual;
        [SerializeField] private GameObject _despawnFxPrefab;

        // ── Network state ──
        private readonly NetworkVariable<int>  _recipeTier =
            new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int>  _ingredientTypeInt =
            new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int>  _droppedByTeam =
            new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // ── State ──
        private float _despawnSec = 8f;
        private bool  _collected;
        private bool  _pendingInit;
        private IngredientType _pendingType;
        private int _pendingTier;
        private int _pendingTeam;
        private float _pendingDespawnSec;

        // ── Public accessors ──
        public int           RecipeTier     => _recipeTier.Value;
        public IngredientType IngredientType => (IngredientType)_ingredientTypeInt.Value;

        // ─────────────────────────────────────────────────────────────────
        // Initialisation — safe before or after NetworkObject.Spawn
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Configure loot payload. Call after Instantiate; may run before Spawn.
        /// NetworkVariables are applied once the object is server-spawned.
        /// </summary>
        public void Initialize(IngredientType ingredientType, int tier, int droppedByTeam, float despawnSec)
        {
            // Reset for pool reuse / re-spawn.
            StopAllCoroutines();
            _collected = false;
            _pendingType = ingredientType;
            _pendingTier = Mathf.Clamp(tier, 1, 3);
            _pendingTeam = droppedByTeam;
            _pendingDespawnSec = despawnSec > 0f ? despawnSec : 8f;
            _pendingInit = true;

            if (IsSpawned && IsServer)
            {
                ApplyPendingInit();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _recipeTier.OnValueChanged += (_, tier) => RefreshVisuals(tier);
            RefreshVisuals(_recipeTier.Value);

            if (IsServer && _pendingInit)
            {
                ApplyPendingInit();
            }
        }

        private void ApplyPendingInit()
        {
            _recipeTier.Value = _pendingTier;
            _ingredientTypeInt.Value = (int)_pendingType;
            _droppedByTeam.Value = _pendingTeam;
            _despawnSec = _pendingDespawnSec;
            _pendingInit = false;
            RefreshVisuals(_recipeTier.Value);
            StartCoroutine(AutoDespawn(_despawnSec));
        }

        // ─────────────────────────────────────────────────────────────────
        // IInteractable — used by PlayerInteractionController OverlapSphere
        // ─────────────────────────────────────────────────────────────────

        public void Interact(object playerObj)
        {
            if (playerObj is not PlayerController player)
            {
                return;
            }

            if (!IsServer)
            {
                InteractServerRpc(player.NetworkObject);
                return;
            }

            TryCollect(player);
        }

        public string GetInteractionPrompt()
        {
            return $"Pick up T{RecipeTier} dish";
        }

        public bool CanInteract(object playerObj)
        {
            if (_collected)
            {
                return false;
            }

            if (playerObj is not PlayerController player)
            {
                return false;
            }

            return !player.IsCarryingMaxItems;
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc(NetworkObjectReference playerRef)
        {
            if (!playerRef.TryGet(out NetworkObject playerNetObj))
            {
                return;
            }

            PlayerController player = playerNetObj.GetComponent<PlayerController>();
            if (player != null)
            {
                TryCollect(player);
            }
        }

        /// <summary>
        /// Legacy entry point kept for any direct callers.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PickupServerRpc(NetworkObjectReference playerRef, ServerRpcParams rpcParams = default)
        {
            if (!playerRef.TryGet(out NetworkObject playerNetObj))
            {
                return;
            }

            PlayerController player = playerNetObj.GetComponent<PlayerController>();
            if (player != null)
            {
                TryCollect(player);
            }
        }

        private void TryCollect(PlayerController player)
        {
            if (!IsServer || _collected || player == null)
            {
                return;
            }

            if (player.IsCarryingMaxItems)
            {
                return;
            }

            _collected = true;
            player.ReceiveCollectedDish(_recipeTier.Value, (IngredientType)_ingredientTypeInt.Value);

            GameLogger.Log($"[LootPickup] Collected by client {player.OwnerClientId} (T{_recipeTier.Value})");
            SpawnDespawnFxClientRpc(transform.position);

            if (IsSpawned)
            {
                NetworkObject.Despawn(destroy: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Auto-despawn
        // ─────────────────────────────────────────────────────────────────

        private IEnumerator AutoDespawn(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!_collected && IsSpawned)
            {
                SpawnDespawnFxClientRpc(transform.position);
                NetworkObject.Despawn(destroy: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Visuals
        // ─────────────────────────────────────────────────────────────────

        private void RefreshVisuals(int tier)
        {
            if (_tier1Visual) _tier1Visual.SetActive(tier == 1);
            if (_tier2Visual) _tier2Visual.SetActive(tier == 2);
            if (_tier3Visual) _tier3Visual.SetActive(tier == 3);
        }

        [ClientRpc]
        private void SpawnDespawnFxClientRpc(Vector3 position)
        {
            if (_despawnFxPrefab != null)
                Instantiate(_despawnFxPrefab, position, Quaternion.identity);
        }
    }
}
