using Core.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Data;

namespace UI.Components.Tabs
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

        public void Initialize(VisualElement root)
        {
            Debug.Log("[ShopTabComponent] Initialize called");

            if (root == null)
            {
                Debug.LogError("[ShopTabComponent] Root is null!");
                return;
            }

            _root = root;
            Debug.Log($"[ShopTabComponent] Root element: {_root.name}");

            _shopItemsGrid = _root.Q<VisualElement>("shop-items-grid");

            if (_shopItemsGrid == null)
            {
                Debug.LogError("[ShopTabComponent] Shop items grid not found");
                return;
            }

            Debug.Log("[ShopTabComponent] Shop items grid found");

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
                Debug.Log($"[ShopTabComponent] Loaded {_shopData.categories.Count} shop categories");
            }
            else
            {
                Debug.LogError("[ShopTabComponent] Failed to load ShopItems.json from Resources/Data");
            }
        }

        private void SetupCategoryButtons()
        {
            Button featuredButton = _root.Q<Button>("featured-button");
            Button skinsButton = _root.Q<Button>("skins-button");
            Button weaponsButton = _root.Q<Button>("weapons-button");
            Button bundlesButton = _root.Q<Button>("bundles-button");

            Debug.Log($"[ShopTabComponent] Setting up category buttons - Featured: {featuredButton != null}, Skins: {skinsButton != null}, Weapons: {weaponsButton != null}, Bundles: {bundlesButton != null}");

            if (featuredButton != null)
            {
                featuredButton.clicked += () => OnCategorySelected("featured", featuredButton);
                Debug.Log("[ShopTabComponent] Featured button listener added");
            }
            if (skinsButton != null)
            {
                skinsButton.clicked += () => OnCategorySelected("skins", skinsButton);
                Debug.Log("[ShopTabComponent] Skins button listener added");
            }
            if (weaponsButton != null)
            {
                weaponsButton.clicked += () => OnCategorySelected("weapons", weaponsButton);
                Debug.Log("[ShopTabComponent] Weapons button listener added");
            }
            if (bundlesButton != null)
            {
                bundlesButton.clicked += () => OnCategorySelected("bundles", bundlesButton);
                Debug.Log("[ShopTabComponent] Bundles button listener added");
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

            Debug.Log($"[ShopTabComponent] Category selected: {category}");
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
                Debug.LogWarning($"[ShopTabComponent] Category not found: {_currentCategory}");
                return;
            }

            // Create UI elements for each item
            foreach (ShopItem item in category.items)
            {
                VisualElement shopItem = CreateShopItem(item);
                _shopItemsGrid.Add(shopItem);
            }

            Debug.Log($"[ShopTabComponent] Populated {category.items.Count} items for category: {_currentCategory}");
        }

        private VisualElement CreateShopItem(ShopItem itemData)
        {
            bool isOwned = PlayerPrefs.GetInt($"Owned_{itemData.id}", 0) == 1;

            VisualElement item = new VisualElement();
            item.AddToClassList("shop-item");

            VisualElement image = new VisualElement();
            image.AddToClassList("shop-item-image");
            item.Add(image);

            Label nameLabel = new Label(itemData.name.ToUpper());
            nameLabel.AddToClassList("shop-item-name");
            item.Add(nameLabel);

            VisualElement priceContainer = new VisualElement();
            priceContainer.AddToClassList("shop-item-price");

            VisualElement priceIcon = new VisualElement();
            priceIcon.AddToClassList("price-icon");
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
            Debug.Log($"[ShopTabComponent] Attempting to buy {item.name} for {item.price} {item.currency}");

            var currencyService = GameBootstrap.Services?.CurrencyService;
            if (currencyService == null)
            {
                Debug.LogError("[ShopTabComponent] CurrencyService not found");
                return;
            }

            bool purchaseSuccess = false;

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
                Debug.Log($"[ShopTabComponent] Successfully purchased {item.name}");

                PlayerPrefs.SetInt($"Owned_{item.id}", 1);

                if (item.type == "skin")
                {
                    PlayerPrefs.SetInt($"Unlocked_{item.id}", 1);
                }

                PlayerPrefs.Save();
                PopulateShopItems();
            }
            else
            {
                Debug.LogWarning($"[ShopTabComponent] Not enough {item.currency} to buy {item.name}");
            }
        }

        public void Dispose()
        {
            Debug.Log("[ShopTabComponent] Disposed");
        }
    }
}
