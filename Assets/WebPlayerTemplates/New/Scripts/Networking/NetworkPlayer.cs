using UnityEngine;
using Unity.Netcode;
using System;
using System.Threading.Tasks;
using RecipeRage.Core.Network;

namespace RecipeRage.Networking
{
    public class NetworkPlayer : NetworkBehaviour, INetworkPlayer
    {
        #region Events
        public event Action<PlayerNetworkState> OnStateChanged;
        public event Action<Vector3> OnPositionUpdated;
        public event Action<string> OnMessageReceived;
        #endregion

        #region Properties
        public string PlayerId { get; private set; }
        public NetworkVariable<NetworkString> PlayerName = new NetworkVariable<NetworkString>();
        public PlayerNetworkState State { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public bool IsLocal => IsOwner;
        #endregion

        #region NetworkVariables
        private NetworkVariable<PlayerNetworkState> _networkState = new NetworkVariable<PlayerNetworkState>(
            PlayerNetworkState.Connecting,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
            
        private NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
            
        private NetworkVariable<bool> _networkAuthenticated = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        #endregion

        #region Unity Lifecycle
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitializePlayer();
            
            if (IsServer)
            {
                _networkState.Value = PlayerNetworkState.Connected;
            }

            // Subscribe to network variable changes
            _networkState.OnValueChanged += OnNetworkStateChanged;
            _networkPosition.OnValueChanged += OnNetworkPositionChanged;
            _networkAuthenticated.OnValueChanged += OnNetworkAuthenticatedChanged;
        }

        public override void OnNetworkDespawn()
        {
            // Unsubscribe from network variable changes
            _networkState.OnValueChanged -= OnNetworkStateChanged;
            _networkPosition.OnValueChanged -= OnNetworkPositionChanged;
            _networkAuthenticated.OnValueChanged -= OnNetworkAuthenticatedChanged;
            
            base.OnNetworkDespawn();
        }
        #endregion

        #region Initialization
        private void InitializePlayer()
        {
            PlayerId = NetworkObjectId.ToString();
            if (IsServer)
            {
                PlayerName.Value = $"Player_{PlayerId}";
            }
            State = _networkState.Value;
            IsAuthenticated = _networkAuthenticated.Value;

            Debug.Log($"[NetworkPlayer] Initialized: {PlayerName.Value} (ID: {PlayerId})");
        }
        #endregion

        #region Network Callbacks
        private void OnNetworkStateChanged(PlayerNetworkState previousValue, PlayerNetworkState newValue)
        {
            State = newValue;
            OnStateChanged?.Invoke(newValue);
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} state changed: {previousValue} -> {newValue}");
        }

        private void OnNetworkPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            OnPositionUpdated?.Invoke(newValue);
        }

        private void OnNetworkAuthenticatedChanged(bool previousValue, bool newValue)
        {
            IsAuthenticated = newValue;
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} authentication changed: {previousValue} -> {newValue}");
        }
        #endregion

        #region Server RPCs
        [Rpc(SendTo.Server)]
        public void UpdateStateServerRpc(PlayerNetworkState newState)
        {
            if (!IsServer) return;
            _networkState.Value = newState;
            NotifyStateChangeClientRpc(newState);
        }

        [Rpc(SendTo.Server)]
        public void UpdatePositionServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            
            // Validate sender is the owner
            if (rpcParams.Receive.SenderClientId != OwnerClientId) return;
            
            _networkPosition.Value = newPosition;
            BroadcastPositionClientRpc(newPosition);
        }

        [Rpc(SendTo.Server)]
        public void AuthenticateServerRpc(string authToken, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            
            // TODO: Implement actual authentication logic
            bool authSuccess = !string.IsNullOrEmpty(authToken);
            _networkAuthenticated.Value = authSuccess;
            
            // Notify the specific client about authentication result
            NotifyAuthenticationClientRpc(authSuccess, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { rpcParams.Receive.SenderClientId }
                }
            });
        }
        #endregion

        #region Client RPCs
        [Rpc(SendTo.Everyone)]
        public void NotifyStateChangeClientRpc(PlayerNetworkState newState)
        {
            if (IsServer) return; // Server already knows the state
            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        [Rpc(SendTo.Everyone)]
        private void BroadcastPositionClientRpc(Vector3 position)
        {
            if (IsOwner) return; // Owner already knows their position
            OnPositionUpdated?.Invoke(position);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyAuthenticationClientRpc(bool authenticated, ClientRpcParams rpcParams = default)
        {
            IsAuthenticated = authenticated;
            OnAuthenticationComplete(authenticated);
        }

        [Rpc(SendTo.Everyone)]
        public void BroadcastMessageClientRpc(string message, ClientRpcParams rpcParams = default)
        {
            OnMessageReceived?.Invoke(message);
        }
        #endregion

        #region INetworkPlayer Implementation
        public Task<bool> Authenticate(string authToken)
        {
            if (!IsSpawned)
                return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>();
            void AuthHandler(bool success)
            {
                tcs.SetResult(success);
                OnAuthenticationComplete -= AuthHandler;
            }
            OnAuthenticationComplete += AuthHandler;

            AuthenticateServerRpc(authToken);
            return tcs.Task;
        }

        private event Action<bool> OnAuthenticationComplete;

        public void UpdatePosition(Vector3 position)
        {
            if (!IsSpawned || !IsOwner)
                return;

            UpdatePositionServerRpc(position);
        }

        public void UpdateState(PlayerNetworkState newState)
        {
            if (!IsSpawned || !IsOwner)
                return;

            UpdateStateServerRpc(newState);
        }

        public void SendMessage(string message)
        {
            if (!IsSpawned || !IsServer)
                return;

            BroadcastMessageClientRpc(message);
        }
        #endregion
    }

    public struct NetworkString : INetworkSerializable
    {
        private string _value;
        public string Value
        {
            get => _value;
            set => _value = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _value);
        }

        public static implicit operator string(NetworkString s) => s.Value;
        public static implicit operator NetworkString(string s) => new NetworkString { Value = s };
    }
}