using UnityEngine;

namespace KitchenClash.Application
{
    /// <summary>
    /// Root-owned <see cref="ICharacterPreviewService"/> that forwards to a scene
    /// implementation when bound. Session and UI always resolve this singleton from Root
    /// (or via parent lookup); scene scopes never re-register the port.
    /// </summary>
    public sealed class CharacterPreviewGateway : ICharacterPreviewService
    {
        private ICharacterPreviewService _inner = NullCharacterPreviewService.Instance;

        public bool HasSceneImplementation => IsLive(_inner);

        public void Bind(ICharacterPreviewService sceneImplementation)
        {
            _inner = IsLive(sceneImplementation)
                ? sceneImplementation
                : NullCharacterPreviewService.Instance;
        }

        public void Unbind(ICharacterPreviewService sceneImplementation)
        {
            if (sceneImplementation != null && ReferenceEquals(_inner, sceneImplementation))
            {
                _inner = NullCharacterPreviewService.Instance;
            }
        }

        public void ShowPreview(GameObject prefab) => ResolveInner().ShowPreview(prefab);

        public void ClearPreview() => ResolveInner().ClearPreview();

        public void ShowLobbyCharacter(int slotIndex, GameObject prefab) =>
            ResolveInner().ShowLobbyCharacter(slotIndex, prefab);

        public void ClearLobbyCharacter(int slotIndex = -1) =>
            ResolveInner().ClearLobbyCharacter(slotIndex);

        private ICharacterPreviewService ResolveInner()
        {
            if (!IsLive(_inner))
            {
                _inner = NullCharacterPreviewService.Instance;
            }

            return _inner;
        }

        /// <summary>
        /// Unity fake-null: a destroyed MonoBehaviour still looks non-null as an interface.
        /// </summary>
        private static bool IsLive(ICharacterPreviewService service)
        {
            if (service == null || ReferenceEquals(service, NullCharacterPreviewService.Instance))
            {
                return false;
            }

            if (service is Object unityObject && unityObject == null)
            {
                return false;
            }

            return true;
        }
    }
}
