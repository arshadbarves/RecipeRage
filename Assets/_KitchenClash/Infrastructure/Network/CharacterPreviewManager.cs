using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public class CharacterPreviewManager : MonoBehaviour
    {
        [SerializeField] private Transform _previewSpawnPoint;
        [SerializeField] private Transform[] _lobbySpawnPoints;

        private GameObject _currentPreviewInstance;
        private GameObject[] _currentLobbyInstances = new GameObject[4];

        public void ShowPreview(GameObject prefab)
        {
            ClearPreview();
            if (prefab == null || _previewSpawnPoint == null)
            {
                return;
            }

            _currentPreviewInstance = Instantiate(prefab, _previewSpawnPoint);
            _currentPreviewInstance.transform.localPosition = Vector3.zero;
            _currentPreviewInstance.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

        public void ClearPreview()
        {
            if (_currentPreviewInstance != null)
            {
                Destroy(_currentPreviewInstance);
                _currentPreviewInstance = null;
            }
        }

        public void ShowLobbyCharacter(int slotIndex, GameObject prefab)
        {
            ClearLobbyCharacter(slotIndex);
            if (prefab == null || _lobbySpawnPoints == null || slotIndex < 0 || slotIndex >= _lobbySpawnPoints.Length)
            {
                return;
            }

            GameObject instance = Instantiate(prefab, _lobbySpawnPoints[slotIndex]);
            instance.transform.localPosition = Vector3.zero;
            _currentLobbyInstances[slotIndex] = instance;
        }

        public void ClearLobbyCharacter(int slotIndex = -1)
        {
            if (slotIndex >= 0 && slotIndex < _currentLobbyInstances.Length)
            {
                if (_currentLobbyInstances[slotIndex] != null)
                {
                    Destroy(_currentLobbyInstances[slotIndex]);
                }

                _currentLobbyInstances[slotIndex] = null;
            }
            else
            {
                for (int i = 0; i < _currentLobbyInstances.Length; i++)
                {
                    if (_currentLobbyInstances[i] != null)
                    {
                        Destroy(_currentLobbyInstances[i]);
                    }

                    _currentLobbyInstances[i] = null;
                }
            }
        }
    }
}
