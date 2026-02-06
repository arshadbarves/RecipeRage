using UnityEngine;

namespace Gameplay.Characters.Visuals
{
    /// <summary>
    /// Manages the spawning and display of 3D character previews in the UI/Lobby.
    /// </summary>
    public class CharacterPreviewManager : MonoBehaviour
    {
        [SerializeField] private Transform _previewSpawnPoint;
        [SerializeField] private Transform[] _lobbySpawnPoints; // Changed to array

        private GameObject _currentPreviewInstance;
        private GameObject[] _currentLobbyInstances = new GameObject[4];

        private UnityEngine.Camera _previewCamera;

        private void Awake()
        {
            InitializeSpawnPoints();
        }

        private void InitializeSpawnPoints()
        {
            // Auto-find Preview Point
            if (_previewSpawnPoint == null)
            {
                var go = GameObject.Find("PreviewSpawnPoint");
                if (go == null)
                {
                    go = new GameObject("PreviewSpawnPoint");
                    // Default position helper (optional, can be moved by user later)
                    go.transform.position = new Vector3(1000, 0, 0); // Far away or specific UI layer spot
                }
                _previewSpawnPoint = go.transform;
            }

            // Ensure Preview Camera exists
            if (_previewCamera == null)
            {
                var camGo = GameObject.Find("PreviewCamera");
                if (camGo == null)
                {
                    camGo = new GameObject("PreviewCamera");
                    camGo.transform.position = _previewSpawnPoint.position + new Vector3(0, 1.5f, 3f); // Offset
                    camGo.transform.LookAt(_previewSpawnPoint.position + Vector3.up); // Look at character head area
                }

                _previewCamera = camGo.GetComponent<UnityEngine.Camera>();
                if (_previewCamera == null) _previewCamera = camGo.AddComponent<UnityEngine.Camera>();

                _previewCamera.clearFlags = CameraClearFlags.Depth; // Render on top of existing
                _previewCamera.depth = 10; // Higher than MainCamera
                _previewCamera.cullingMask = -1; // Everything for now, or specific layer
                _previewCamera.enabled = false; // Disabled by default
            }

            // Ensure Preview Light exists
            var lightGo = GameObject.Find("PreviewLight");
            if (lightGo == null)
            {
                lightGo = new GameObject("PreviewLight");
                lightGo.transform.position = _previewSpawnPoint.position + new Vector3(0, 3, 2);
                lightGo.transform.LookAt(_previewSpawnPoint.position);

                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Spot;
                light.intensity = 1.5f;
                light.spotAngle = 60;
            }

            // Auto-find Lobby Points
            if (_lobbySpawnPoints == null || _lobbySpawnPoints.Length == 0)
            {
                _lobbySpawnPoints = new Transform[4];
                for (int i = 0; i < 4; i++)
                {
                    // Try find "LobbySpawnPoint_0", "LobbySpawnPoint_1" etc
                    var go = GameObject.Find($"LobbySpawnPoint_{i}");
                    if (go == null) go = GameObject.Find($"LobbySpawnPoint{i + 1}"); // Try 1-based too logic often differs

                    if (go == null)
                    {
                        go = new GameObject($"LobbySpawnPoint_{i}");
                        // Default arrangement
                        go.transform.position = new Vector3(i * 2.0f, 0, 0);
                    }
                    _lobbySpawnPoints[i] = go.transform;
                }
            }
        }

        public void ShowPreview(GameObject prefab)
        {
            ClearPreview();

            if (prefab == null || _previewSpawnPoint == null) return;

            if (_previewCamera != null) _previewCamera.enabled = true; // Enable camera

            _currentPreviewInstance = Instantiate(prefab, _previewSpawnPoint);
            _currentPreviewInstance.transform.localPosition = Vector3.zero;
            _currentPreviewInstance.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face camera

            CleanupForPreview(_currentPreviewInstance);
        }

        public void ClearPreview()
        {
            if (_previewCamera != null) _previewCamera.enabled = false; // Disable camera

            if (_currentPreviewInstance != null)
            {
                Destroy(_currentPreviewInstance);
                _currentPreviewInstance = null;
            }
        }

        public void ShowLobbyCharacter(int slotIndex, GameObject prefab)
        {
            ClearLobbyCharacter(slotIndex);

            if (prefab == null || _lobbySpawnPoints == null || slotIndex < 0 || slotIndex >= _lobbySpawnPoints.Length) return;

            Transform spawnPoint = _lobbySpawnPoints[slotIndex];
            if (spawnPoint == null) return;

            var instance = Instantiate(prefab, spawnPoint);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            CleanupForPreview(instance);
            _currentLobbyInstances[slotIndex] = instance;
        }

        public void ClearLobbyCharacter(int slotIndex = -1)
        {
            if (slotIndex >= 0)
            {
                if (slotIndex < _currentLobbyInstances.Length && _currentLobbyInstances[slotIndex] != null)
                {
                    Destroy(_currentLobbyInstances[slotIndex]);
                    _currentLobbyInstances[slotIndex] = null;
                }
            }
            else
            {
                // Clear all if -1
                for (int i = 0; i < _currentLobbyInstances.Length; i++)
                {
                    if (_currentLobbyInstances[i] != null)
                    {
                        Destroy(_currentLobbyInstances[i]);
                        _currentLobbyInstances[i] = null;
                    }
                }
            }
        }

        private void CleanupForPreview(GameObject instance)
        {
            // Remove things that shouldn't run in preview (e.g., character controllers, audio listeners)
            var rbs = instance.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs) rb.isKinematic = true;

            var cols = instance.GetComponentsInChildren<Collider>();
            foreach (var col in cols) col.enabled = false;

            // Optional: Set specific layer for UI lighting
        }
    }
}
