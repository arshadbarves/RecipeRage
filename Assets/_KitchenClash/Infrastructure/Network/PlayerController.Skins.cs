using System.Linq;
using KitchenClash.Application.Models;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using KitchenClash.Domain;
using Playcenter.Shell;


namespace KitchenClash.Infrastructure.Network
{
    public partial class PlayerController
    {
        #region Skins

        public string GetSkinId() => _skinId.Value.ToString();

        public void SetSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId))
            {
                GameLogger.LogWarning("Cannot set empty skin id");
                return;
            }

            if (IsServer)
            {
                SetSkinInternal(skinId);
                return;
            }

            if (IsLocalPlayer)
            {
                SetSkinServerRpc(skinId);
            }
        }

        [ServerRpc]
        private void SetSkinServerRpc(string skinId)
        {
            SetSkinInternal(skinId);
        }

        private void SetSkinInternal(string skinId)
        {
            if (!IsServer)
            {
                return;
            }

            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                GameLogger.LogWarning("No skins available for current character");
                _skinId.Value = default;
                return;
            }

            bool existsForCharacter = CharacterClass.Skins.Any(s => s != null && s.id == skinId);
            if (!existsForCharacter)
            {
                GameLogger.LogWarning($"Skin '{skinId}' does not belong to character '{CharacterClass.DisplayName}', falling back to default");
                _skinId.Value = new FixedString64Bytes(GetDefaultSkinIdForCharacter() ?? string.Empty);
                return;
            }

            _skinId.Value = new FixedString64Bytes(skinId);
        }

        private void InitializeSkinSystem()
        {
            if (IsServer)
            {
                EnsureSkinInitialized();
            }

            _skinId.OnValueChanged += OnSkinIdChanged;
            ApplySkin(_skinId.Value);
        }

        private void CleanupSkinSystem()
        {
            _skinId.OnValueChanged -= OnSkinIdChanged;

            if (_skinInstance != null)
            {
                Destroy(_skinInstance);
                _skinInstance = null;
            }

            if (_fallbackModelRenderer != null)
            {
                _fallbackModelRenderer.enabled = true;
            }
        }

        private void OnSkinIdChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            ApplySkin(newValue);
        }

        private void EnsureSkinInitialized()
        {
            if (!IsServer)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_skinId.Value.ToString()))
            {
                EnsureValidSkinForCharacter();
                return;
            }

            string initialSkinId = SelectInitialSkinId();
            if (!string.IsNullOrEmpty(initialSkinId))
            {
                _skinId.Value = new FixedString64Bytes(initialSkinId);
            }
        }

        private void EnsureValidSkinForCharacter()
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return;
            }

            string currentSkinId = _skinId.Value.ToString();
            if (string.IsNullOrEmpty(currentSkinId))
            {
                if (IsServer)
                {
                    string initialSkinId = SelectInitialSkinId();
                    if (!string.IsNullOrEmpty(initialSkinId))
                    {
                        _skinId.Value = new FixedString64Bytes(initialSkinId);
                    }
                }
                return;
            }

            if (!string.IsNullOrEmpty(currentSkinId) && CharacterClass.Skins.Any(s => s != null && s.id == currentSkinId))
            {
                return;
            }

            if (IsServer)
            {
                string defaultSkinId = GetDefaultSkinIdForCharacter();
                if (!string.IsNullOrEmpty(defaultSkinId))
                {
                    _skinId.Value = new FixedString64Bytes(defaultSkinId);
                }
            }
        }

        private string SelectInitialSkinId()
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return null;
            }

            if (IsBotNetworkObject() && _randomizeBotSkin && CharacterClass.Skins.Count > 1)
            {
                int index = UnityEngine.Random.Range(0, CharacterClass.Skins.Count);
                return CharacterClass.Skins[index]?.id;
            }

            return GetDefaultSkinIdForCharacter();
        }

        private string GetDefaultSkinIdForCharacter()
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return null;
            }

            SkinItem defaultSkin = CharacterClass.Skins.FirstOrDefault(s => s != null && s.isDefault);
            defaultSkin ??= CharacterClass.Skins.FirstOrDefault(s => s != null);
            return defaultSkin?.id;
        }

        private SkinItem GetSkinItem(string skinId)
        {
            if (CharacterClass == null || CharacterClass.Skins == null || CharacterClass.Skins.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(skinId))
            {
                return null;
            }

            return CharacterClass.Skins.FirstOrDefault(s => s != null && s.id == skinId);
        }

        private void ApplySkin(FixedString64Bytes skinIdValue)
        {
            string skinId = skinIdValue.ToString();
            SkinItem skin = GetSkinItem(skinId) ?? GetSkinItem(GetDefaultSkinIdForCharacter());

            Transform root = GetOrFindSkinRoot();
            if (root == null)
            {
                return;
            }

            EnsureFallbackRendererCached(root);

            if (_skinInstance != null)
            {
                Destroy(_skinInstance);
                _skinInstance = null;
            }

            if (skin == null || skin.prefab == null)
            {
                SetFallbackModelVisible(true);
                return;
            }

            SetFallbackModelVisible(false);

            _skinInstance = Instantiate(skin.prefab, root);
            _skinInstance.transform.localPosition = Vector3.zero;
            _skinInstance.transform.localRotation = Quaternion.identity;
            _skinInstance.transform.localScale = Vector3.one;
        }

        private Transform GetOrFindSkinRoot()
        {
            if (_skinRoot != null)
            {
                return _skinRoot;
            }

            _skinRoot = transform.Find("Model");
            return _skinRoot;
        }

        private void EnsureFallbackRendererCached(Transform root)
        {
            if (_fallbackModelRenderer != null)
            {
                return;
            }

            _fallbackModelRenderer = root.GetComponent<MeshRenderer>();
        }

        private void SetFallbackModelVisible(bool isVisible)
        {
            if (_fallbackModelRenderer == null)
            {
                return;
            }

            _fallbackModelRenderer.enabled = isVisible;
        }

        private bool IsBotNetworkObject()
        {
            return NetworkObject != null && !NetworkObject.IsPlayerObject;
        }

        #endregion

    }
}
