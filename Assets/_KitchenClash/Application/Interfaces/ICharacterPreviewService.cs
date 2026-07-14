using UnityEngine;

namespace KitchenClash.Application
{
    /// <summary>
    /// Presentation-facing port for 3D character previews in lobby / details screens.
    /// Implemented by Infrastructure scene MonoBehaviour.
    /// </summary>
    public interface ICharacterPreviewService
    {
        void ShowPreview(GameObject prefab);
        void ClearPreview();
        void ShowLobbyCharacter(int slotIndex, GameObject prefab);
        void ClearLobbyCharacter(int slotIndex = -1);
    }
}
