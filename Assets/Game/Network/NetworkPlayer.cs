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

            if (IsServer)
            {
                var modifier = ServiceLocator.Get<IChefProgressionService>().GetSelectedModifier();
                _playerController.ApplyChefModifier(modifier);
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
