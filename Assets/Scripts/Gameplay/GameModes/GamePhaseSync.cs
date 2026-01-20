using Unity.Netcode;
using Core.Logging;

namespace Gameplay.GameModes
{
    /// <summary>
    /// Minimal NetworkBehaviour for syncing game phase across network.
    /// Controlled by GameModeController (pure C# class).
    /// </summary>
    public class GamePhaseSync : NetworkBehaviour
    {
        private NetworkVariable<GamePhase> _currentPhase = new NetworkVariable<GamePhase>(
            GamePhase.Waiting,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>
        /// Current synchronized game phase
        /// </summary>
        public GamePhase CurrentPhase => _currentPhase.Value;

        /// <summary>
        /// Event fired when phase changes
        /// </summary>
        public event System.Action<GamePhase, GamePhase> OnPhaseChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentPhase.OnValueChanged += HandlePhaseChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _currentPhase.OnValueChanged -= HandlePhaseChanged;
        }

        /// <summary>
        /// Set the current phase (Server only)
        /// </summary>
        public void SetPhase(GamePhase newPhase)
        {
            if (!IsServer)
            {
                GameLogger.LogWarning("Only server can set game phase");
                return;
            }

            _currentPhase.Value = newPhase;
        }

        private void HandlePhaseChanged(GamePhase previousPhase, GamePhase newPhase)
        {
            GameLogger.Log($"Game phase changed: {previousPhase} → {newPhase}");
            OnPhaseChanged?.Invoke(previousPhase, newPhase);
        }
    }
}
