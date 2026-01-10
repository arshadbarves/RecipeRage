using System.Collections.Generic;
using Core.Logging;
using Core.Session;
using Gameplay.Skins;
using Gameplay.Skins.Data;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Components.Tabs
{
    /// <summary>
    /// Skins tab content component
    /// Updated to use SkinsService instead of JSON loading
    /// </summary>
    public class SkinsTabComponent
    {
        private VisualElement _root;
        private VisualElement _skinsGrid;
        private Label _skinNameLabel;
        private Button _equipButton;
        private string _selectedSkinId;
        private ISkinsService _skinsService;
        private SessionManager _sessionManager;

        [Inject]
        public SkinsTabComponent(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public void Initialize(VisualElement root)
        {
            GameLogger.Log("Initialize called");

            if (root == null)
            {
                GameLogger.LogError("Root is null!");
                return;
            }

            _root = root;
            GameLogger.Log($"Root element: {_root.name}");

            _skinsGrid = _root.Q<VisualElement>("skins-grid");
            _skinNameLabel = _root.Q<Label>("skin-name");
            _equipButton = _root.Q<Button>("equip-button");

            GameLogger.Log($"Elements found - Grid: {_skinsGrid != null}, Label: {_skinNameLabel != null}, Button: {_equipButton != null}");

            if (_skinsGrid == null)
            {
                GameLogger.LogError("Skins grid not found");
                return;
            }

            // Resolve SkinsService from session container
            ResolveServices();

            if (_skinsService == null)
            {
                GameLogger.LogError("SkinsService not available");
                return;
            }

            // Get equipped skin for character 0 (default character)
            var equippedSkin = _skinsService.GetEquippedSkin(0);
            _selectedSkinId = equippedSkin?.id;

            if (_equipButton != null)
            {
                _equipButton.clicked += OnEquipButtonClicked;
                GameLogger.Log("Equip button listener added");
            }

            UpdateSkinNameDisplay();
            PopulateSkins();
            UpdateEquipButton();
        }

        private void ResolveServices()
        {
            if (_skinsService != null) return;

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                _skinsService = sessionContainer.Resolve<ISkinsService>();
            }
        }

        private void UpdateSkinNameDisplay()
        {
            if (_skinNameLabel != null && _skinsService != null && !string.IsNullOrEmpty(_selectedSkinId))
            {
                SkinItem skin = _skinsService.GetSkin(_selectedSkinId);
                if (skin != null)
                {
                    _skinNameLabel.text = skin.name.ToUpper();
                }
            }
        }

        private void PopulateSkins()
        {
            if (_skinsGrid == null || _skinsService == null) return;

            _skinsGrid.Clear();

            // Get skins for character 0 (default character)
            List<SkinItem> skins = _skinsService.GetSkinsForCharacter(0);

            foreach (SkinItem skin in skins)
            {
                VisualElement skinItem = CreateSkinItem(skin);
                _skinsGrid.Add(skinItem);
            }

            GameLogger.Log($"Populated {skins.Count} skins");
        }

        private VisualElement CreateSkinItem(SkinItem skin)
        {
            bool isUnlocked = _skinsService.IsSkinUnlocked(skin.id);
            bool isEquipped = _skinsService.GetEquippedSkin(0)?.id == skin.id;

            Button skinItem = new Button(() => OnSkinSelected(skin));
            skinItem.AddToClassList("skin-item");

            if (skin.id == _selectedSkinId)
            {
                skinItem.AddToClassList("selected");
            }

            if (!isUnlocked)
            {
                skinItem.AddToClassList("skin-locked");
            }

            VisualElement image = new VisualElement();
            image.AddToClassList("skin-item-image");
            skinItem.Add(image);

            if (!isUnlocked)
            {
                VisualElement lockIcon = new VisualElement();
                lockIcon.AddToClassList("lock-icon");
                image.Add(lockIcon);

                // Show price for locked skins
                Label costLabel = new Label($"{skin.Price} 💰");
                costLabel.style.position = Position.Absolute;
                costLabel.style.bottom = 5;
                costLabel.style.width = new Length(100, LengthUnit.Percent);
                costLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                costLabel.style.color = new Color(1f, 0.8f, 0.2f);
                costLabel.style.fontSize = 12;
                costLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                image.Add(costLabel);
            }

            Label nameLabel = new Label(skin.name.ToUpper());
            nameLabel.AddToClassList("skin-item-name");
            skinItem.Add(nameLabel);

            // Add rarity badge
            Label rarityLabel = new Label(skin.rarity.ToString().ToUpper());
            rarityLabel.AddToClassList("skin-rarity-badge");
            rarityLabel.style.fontSize = 10;
            rarityLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            rarityLabel.style.marginTop = 2;
            rarityLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Set rarity color
            Color rarityColor = skin.rarity switch
            {
                SkinRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                SkinRarity.Rare => new Color(0.3f, 0.6f, 1f),
                SkinRarity.Epic => new Color(0.8f, 0.3f, 1f),
                SkinRarity.Legendary => new Color(1f, 0.7f, 0.2f),
                _ => Color.white
            };
            rarityLabel.style.color = rarityColor;
            skinItem.Add(rarityLabel);

            if (isEquipped)
            {
                Label equippedLabel = new Label("EQUIPPED");
                equippedLabel.style.fontSize = 10;
                equippedLabel.style.color = new Color(0.4f, 0.8f, 0.4f);
                equippedLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                equippedLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                skinItem.Add(equippedLabel);
            }

            return skinItem;
        }

        private void OnSkinSelected(SkinItem skin)
        {
            bool isUnlocked = _skinsService.IsSkinUnlocked(skin.id);

            if (!isUnlocked)
            {
                GameLogger.Log($"Skin is locked: {skin.name} (Cost: {skin.Price} coins)");
                return;
            }

            _selectedSkinId = skin.id;
            UpdateSkinNameDisplay();
            UpdateEquipButton();
            PopulateSkins();

            GameLogger.Log($"Skin selected: {skin.name}");
        }

        private void UpdateEquipButton()
        {
            if (_equipButton == null || _skinsService == null) return;

            var equippedSkin = _skinsService.GetEquippedSkin(0);
            bool isEquipped = equippedSkin?.id == _selectedSkinId;

            _equipButton.text = isEquipped ? "EQUIPPED" : "EQUIP";

            if (isEquipped)
            {
                _equipButton.AddToClassList("equipped");
            }
            else
            {
                _equipButton.RemoveFromClassList("equipped");
            }
        }

        private void OnEquipButtonClicked()
        {
            if (_skinsService == null || string.IsNullOrEmpty(_selectedSkinId)) return;

            var equippedSkin = _skinsService.GetEquippedSkin(0);
            if (equippedSkin?.id == _selectedSkinId)
            {
                GameLogger.Log("Skin already equipped");
                return;
            }

            bool success = _skinsService.EquipSkin(0, _selectedSkinId);

            if (success)
            {
                GameLogger.Log($"Equipped skin: {_selectedSkinId}");
                UpdateEquipButton();
                PopulateSkins();
            }
            else
            {
                GameLogger.LogWarning($"Failed to equip skin: {_selectedSkinId}");
            }
        }

        public void Dispose()
        {
            if (_equipButton != null)
            {
                _equipButton.clicked -= OnEquipButtonClicked;
            }

            GameLogger.Log("Disposed");
        }
    }
}
