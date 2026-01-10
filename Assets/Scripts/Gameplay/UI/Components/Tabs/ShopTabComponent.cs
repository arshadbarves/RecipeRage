using UnityEngine;
using UnityEngine.UIElements;
using Gameplay.UI.Data;
using Gameplay.UI.Features.Shop;
using Core.Logging;

namespace Gameplay.UI.Components.Tabs
{
    /// <summary>
    /// Shop tab content component
    /// Renamed from ShopUI for consistency
    /// </summary>
    public class ShopTabComponent
    {
        private VisualElement _root;
        private VisualElement _shopItemsGrid;
        private string _currentCategory = "featured";
        private ShopData _shopData;
        private VisualTreeAsset _shopItemTemplate;

        private readonly ShopViewModel _viewModel;

        public ShopTabComponent(ShopViewModel viewModel)
        {
            _viewModel = viewModel;
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

            _shopItemsGrid = _root.Q<VisualElement>("shop-items-grid");

            if (_shopItemsGrid == null)
            {
                GameLogger.LogError("Shop items grid not found");
                return;
            }

            GameLogger.Log("Shop items grid found");

            LoadTemplate();
            LoadShopData();
            SetupCategoryButtons();
            PopulateShopItems();
        }

        private void LoadTemplate()
        {
            _shopItemTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/ShopItem");
            if (_shopItemTemplate == null)
            {
                GameLogger.LogError("Failed to load ShopItem template!");
            }
            else
            {
                GameLogger.Log("ShopItem template loaded successfully");
            }
        }

        private void LoadShopData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/ShopItems");
            if (jsonFile != null)
            {
                _shopData = JsonUtility.FromJson<ShopData>(jsonFile.text);
                GameLogger.Log($"Loaded {_shopData.categories.Count} shop categories");
            }
            else
            {
                GameLogger.LogError("Failed to load ShopItems.json from Resources/Data");
            }
        }

        private void SetupCategoryButtons()
        {
            Button featuredButton = _root.Q<Button>("featured-button");
            Button skinsButton = _root.Q<Button>("skins-button");
            Button weaponsButton = _root.Q<Button>("weapons-button");
            Button bundlesButton = _root.Q<Button>("bundles-button");

            GameLogger.Log($"Setting up category buttons - Featured: {featuredButton != null}, Skins: {skinsButton != null}, Weapons: {weaponsButton != null}, Bundles: {bundlesButton != null}");

            if (featuredButton != null)
            {
                featuredButton.clicked += () => OnCategorySelected("featured", featuredButton);
                GameLogger.Log("Featured button listener added");
            }
            if (skinsButton != null)
            {
                skinsButton.clicked += () => OnCategorySelected("skins", skinsButton);
                GameLogger.Log("Skins button listener added");
            }
            if (weaponsButton != null)
            {
                weaponsButton.clicked += () => OnCategorySelected("weapons", weaponsButton);
                GameLogger.Log("Weapons button listener added");
            }
            if (bundlesButton != null)
            {
                bundlesButton.clicked += () => OnCategorySelected("bundles", bundlesButton);
                GameLogger.Log("Bundles button listener added");
            }
        }

        private void OnCategorySelected(string category, Button selectedButton)
        {
            _currentCategory = category;

            // Update button states
            Button featuredButton = _root.Q<Button>("featured-button");
            Button skinsButton = _root.Q<Button>("skins-button");
            Button weaponsButton = _root.Q<Button>("weapons-button");
            Button bundlesButton = _root.Q<Button>("bundles-button");

            featuredButton?.RemoveFromClassList("category-active");
            skinsButton?.RemoveFromClassList("category-active");
            weaponsButton?.RemoveFromClassList("category-active");
            bundlesButton?.RemoveFromClassList("category-active");

            selectedButton?.AddToClassList("category-active");

            GameLogger.Log($"Category selected: {category}");
            PopulateShopItems();
        }

        private void PopulateShopItems()
        {
            if (_shopItemsGrid == null || _shopData == null) return;

            _shopItemsGrid.Clear();

            // Find the category
            ShopCategory category = _shopData.categories.Find(c => c.id == _currentCategory);
            if (category == null)
            {
                GameLogger.LogWarning($"Category not found: {_currentCategory}");
                return;
            }

            // Create UI elements for each item
            foreach (ShopItem item in category.items)
            {
                VisualElement shopItem = CreateShopItem(item);
                _shopItemsGrid.Add(shopItem);
            }

            GameLogger.Log($"Populated {category.items.Count} items for category: {_currentCategory}");
        }

        private VisualElement CreateShopItem(ShopItem itemData)
        {
            if (_shopItemTemplate == null)
            {
                GameLogger.LogError("ShopItem template is null!");
                return new VisualElement();
            }

            bool isOwned = PlayerPrefs.GetInt($"Owned_{itemData.id}", 0) == 1;

            // Clone template
            TemplateContainer itemContainer = _shopItemTemplate.CloneTree();
            Button shopItemButton = itemContainer.Q<Button>("shop-item");

            if (shopItemButton == null)
            {
                GameLogger.LogError("Failed to find 'shop-item' button in template!");
                return itemContainer;
            }

            // Add rarity class
            string rarityClass = GetRarityClass(itemData.rarity);
            shopItemButton.AddToClassList(rarityClass);

            // Setup click handler
            shopItemButton.clicked += () => OnBuyItem(itemData);

            // Get elements from template
            Label itemBadge = itemContainer.Q<Label>("item-badge");
            Label itemName = itemContainer.Q<Label>("item-name");
            Label itemDescription = itemContainer.Q<Label>("item-description");
            Label priceValue = itemContainer.Q<Label>("price-value");

            // Populate data
            if (itemName != null)
                itemName.text = itemData.name.ToUpper();

            if (itemDescription != null)
                itemDescription.text = itemData.description;

            if (priceValue != null)
                priceValue.text = isOwned ? "OWNED" : itemData.price.ToString();

            // Show/hide badge
            if (itemBadge != null)
            {
                if (!string.IsNullOrEmpty(itemData.badge))
                {
                    itemBadge.text = itemData.badge.ToUpper();
                    itemBadge.RemoveFromClassList("hidden");

                    // Add badge type class
                    if (itemData.badge.ToLower() == "sale")
                        itemBadge.AddToClassList("sale");
                    else if (itemData.badge.ToLower() == "new")
                        itemBadge.AddToClassList("new");
                    else if (itemData.badge.ToLower() == "limited")
                        itemBadge.AddToClassList("limited");
                }
                else
                {
                    itemBadge.AddToClassList("hidden");
                }
            }

            // Disable if owned
            if (isOwned)
            {
                shopItemButton.SetEnabled(false);
            }

            return itemContainer;
        }

        private string GetRarityClass(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common" => "common",
                "rare" => "rare",
                "epic" => "epic",
                "legendary" => "legendary",
                _ => "common"
            };
        }

        private void OnBuyItem(ShopItem item)
        {
            GameLogger.Log($"Attempting to buy {item.name} for {item.price} {item.currency}");

            bool purchaseSuccess = _viewModel.BuyItem(item);

            if (purchaseSuccess)
            {
                GameLogger.Log($"Successfully purchased {item.name}");
                PopulateShopItems();
            }
            else
            {
                GameLogger.LogWarning($"Not enough {item.currency} to buy {item.name}");
            }
        }

        public void Dispose()
        {
            GameLogger.Log("Disposed");
        }
    }
}
