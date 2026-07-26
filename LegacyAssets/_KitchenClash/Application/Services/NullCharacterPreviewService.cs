using UnityEngine;

namespace KitchenClash.Application
{
    /// <summary>
    /// No-op character preview when no scene preview manager is bound.
    /// Used by <see cref="CharacterPreviewGateway"/> until <c>MenuSceneBinder</c> attaches
    /// a scene <c>CharacterPreviewManager</c>.
    /// </summary>
    public sealed class NullCharacterPreviewService : ICharacterPreviewService
    {
        public static readonly NullCharacterPreviewService Instance = new();

        public void ShowPreview(GameObject prefab)
        {
        }

        public void ClearPreview()
        {
        }

        public void ShowLobbyCharacter(int slotIndex, GameObject prefab)
        {
        }

        public void ClearLobbyCharacter(int slotIndex = -1)
        {
        }
    }
}
