using Unity.Netcode;
using Core.Logging;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Minimal NetworkBehaviour for syncing game phase across network.
    /// </summary>
    public class GamePhaseSync : NetworkBehaviour
    {
        private NetworkVariable<GamePhase> _currentPhase = new NetworkVariable<GamePhase>(
            GamePhase.Waiting,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public GamePhase CurrentPhase => _currentPhase.Value;

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
