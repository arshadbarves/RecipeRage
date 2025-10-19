using UnityEngine;
using UnityEngine.UIElements;
using UI.Data;

namespace UI.Components
{
    /// <summary>
    /// Shop tab content - embedded directly in MainMenu tabs
    /// Loads items dynamically from JSON data
    /// </summary>
    public class ShopUI
    {
        private VisualElement _root;
        private VisualElement _shopItemsGrid;
        private string _currentCategory = "featured";
        private ShopData _shopData;

        public void Initialize(VisualElement root)
        {
            Debug.Log("[ShopUI] Initialize called");
            
            if (root == null)
            {
                Debug.LogError("[ShopUI] Root is null!");
                return;
            }
            
            _root = root;
            Debug.Log($"[ShopUI] Root element: {_root.name}");
            
            _shopItemsGrid = _root.Q<VisualElement>("shop-items-grid");

            if (_shopItemsGrid == null)
            {
                Debug.LogError("[ShopUI] Shop items grid not found");
                return;
            }

            Debug.Log("[ShopUI] Shop items grid found");
            
            LoadShopData();
            SetupCategoryButtons();
            PopulateShopItems();
        }
        
        private void LoadShopData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/ShopItems");
            if (jsonFile != null)
            {
                _shopData = JsonUtility.FromJson<ShopData>(jsonFile.text);
                Debug.Log($"[ShopUI] Loaded {_shopData.categories.Count} shop categories");
            }
            else
            {
                Debug.LogError("[ShopUI] Failed to load ShopItems.json from Resources/Data");
            }
        }

        private void SetupCategoryButtons()
        {
            Button featuredButton = _root.Q<Button>("featured-button");
            Button skinsButton = _root.Q<Button>("skins-button");
            Button weaponsButton = _root.Q<Button>("weapons-button");
            Button bundlesButton = _root.Q<Button>("bundles-button");

            Debug.Log($"[ShopUI] Setting up category buttons - Featured: {featuredButton != null}, Skins: {skinsButton != null}, Weapons: {weaponsButton != null}, Bundles: {bundlesButton != null}");

            if (featuredButton != null) 
            {
                featuredButton.clicked += () => OnCategorySelected("featured", featuredButton);
                Debug.Log("[ShopUI] Featured button listener added");
            }
            if (skinsButton != null) 
            {
                skinsButton.clicked += () => OnCategorySelected("skins", skinsButton);
                Debug.Log("[ShopUI] Skins button listener added");
            }
            if (weaponsButton != null) 
            {
                weaponsButton.clicked += () => OnCategorySelected("weapons", weaponsButton);
                Debug.Log("[ShopUI] Weapons button listener added");
            }
            if (bundlesButton != null) 
            {
                bundlesButton.clicked += () => OnCategorySelected("bundles", bundlesButton);
                Debug.Log("[ShopUI] Bundles button listener added");
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

            Debug.Log($"[ShopUI] Category selected: {category}");
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
                Debug.LogWarning($"[ShopUI] Category not found: {_currentCategory}");
                return;
            }

            // Create UI elements for each item
            foreach (ShopItem item in category.items)
            {
                VisualElement shopItem = CreateShopItem(item);
                _shopItemsGrid.Add(shopItem);
            }
            
            Debug.Log($"[ShopUI] Populated {category.items.Count} items for category: {_currentCategory}");
        }

        private VisualElement CreateShopItem(ShopItem itemData)
        {
            bool isOwned = PlayerPrefs.GetInt($"Owned_{itemData.id}", 0) == 1;

            VisualElement item = new VisualElement();
            item.AddToClassList("shop-item");

            VisualElement image = new VisualElement();
            image.AddToClassList("shop-item-image");
            // TODO: Load icon if available
            // if (!string.IsNullOrEmpty(itemData.icon))
            // {
            //     image.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>(itemData.icon));
            // }
            item.Add(image);

            Label nameLabel = new Label(itemData.name.ToUpper());
            nameLabel.AddToClassList("shop-item-name");
            item.Add(nameLabel);

            VisualElement priceContainer = new VisualElement();
            priceContainer.AddToClassList("shop-item-price");

            VisualElement priceIcon = new VisualElement();
            priceIcon.AddToClassList("price-icon");
            // Add gem icon class if currency is gems
            if (itemData.currency == "gems")
            {
                priceIcon.AddToClassList("gem-icon");
            }
            priceContainer.Add(priceIcon);

            Label priceLabel = new Label(itemData.price.ToString());
            priceLabel.AddToClassList("price-text");
            priceContainer.Add(priceLabel);

            item.Add(priceContainer);

            Button buyButton = new Button(() => OnBuyItem(itemData));
            buyButton.text = isOwned ? "OWNED" : "BUY";
            buyButton.AddToClassList("buy-button");
            buyButton.SetEnabled(!isOwned);

            if (isOwned)
            {
                buyButton.style.backgroundColor = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            }

            item.Add(buyButton);

            return item;
        }

        private void OnBuyItem(ShopItem item)
        {
            Debug.Log($"[ShopUI] Attempting to buy {item.name} for {item.price} {item.currency}");

            var currencyService = Core.Bootstrap.GameBootstrap.Services?.CurrencyService;
            if (currencyService == null)
            {
                Debug.LogError("[ShopUI] CurrencyService not found");
                return;
            }

            bool purchaseSuccess = false;
            
            // Check currency type and spend accordingly
            if (item.currency == "coins")
            {
                purchaseSuccess = currencyService.SpendCoins(item.price);
            }
            else if (item.currency == "gems")
            {
                purchaseSuccess = currencyService.SpendGems(item.price);
            }

            if (purchaseSuccess)
            {
                Debug.Log($"[ShopUI] Successfully purchased {item.name}");
                
                // Mark item as owned (TODO: Use SaveService instead of PlayerPrefs)
                PlayerPrefs.SetInt($"Owned_{item.id}", 1);
                
                // If it's a skin, also unlock it
                if (item.type == "skin")
                {
                    PlayerPrefs.SetInt($"Unlocked_{item.id}", 1);
                }
                
                PlayerPrefs.Save();

                // Refresh shop to show owned items
                PopulateShopItems();
            }
            else
            {
                Debug.LogWarning($"[ShopUI] Not enough {item.currency} to buy {item.name}");
                // TODO: Show "not enough currency" message to user
            }
        }
    }
}
