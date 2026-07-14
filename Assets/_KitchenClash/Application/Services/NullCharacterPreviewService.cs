using UnityEngine;

namespace KitchenClash.Application
{
    /// <summary>
    /// No-op character preview when no scene preview manager is registered.
    /// MenuLifetimeScope overrides with CharacterPreviewManager from hierarchy.
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
