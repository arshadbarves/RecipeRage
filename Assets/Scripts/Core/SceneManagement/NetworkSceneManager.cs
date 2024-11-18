using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    /// <summary>
    ///     Handles scene management with network synchronization for P2P gameplay
    /// </summary>
    public class NetworkSceneManager : NetworkBehaviour
    {
        private readonly Dictionary<string, SceneData> _loadedScenes = new Dictionary<string, SceneData>();
        private readonly Dictionary<string, HashSet<ulong>> _sceneLoadStatus = new Dictionary<string, HashSet<ulong>>();
        private SceneLoadRequest _currentRequest;

        public event Action<float> OnLoadProgressUpdated;
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        private bool _isTransitioning;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientDisconnectCallback += HandleClientDisconnect;
                NetworkManager.OnClientConnectedCallback += HandleClientConnect;
            }
        }

        public async Task<bool> LoadSceneAsync(SceneData sceneData, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (!IsServer)
            {
                Debug.LogError("Only server can initiate scene loading");
                return false;
            }

            try
            {
                _currentRequest = new SceneLoadRequest
                {
                    SceneName = sceneData.sceneName,
                    SceneData = sceneData,
                    LoadMode = loadMode,
                    CompletionSource = new TaskCompletionSource<bool>(),
                    StartTime = Time.time,
                    LoadedClients = new HashSet<ulong>()
                };

                // Notify all clients about scene load start
                NotifySceneLoadStartClientRpc(sceneData.sceneName);
                OnSceneLoadStarted?.Invoke(sceneData.sceneName);

                // Load dependencies first
                foreach (string dependency in sceneData.dependencies)
                {
                    if (!_loadedScenes.ContainsKey(dependency))
                    {
                        SceneData depData = Resources.Load<SceneData>($"SceneData/{dependency}");
                        if (depData != null)
                        {
                            await LoadSceneAsync(depData, LoadSceneMode.Additive);
                        }
                    }
                }

                // Network synchronized loading
                if (sceneData.requiresNetworkSync)
                {
                    _sceneLoadStatus[sceneData.sceneName] = new HashSet<ulong>();
                    
                    // Notify clients to prepare for scene load
                    PrepareSceneLoadClientRpc(new SceneLoadParams
                    {
                        SceneName = sceneData.sceneName,
                        LoadMode = loadMode,
                        NetworkTimeout = sceneData.networkTimeout
                    });

                    NetworkManager.SceneManager.LoadScene(sceneData.sceneName, loadMode);

                    Task result = await Task.WhenAny(
                        _currentRequest.CompletionSource.Task,
                        Task.Delay(TimeSpan.FromSeconds(sceneData.networkTimeout))
                    );

                    if (result != _currentRequest.CompletionSource.Task)
                    {
                        NotifySceneLoadTimeoutClientRpc(sceneData.sceneName);
                        Debug.LogWarning($"Scene load timeout: {sceneData.sceneName}");
                        return false;
                    }
                }
                // Local loading
                else
                {
                    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneData.sceneName, loadMode);
                    while (operation is { isDone: false })
                    {
                        float progress = operation.progress;
                        OnLoadProgressUpdated?.Invoke(progress);
                        UpdateLoadProgressClientRpc(progress);
                        await Task.Yield();
                    }
                }

                _loadedScenes[sceneData.sceneName] = sceneData;
                NotifySceneLoadCompleteClientRpc(sceneData.sceneName);
                OnSceneLoadCompleted?.Invoke(sceneData.sceneName);
                return true;
            }
            catch (Exception e)
            {
                NotifySceneLoadErrorClientRpc(sceneData.sceneName, e.Message);
                Debug.LogError($"Error loading scene {sceneData.sceneName}: {e}");
                return false;
            }
        }

        [ClientRpc]
        private void NotifySceneLoadStartClientRpc(string sceneName)
        {
            Debug.Log($"[Client {NetworkManager.LocalClientId}] Scene load starting: {sceneName}");
        }

        [ClientRpc]
        private void PrepareSceneLoadClientRpc(SceneLoadParams loadParams)
        {
            if (!IsServer)
            {
                ClientSceneLoadPreparedServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ClientSceneLoadPreparedServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            if (_currentRequest.LoadedClients != null)
            {
                _currentRequest.LoadedClients.Add(clientId);
                
                // Check if all clients are ready
                if (_currentRequest.LoadedClients.Count == NetworkManager.ConnectedClientsIds.Count)
                {
                    _currentRequest.CompletionSource.TrySetResult(true);
                }
            }
        }

        [ClientRpc]
        private void UpdateLoadProgressClientRpc(float progress)
        {
            OnLoadProgressUpdated?.Invoke(progress);
        }

        [ClientRpc]
        private void NotifySceneLoadCompleteClientRpc(string sceneName)
        {
            Debug.Log($"[Client {NetworkManager.LocalClientId}] Scene load complete: {sceneName}");
        }

        [ClientRpc]
        private void NotifySceneLoadTimeoutClientRpc(string sceneName)
        {
            Debug.LogWarning($"[Client {NetworkManager.LocalClientId}] Scene load timeout: {sceneName}");
        }

        [ClientRpc]
        private void NotifySceneLoadErrorClientRpc(string sceneName, string error)
        {
            Debug.LogError($"[Client {NetworkManager.LocalClientId}] Scene load error in {sceneName}: {error}");
        }

        [ClientRpc]
        private void SyncSceneStateClientRpc(SceneStateParams[] activeScenes)
        {
            if (IsServer) return; // Server already has the correct state

            foreach (var sceneState in activeScenes)
            {
                if (!IsSceneLoaded(sceneState.SceneName))
                {
                    QueueSceneLoad(sceneState);
                }
            }
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            foreach (HashSet<ulong> status in _sceneLoadStatus.Values)
            {
                status.Remove(clientId);
            }

            if (_currentRequest.LoadedClients != null)
            {
                _currentRequest.LoadedClients.Remove(clientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ValidateSceneLoadServerRpc(string sceneName, ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            
            // Basic validation
            if (!IsSceneLoadAllowed(sceneName, clientId))
            {
                NotifySceneLoadErrorClientRpc(sceneName, "Unauthorized scene load attempt");
                return;
            }

            // Additional validation for game state
            if (!IsValidGameState(sceneName))
            {
                NotifySceneLoadErrorClientRpc(sceneName, "Invalid game state for scene load");
                return;
            }
        }

        private bool IsSceneLoadAllowed(string sceneName, ulong clientId)
        {
            // Only allow scene loads from authorized clients (host/server)
            if (!IsServer && clientId != NetworkManager.ServerClientId)
                return false;

            // Check if scene exists in allowed scenes
            if (!_loadedScenes.ContainsKey(sceneName) && !Resources.Load<SceneData>($"SceneData/{sceneName}"))
                return false;

            return true;
        }

        private bool IsValidGameState(string sceneName)
        {
            // Prevent scene loads during critical game states
            if (_isTransitioning) return false;

            // Check if current game state allows scene transition
            return !_currentRequest.CompletionSource?.Task.IsCompleted ?? true;
        }

        private void QueueSceneLoad(SceneStateParams sceneState)
        {
            var sceneData = Resources.Load<SceneData>($"SceneData/{sceneState.SceneName}");
            if (sceneData != null)
            {
                // Load scene additively to maintain current state
                _ = LoadSceneAsync(sceneData, LoadSceneMode.Additive);
            }
        }

        private bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                    return true;
            }
            return false;
        }

        private void HandleClientConnect(ulong clientId)
        {
            if (!IsServer) return;

            // Send current scene state to new client
            var activeScenes = GetActiveScenes();
            SyncSceneStateClientRpc(activeScenes);
        }

        private SceneStateParams[] GetActiveScenes()
        {
            var activeScenes = new List<SceneStateParams>();
            foreach (var scene in _loadedScenes)
            {
                activeScenes.Add(new SceneStateParams
                {
                    SceneName = scene.Key,
                    LoadMode = LoadSceneMode.Additive,
                    IsActive = true
                });
            }
            return activeScenes.ToArray();
        }

        public override void OnDestroy()
        {
            if (IsServer)
            {
                NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
                NetworkManager.OnClientConnectedCallback -= HandleClientConnect;
            }
            base.OnDestroy();
        }

        private struct SceneLoadRequest
        {
            public string SceneName;
            public SceneData SceneData;
            public LoadSceneMode LoadMode;
            public TaskCompletionSource<bool> CompletionSource;
            public float StartTime;
            public HashSet<ulong> LoadedClients;
        }

        private struct SceneLoadParams : INetworkSerializable
        {
            public string SceneName;
            public LoadSceneMode LoadMode;
            public float NetworkTimeout;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref SceneName);
                serializer.SerializeValue(ref LoadMode);
                serializer.SerializeValue(ref NetworkTimeout);
            }
        }

        private struct SceneStateParams : INetworkSerializable
        {
            public string SceneName;
            public LoadSceneMode LoadMode;
            public bool IsActive;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref SceneName);
                serializer.SerializeValue(ref LoadMode);
                serializer.SerializeValue(ref IsActive);
            }
        }
    }
}