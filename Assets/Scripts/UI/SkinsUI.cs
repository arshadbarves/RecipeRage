using UnityEngine;
using UnityEngine.UIElements;
using UI.Data;

namespace UI
{
    /// <summary>
    /// Skins tab content - embedded directly in MainMenu tabs
    /// Loads skins dynamically from JSON data
    /// </summary>
    public class SkinsUI
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
            Debug.Log("[SkinsUI] Initialize called");
            
            if (root == null)
            {
                Debug.LogError("[SkinsUI] Root is null!");
                return;
            }
            
            _root = root;
            Debug.Log($"[SkinsUI] Root element: {_root.name}");
            
            _skinsGrid = _root.Q<VisualElement>("skins-grid");
            _skinNameLabel = _root.Q<Label>("skin-name");
            _equipButton = _root.Q<Button>("equip-button");

            Debug.Log($"[SkinsUI] Elements found - Grid: {_skinsGrid != null}, Label: {_skinNameLabel != null}, Button: {_equipButton != null}");

            if (_skinsGrid == null)
            {
                Debug.LogError("[SkinsUI] Skins grid not found");
                return;
            }

            LoadSkinsData();
            
            _equippedSkinId = PlayerPrefs.GetString("EquippedSkin", "classic_chef");
            _selectedSkinId = _equippedSkinId;

            if (_equipButton != null)
            {
                _equipButton.clicked += OnEquipButtonClicked;
                Debug.Log("[SkinsUI] Equip button listener added");
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
                Debug.Log($"[SkinsUI] Loaded {_skinsData.skins.Count} skins");
            }
            else
            {
                Debug.LogError("[SkinsUI] Failed to load Skins.json from Resources/Data");
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
            
            Debug.Log($"[SkinsUI] Populated {_skinsData.skins.Count} skins");
        }

        private VisualElement CreateSkinItem(SkinItem skin)
        {
            // Check if skin is unlocked (either by default or purchased)
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
            // TODO: Load icon if available
            // if (!string.IsNullOrEmpty(skin.icon))
            // {
            //     image.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>(skin.icon));
            // }
            skinItem.Add(image);
            
            // Add lock icon if locked
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
            // Check if skin is unlocked
            bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;
            
            if (!isUnlocked)
            {
                Debug.Log($"[SkinsUI] Skin is locked: {skin.name}");
                // TODO: Show "Skin is locked" message
                return;
            }
            
            _selectedSkinId = skin.id;
            UpdateSkinNameDisplay();
            UpdateEquipButton();
            PopulateSkins();

            Debug.Log($"[SkinsUI] Skin selected: {skin.name}");
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

            // Check if selected skin is unlocked
            if (_skinsData != null)
            {
                SkinItem skin = _skinsData.skins.Find(s => s.id == _selectedSkinId);
                if (skin != null)
                {
                    bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;
                    if (!isUnlocked)
                    {
                        Debug.LogWarning($"[SkinsUI] Cannot equip locked skin: {skin.name}");
                        return;
                    }
                }
            }

            Debug.Log($"[SkinsUI] Equipping skin: {_selectedSkinId}");
            _equippedSkinId = _selectedSkinId;
            PlayerPrefs.SetString("EquippedSkin", _selectedSkinId);
            PlayerPrefs.Save();

            UpdateEquipButton();
            PopulateSkins();
        }
    }
}
