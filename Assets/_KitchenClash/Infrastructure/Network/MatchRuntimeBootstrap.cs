using KitchenClash.Application;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Server-only bridge that wires the mode-specific win condition after the
    /// map scene has loaded. Spawned by <see cref="MatchRuntimePhase"/> once
    /// gameplay scenes are ready; creates the <see cref="MatchWinConditionCoordinator"/>
    /// NetworkObject and hands it the active mode id (e.g. "rush_service").
    /// Self-despawns after wiring so it does not leak into the next match.
    /// </summary>
    public sealed class MatchRuntimeBootstrap : NetworkBehaviour
    {
        private string _modeId;
        private bool _wired;

        [Inject] private IEventBus _eventBus;
        [Inject] private IConfigService _cfg;
        [Inject] private IMatchContext _matchContext;

        /// <summary>Set before Spawn() so the server wires the correct mode.</summary>
        public string ModeId
        {
            get => _modeId;
            set => _modeId = value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer || _wired)
            {
                return;
            }

            _wired = true;
            WireCoordinator();
            StartCoroutine(DespawnNextFrame());
        }

        private void WireCoordinator()
        {
            if (string.IsNullOrEmpty(_modeId))
            {
                GameLogger.LogWarning("[MatchRuntimeBootstrap] No mode id set — win condition coordinator not wired.");
                return;
            }

            var go = new GameObject("MatchWinConditionCoordinator");
            go.AddComponent<NetworkObject>();
            var coordinator = go.AddComponent<MatchWinConditionCoordinator>();

            // Forward the deps this bootstrap was injected with — coordinator
            // lives in the same match scope, so the container injects this object
            // via its IMatchContextReceiver/awake path only when present in a scene.
            // Dynamic spawn needs manual wiring.
            coordinator.InjectDeps(_eventBus, _cfg, _matchContext);

            go.GetComponent<NetworkObject>().Spawn();
            coordinator.SetMode(_modeId);

            GameLogger.Log($"[MatchRuntimeBootstrap] Wired win condition coordinator for mode '{_modeId}'");
        }

        private System.Collections.IEnumerator DespawnNextFrame()
        {
            yield return null;
            if (IsSpawned && IsServer)
            {
                NetworkObject.Despawn();
            }
        }
    }
}
