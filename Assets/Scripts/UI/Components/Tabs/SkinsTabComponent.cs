using Core.Logging;
using Core.Skins.Data;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Data;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Skins tab content component
    /// Renamed from SkinsUI for consistency
    /// </summary>
    public class SkinsTabComponent
    {
        private VisualElement _root;
        private VisualElement _skinsGrid;
        private Label _skinNameLabel;
        private Button _equipButton;
        private string _selectedSkinId;
        private string _equippedSkinId;
        private SkinsData _skinsData;

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

            LoadSkinsData();

            _equippedSkinId = PlayerPrefs.GetString("EquippedSkin", "classic_chef");
            _selectedSkinId = _equippedSkinId;

            if (_equipButton != null)
            {
                _equipButton.clicked += OnEquipButtonClicked;
                GameLogger.Log("Equip button listener added");
            }

            UpdateSkinNameDisplay();
            PopulateSkins();
            UpdateEquipButton();
        }

        private void LoadSkinsData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Skins");
            if (jsonFile != null)
            {
                _skinsData = JsonUtility.FromJson<SkinsData>(jsonFile.text);
                GameLogger.Log($"Loaded {_skinsData.skins.Count} skins");
            }
            else
            {
                GameLogger.LogError("Failed to load Skins.json from Resources/Data");
            }
        }

        private void UpdateSkinNameDisplay()
        {
            if (_skinNameLabel != null && _skinsData != null)
            {
                SkinItem skin = _skinsData.skins.Find(s => s.id == _selectedSkinId);
                if (skin != null)
                {
                    _skinNameLabel.text = skin.name.ToUpper();
                }
            }
        }

        private void PopulateSkins()
        {
            if (_skinsGrid == null || _skinsData == null) return;

            _skinsGrid.Clear();

            foreach (SkinItem skin in _skinsData.skins)
            {
                VisualElement skinItem = CreateSkinItem(skin);
                _skinsGrid.Add(skinItem);
            }

            GameLogger.Log($"Populated {_skinsData.skins.Count} skins");
        }

        private VisualElement CreateSkinItem(SkinItem skin)
        {
            bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;

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
            }

            Label nameLabel = new Label(skin.name.ToUpper());
            nameLabel.AddToClassList("skin-item-name");
            skinItem.Add(nameLabel);

            return skinItem;
        }

        private void OnSkinSelected(SkinItem skin)
        {
            bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;

            if (!isUnlocked)
            {
                GameLogger.Log($"Skin is locked: {skin.name}");
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
            if (_equipButton == null) return;

            bool isEquipped = _selectedSkinId == _equippedSkinId;
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
            if (_selectedSkinId == _equippedSkinId) return;

            if (_skinsData != null)
            {
                SkinItem skin = _skinsData.skins.Find(s => s.id == _selectedSkinId);
                if (skin != null)
                {
                    bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;
                    if (!isUnlocked)
                    {
                        GameLogger.LogWarning($"Cannot equip locked skin: {skin.name}");
                        return;
                    }
                }
            }

            GameLogger.Log($"Equipping skin: {_selectedSkinId}");
            _equippedSkinId = _selectedSkinId;
            PlayerPrefs.SetString("EquippedSkin", _selectedSkinId);
            PlayerPrefs.Save();

            UpdateEquipButton();
            PopulateSkins();
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
