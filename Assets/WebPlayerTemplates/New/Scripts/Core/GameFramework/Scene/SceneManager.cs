using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.GameFramework.Event.Core;
using Core.GameFramework.Event.Events.NetworkEvents;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core.GameFramework.Scene
{
    public class SceneLoadManager : NetworkBehaviour
    {

        private const float LoadTimeout = 30f; // Timeout in seconds

        private readonly Dictionary<ulong, bool> _clientLoadStatus = new Dictionary<ulong, bool>();

        private readonly NetworkVariable<int> _clientsLoaded = new NetworkVariable<int>();
        private readonly NetworkVariable<SceneConfig.GameScene> _currentScene =
            new NetworkVariable<SceneConfig.GameScene>();
        [Inject] private EventManager _eventManager;
        [Inject] private SceneConfig _sceneConfig;
        private TaskCompletionSource<bool> _sceneLoadCompletionSource;

        private bool IsNetworkActive =>
            NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        public SceneLoadProgress LoadProgress { get; private set; } = new SceneLoadProgress();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }

            // Subscribe to scene loading events
            NetworkManager.SceneManager.OnLoad += OnLoadStart;
            NetworkManager.SceneManager.OnLoadComplete += OnLoadComplete;
            NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            NetworkManager.SceneManager.OnSynchronize += OnSynchronizeStart;
            NetworkManager.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
        }

        // Main entry point for loading scenes
        public async Task<bool> LoadScene(SceneConfig.GameScene targetScene)
        {
            // Check if we're running in network mode
            if (IsNetworkActive)
            {
                return await LoadSceneNetworked(targetScene);
            }
            return await LoadSceneLocally(targetScene);
        }

        public async Task<bool> LoadSceneNetworked(SceneConfig.GameScene targetScene)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Only server can initiate scene loading");
                return false;
            }

            SceneConfig.SceneData sceneData = _sceneConfig.GetSceneData(targetScene);
            if (sceneData == null)
            {
                Debug.LogError($"Scene data not found for {targetScene}");
                return false;
            }

            // Initialize progress tracking
            ResetLoadProgress();
            _sceneLoadCompletionSource = new TaskCompletionSource<bool>();

            // If scene requires loading screen
            if (sceneData.requiresLoading)
            {
                await LoadLoadingScreen();
            }

            try
            {
                // Notify clients about new scene loading
                NotifySceneLoadStartClientRpc(targetScene);

                // Start scene loading
                LoadProgress.CurrentOperation = "Loading Scene";
                SceneEventProgressStatus loadOperation = NetworkManager.SceneManager.LoadScene(
                    sceneData.sceneName,
                    LoadSceneMode.Single
                );

                // Start timeout monitoring
                StartTimeoutMonitoring();

                // Wait for completion
                bool success = await _sceneLoadCompletionSource.Task;
                if (success)
                {
                    _currentScene.Value = targetScene;
                    return true;
                }
                Debug.LogError("Scene load failed or timed out");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading scene: {e.Message}");
                _sceneLoadCompletionSource.TrySetResult(false);
                return false;
            }
        }

        // Local scene loading
        private async Task<bool> LoadSceneLocally(SceneConfig.GameScene targetScene)
        {
            SceneConfig.SceneData sceneData = _sceneConfig.GetSceneData(targetScene);
            if (sceneData == null)
            {
                Debug.LogError($"Scene data not found for {targetScene}");
                return false;
            }

            try
            {
                // Reset progress tracking
                ResetLoadProgress();
                LoadProgress.TotalClients = 1;
                LoadProgress.CurrentOperation = "Loading Scene Locally";

                // Load loading screen if required
                if (sceneData.requiresLoading)
                {
                    await LoadLoadingScreen();
                }

                // Load the actual scene
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneData.sceneName);
                if (asyncOperation == null)
                {
                    Debug.LogError($"Failed to start loading scene {sceneData.sceneName}");
                    return false;
                }

                // Optionally prevent scene activation until we're ready
                asyncOperation.allowSceneActivation = false;

                // Track loading progress
                while (asyncOperation.progress < 0.9f)
                {
                    LoadProgress.SceneLoadingProgress = asyncOperation.progress;
                    LoadProgress.CurrentOperation = $"Loading Scene: {asyncOperation.progress * 100:F0}%";
                    await Task.Yield();
                }

                // Scene is ready to activate
                LoadProgress.SceneLoadingProgress = 1f;
                LoadProgress.CurrentOperation = "Finalizing Scene Load";
                asyncOperation.allowSceneActivation = true;

                // Wait for final activation
                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                LoadProgress.IsComplete = true;
                LoadProgress.ClientsLoaded = 1;
                LoadProgress.CurrentOperation = "Scene Load Complete";

                // Publish local scene load complete event
                _eventManager.Publish(new SceneLoadCompleteEvent(targetScene));

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading scene locally: {e.Message}");
                return false;
            }
        }

        private void ResetLoadProgress()
        {
            LoadProgress = new SceneLoadProgress {
                AssetLoadingProgress = 0f,
                SceneLoadingProgress = 0f,
                ClientSyncProgress = 0f,
                ClientsLoaded = 0,
                TotalClients = IsNetworkActive ? NetworkManager.ConnectedClientsIds.Count : 1,
                CurrentOperation = "Initializing",
                IsComplete = false
            };
        }

        // Add method to check if scene exists
        public bool DoesSceneExist(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string scene = Path.GetFileNameWithoutExtension(path);
                if (scene == sceneName)
                    return true;
            }
            return false;
        }

        private async void StartTimeoutMonitoring()
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(LoadTimeout));
                if (!_sceneLoadCompletionSource.Task.IsCompleted)
                {
                    Debug.LogWarning("Scene load timed out");
                    _sceneLoadCompletionSource.TrySetResult(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in timeout monitoring: {e.Message}");
            }
        }

        private async Task LoadLoadingScreen()
        {
            SceneConfig.SceneData loadingSceneData = _sceneConfig.GetSceneData(_sceneConfig.loadingScene);
            if (loadingSceneData != null)
            {
                LoadProgress.CurrentOperation = "Loading Loading Screen";
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(loadingSceneData.sceneName);

                while (asyncOperation is { isDone: false })
                {
                    LoadProgress.SceneLoadingProgress = asyncOperation.progress;
                    await Task.Yield();
                }
            }
        }

        private void OnLoadStart(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            LoadProgress.CurrentOperation = $"Loading {sceneName}";
            LoadProgress.SceneLoadingProgress = 0f;
        }

        private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                LoadProgress.SceneLoadingProgress = 1f;
                LoadProgress.CurrentOperation = "Scene Loaded, Waiting for Other Clients";
                NotifyClientReadyServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotifyClientReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            if (_clientLoadStatus.TryAdd(clientId, true))
            {
                _clientsLoaded.Value++;
                LoadProgress.ClientsLoaded = _clientsLoaded.Value;

                // Check if all clients are ready
                if (_clientsLoaded.Value == NetworkManager.ConnectedClientsIds.Count)
                {
                    NotifyAllClientsReadyClientRpc();
                    _sceneLoadCompletionSource?.TrySetResult(true);
                }
            }
        }

        [ClientRpc]
        private void NotifyAllClientsReadyClientRpc()
        {
            LoadProgress.IsComplete = true;
            LoadProgress.CurrentOperation = "All Clients Ready";
            _eventManager.Publish(new SceneLoadCompleteEvent(_currentScene.Value));
        }

        [ClientRpc]
        private void NotifySceneLoadStartClientRpc(SceneConfig.GameScene targetScene)
        {
            LoadProgress.CurrentOperation = $"Starting Load of {targetScene}";
            _eventManager.Publish(new SceneLoadStartEvent(targetScene));
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            foreach (ulong clientId in clientsTimedOut)
            {
                Debug.LogWarning($"Client {clientId} timed out while loading scene {sceneName}");
            }
        }

        private void OnSynchronizeStart(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                LoadProgress.CurrentOperation = "Synchronizing Scene";
                LoadProgress.ClientSyncProgress = 0f;
            }
        }

        private void OnSynchronizeComplete(ulong clientId)
        {
            if (NetworkManager.LocalClientId == clientId)
            {
                LoadProgress.ClientSyncProgress = 1f;
                LoadProgress.CurrentOperation = "Scene Synchronized";
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            NetworkManager.SceneManager.OnLoad -= OnLoadStart;
            NetworkManager.SceneManager.OnLoadComplete -= OnLoadComplete;
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.SceneManager.OnSynchronize -= OnSynchronizeStart;
            NetworkManager.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;

            base.OnNetworkDespawn();
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                _clientLoadStatus[clientId] = false;
                LoadProgress.TotalClients = NetworkManager.ConnectedClientsIds.Count;
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (IsServer)
            {
                _clientLoadStatus.Remove(clientId);
                UpdateClientLoadCount();
            }
        }

        private void UpdateClientLoadCount()
        {
            int readyCount = 0;
            foreach (bool status in _clientLoadStatus.Values)
            {
                if (status) readyCount++;
            }
            _clientsLoaded.Value = readyCount;
            LoadProgress.ClientsLoaded = readyCount;
            LoadProgress.TotalClients = NetworkManager.ConnectedClientsIds.Count;
        }
    }
}