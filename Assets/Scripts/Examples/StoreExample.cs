using System.Collections.Generic;
using RecipeRage.Modules.Logging;
using RecipeRage.Store;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script that demonstrates how to use the Store module
    /// </summary>
    public class StoreExample : MonoBehaviour
    {
        private const string LOG_TAG = "StoreExample";

        [Header("Store Settings")]
        [SerializeField]
        private string _defaultOfferId = "example_offer";

        [SerializeField] private string _defaultItemId = "example_item";
        [SerializeField] private int _consumeQuantity = 1;

        [Header("UI Components")]
        [SerializeField]
        private InputField _offerIdInput;

        [SerializeField] private InputField _itemIdInput;
        [SerializeField] private InputField _consumeQuantityInput;
        [SerializeField] private Button _initButton;
        [SerializeField] private Button _queryCatalogButton;
        [SerializeField] private Button _queryInventoryButton;
        [SerializeField] private Button _queryOffersButton;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _consumeButton;
        [SerializeField] private Button _restoreButton;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _resultText;
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private ScrollRect _catalogScrollView;
        [SerializeField] private ScrollRect _inventoryScrollView;
        [SerializeField] private ScrollRect _offersScrollView;
        [SerializeField] private GameObject _catalogItemPrefab;
        [SerializeField] private GameObject _inventoryItemPrefab;
        [SerializeField] private GameObject _offerItemPrefab;
        [SerializeField] private Transform _catalogContent;
        [SerializeField] private Transform _inventoryContent;
        [SerializeField] private Transform _offersContent;

        // Cached data
        private List<CatalogItem> _catalogItems = new List<CatalogItem>();

        // Flag to track if initialization is in progress
        private bool _initializingStore;
        private List<InventoryItem> _inventoryItems = new List<InventoryItem>();
        private List<StoreOffer> _offers = new List<StoreOffer>();

        private void OnEnable()
        {
            // Register event handlers with wrapper methods to match new signatures
            StoreHelper.RegisterCatalogQueryCallback(items => OnCatalogQueried(items, true));
            StoreHelper.RegisterInventoryQueryCallback(items => OnInventoryQueried(items, true));
            StoreHelper.RegisterOffersQueryCallback(offers => OnOffersQueried(offers, true));
            StoreHelper.RegisterPurchaseSuccessCallback(OnPurchaseSuccess);
            StoreHelper.RegisterPurchaseFailedCallback((offerId, error) => OnPurchaseFailure(new PurchaseResult { OfferId = offerId, ErrorMessage = error }));
            StoreHelper.RegisterItemAddedCallback(OnItemAdded);
            StoreHelper.RegisterItemConsumedCallback(OnItemConsumed);
            StoreHelper.RegisterCurrencyBalanceChangedCallback(currencies => { }); // Empty handler

            // Initialize UI
            SetupUI();
        }

        private void OnDisable()
        {
            // Unregister event handlers with matching wrapper methods
            StoreHelper.UnregisterCatalogQueryCallback(items => OnCatalogQueried(items, true));
            StoreHelper.UnregisterInventoryQueryCallback(items => OnInventoryQueried(items, true));
            StoreHelper.UnregisterOffersQueryCallback(offers => OnOffersQueried(offers, true));
            StoreHelper.UnregisterPurchaseSuccessCallback(OnPurchaseSuccess);
            StoreHelper.UnregisterPurchaseFailedCallback((offerId, error) => OnPurchaseFailure(new PurchaseResult { OfferId = offerId, ErrorMessage = error }));
            StoreHelper.UnregisterItemAddedCallback(OnItemAdded);
            StoreHelper.UnregisterItemConsumedCallback(OnItemConsumed);
            StoreHelper.UnregisterCurrencyBalanceChangedCallback(currencies => { }); // Empty handler
        }

        private void SetupUI()
        {
            // Set default values
            _offerIdInput.text = _defaultOfferId;
            _itemIdInput.text = _defaultItemId;
            _consumeQuantityInput.text = _consumeQuantity.ToString();

            // Set button click handlers
            _initButton.onClick.AddListener(InitializeStore);
            _queryCatalogButton.onClick.AddListener(QueryCatalog);
            _queryInventoryButton.onClick.AddListener(QueryInventory);
            _queryOffersButton.onClick.AddListener(QueryOffers);
            _purchaseButton.onClick.AddListener(PurchaseOffer);
            _consumeButton.onClick.AddListener(ConsumeItem);
            _restoreButton.onClick.AddListener(RestorePurchases);

            // Update UI state
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            bool isInitialized = StoreHelper.IsInitialized;
            bool isInitializing = _initializingStore;

            _initButton.interactable = !isInitialized && !isInitializing;
            _queryCatalogButton.interactable = isInitialized;
            _queryInventoryButton.interactable = isInitialized;
            _queryOffersButton.interactable = isInitialized;
            _purchaseButton.interactable = isInitialized && !string.IsNullOrEmpty(_offerIdInput.text) &&
                                           !StoreHelper.IsPurchaseInProgress();
            _consumeButton.interactable = isInitialized && _inventoryItems.Count > 0 &&
                                          !string.IsNullOrEmpty(_itemIdInput.text) &&
                                          int.TryParse(_consumeQuantityInput.text, out _);
            _restoreButton.interactable = isInitialized;

            _loadingPanel.SetActive(isInitializing || StoreHelper.IsPurchaseInProgress());

            _statusText.text = isInitialized ? "Store Initialized" :
                isInitializing ? "Initializing Store..." : "Store Not Initialized";
        }

        private void InitializeStore()
        {
            if (StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store already initialized");
                return;
            }

            _initializingStore = true;
            UpdateUIState();

            LogHelper.Info(LOG_TAG, "Initializing store");
            _statusText.text = "Initializing store...";
            _resultText.text = "";

            StoreHelper.Initialize(success =>
            {
                _initializingStore = false;

                if (success)
                {
                    LogHelper.Info(LOG_TAG, "Store initialized successfully");
                    _statusText.text = "Store initialized successfully";

                    // Auto-query catalog, inventory, and offers
                    QueryCatalog();
                    QueryInventory();
                    QueryOffers();
                }
                else
                {
                    LogHelper.Error(LOG_TAG, "Failed to initialize store");
                    _statusText.text = "Failed to initialize store";
                }

                UpdateUIState();
            });
        }

        private void QueryCatalog()
        {
            if (!StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store not initialized");
                return;
            }

            LogHelper.Info(LOG_TAG, "Querying catalog");
            _statusText.text = "Querying catalog...";
            _resultText.text = "";

            StoreHelper.QueryCatalog(items =>
            {
                LogHelper.Info(LOG_TAG, $"Successfully queried catalog. Found {items.Count} items.");
                _statusText.text = $"Catalog query successful. Found {items.Count} items.";
                OnCatalogQueried(items, true);
                UpdateUIState();
            });
        }

        private void QueryInventory()
        {
            if (!StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store not initialized");
                return;
            }

            LogHelper.Info(LOG_TAG, "Querying inventory");
            _statusText.text = "Querying inventory...";
            _resultText.text = "";

            StoreHelper.QueryInventory(items =>
            {
                LogHelper.Info(LOG_TAG, $"Successfully queried inventory. Found {items.Count} items.");
                _statusText.text = $"Inventory query successful. Found {items.Count} items.";
                OnInventoryQueried(items, true);
                UpdateUIState();
            });
        }

        private void QueryOffers()
        {
            if (!StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store not initialized");
                return;
            }

            LogHelper.Info(LOG_TAG, "Querying offers");
            _statusText.text = "Querying offers...";
            _resultText.text = "";

            StoreHelper.QueryOffers(offers =>
            {
                LogHelper.Info(LOG_TAG, $"Successfully queried offers. Found {offers.Count} offers.");
                _statusText.text = $"Offers query successful. Found {offers.Count} offers.";
                OnOffersQueried(offers, true);
                UpdateUIState();
            });
        }

        private void PurchaseOffer()
        {
            if (!StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store not initialized");
                return;
            }

            string offerId = _offerIdInput.text;
            if (string.IsNullOrEmpty(offerId))
            {
                LogHelper.Warning(LOG_TAG, "Offer ID is empty");
                _statusText.text = "Offer ID is empty";
                return;
            }

            if (StoreHelper.IsPurchaseInProgress())
            {
                LogHelper.Warning(LOG_TAG, "A purchase is already in progress");
                _statusText.text = "A purchase is already in progress";
                return;
            }

            LogHelper.Info(LOG_TAG, $"Purchasing offer: {offerId}");
            _statusText.text = $"Purchasing offer: {offerId}...";
            _resultText.text = "";

            UpdateUIState();

            StoreHelper.PurchaseOffer(offerId, result =>
            {
                if (result.Status == PurchaseStatus.Success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully purchased offer: {offerId}");
                    _statusText.text = $"Purchase successful: {offerId}";
                    _resultText.text =
                        $"Transaction ID: {result.TransactionId}\nGranted Items: {result.GrantedItems.Count}";

                    // Update inventory
                    QueryInventory();
                }
                else
                {
                    LogHelper.Error(LOG_TAG,
                        $"Failed to purchase offer: {offerId}. Status: {result.Status}. Error: {result.ErrorMessage}");
                    _statusText.text = $"Purchase failed: {offerId}";
                    _resultText.text = $"Status: {result.Status}\nError: {result.ErrorMessage}";
                }

                UpdateUIState();
            });
        }

        private void ConsumeItem()
        {
            if (!StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store not initialized");
                return;
            }

            string itemId = _itemIdInput.text;
            if (string.IsNullOrEmpty(itemId))
            {
                LogHelper.Warning(LOG_TAG, "Item ID is empty");
                _statusText.text = "Item ID is empty";
                return;
            }

            if (!int.TryParse(_consumeQuantityInput.text, out int quantity) || quantity <= 0)
            {
                LogHelper.Warning(LOG_TAG, "Invalid consume quantity");
                _statusText.text = "Invalid consume quantity";
                return;
            }

            // Find the inventory item with the given item ID
            var inventoryItem = _inventoryItems.Find(item => item.CatalogItemId == itemId);
            if (inventoryItem == null)
            {
                LogHelper.Warning(LOG_TAG, $"Item {itemId} not found in inventory");
                _statusText.text = $"Item {itemId} not found in inventory";
                return;
            }

            LogHelper.Info(LOG_TAG, $"Consuming item: {inventoryItem.InventoryItemId}, quantity: {quantity}");
            _statusText.text = $"Consuming item: {inventoryItem.InventoryItemId}, quantity: {quantity}...";
            _resultText.text = "";

            StoreHelper.ConsumeItem(inventoryItem.InventoryItemId, quantity, success =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG,
                        $"Successfully consumed {quantity} of item {inventoryItem.InventoryItemId}");
                    _statusText.text = $"Item consumed successfully: {quantity} of {inventoryItem.InventoryItemId}";

                    // Update inventory
                    QueryInventory();
                }
                else
                {
                    LogHelper.Error(LOG_TAG, $"Failed to consume item: {inventoryItem.InventoryItemId}");
                    _statusText.text = $"Failed to consume item: {inventoryItem.InventoryItemId}";
                }

                UpdateUIState();
            });
        }

        private void RestorePurchases()
        {
            if (!StoreHelper.IsInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store not initialized");
                return;
            }

            LogHelper.Info(LOG_TAG, "Restoring purchases");
            _statusText.text = "Restoring purchases...";
            _resultText.text = "";

            StoreHelper.RestorePurchases(success =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, "Purchases restored successfully");
                    _statusText.text = "Purchases restored successfully";

                    // Refresh inventory
                    QueryInventory();
                }
                else
                {
                    LogHelper.Error(LOG_TAG, "Failed to restore purchases");
                    _statusText.text = "Failed to restore purchases";
                }

                UpdateUIState();
            });
        }

        private void OnItemConsumed(string itemId, int quantity)
        {
            LogHelper.Info(LOG_TAG, $"Item consumed event: {itemId}, quantity: {quantity}");

            // Update inventory
            QueryInventory();
        }

        // Event handlers
        private void OnCatalogQueried(List<CatalogItem> items, bool success)
        {
            if (success)
            {
                _catalogItems = items;
                UpdateCatalogUI();
            }
        }

        private void OnInventoryQueried(List<InventoryItem> items, bool success)
        {
            if (success)
            {
                _inventoryItems = items;
                UpdateInventoryUI();
            }

            UpdateUIState();
        }

        private void OnOffersQueried(List<StoreOffer> offers, bool success)
        {
            if (success)
            {
                _offers = offers;
                UpdateOffersUI();

                // If we have offers, set the first one as default
                if (offers.Count > 0 && string.IsNullOrEmpty(_offerIdInput.text))
                    _offerIdInput.text = offers[0].OfferId;
            }
        }

        private void OnPurchaseSuccess(PurchaseResult result)
        {
            LogHelper.Info(LOG_TAG, $"Purchase success event: {result.OfferId}");

            // Update inventory
            QueryInventory();
        }

        private void OnPurchaseFailure(PurchaseResult result)
        {
            LogHelper.Error(LOG_TAG,
                $"Purchase failure event: {result.OfferId}. Status: {result.Status}. Error: {result.ErrorMessage}");
        }

        private void OnItemAdded(InventoryItem item)
        {
            LogHelper.Info(LOG_TAG, $"Item added event: {item.InventoryItemId}");

            // Update inventory
            QueryInventory();
        }

        // UI update methods
        private void UpdateCatalogUI()
        {
            if (_catalogContent == null || _catalogItemPrefab == null)
                return;

            // Clear existing content
            foreach (Transform child in _catalogContent) Destroy(child.gameObject);

            // Create UI for each catalog item
            foreach (var item in _catalogItems)
            {
                var itemObject = Instantiate(_catalogItemPrefab, _catalogContent);

                // Set item details
                var itemName = itemObject.transform.Find("ItemName")?.GetComponent<Text>();
                var itemDescription = itemObject.transform.Find("ItemDescription")?.GetComponent<Text>();
                var itemType = itemObject.transform.Find("ItemType")?.GetComponent<Text>();
                var itemId = itemObject.transform.Find("ItemId")?.GetComponent<Text>();
                var selectButton = itemObject.transform.Find("SelectButton")?.GetComponent<Button>();

                if (itemName != null)
                    itemName.text = item.DisplayName;

                if (itemDescription != null)
                    itemDescription.text = item.Description;

                if (itemType != null)
                    itemType.text = item.ItemType.ToString();

                if (itemId != null)
                    itemId.text = item.ItemId;

                if (selectButton != null)
                    selectButton.onClick.AddListener(() => { _itemIdInput.text = item.ItemId; });
            }
        }

        private void UpdateInventoryUI()
        {
            if (_inventoryContent == null || _inventoryItemPrefab == null)
                return;

            // Clear existing content
            foreach (Transform child in _inventoryContent) Destroy(child.gameObject);

            // Create UI for each inventory item
            foreach (var item in _inventoryItems)
            {
                var itemObject = Instantiate(_inventoryItemPrefab, _inventoryContent);

                // Set item details
                var itemName = itemObject.transform.Find("ItemName")?.GetComponent<Text>();
                var itemQuantity = itemObject.transform.Find("ItemQuantity")?.GetComponent<Text>();
                var itemId = itemObject.transform.Find("ItemId")?.GetComponent<Text>();
                var selectButton = itemObject.transform.Find("SelectButton")?.GetComponent<Button>();
                var consumeButton = itemObject.transform.Find("ConsumeButton")?.GetComponent<Button>();

                string itemNameText = "Unknown Item";
                if (item.CatalogItem != null)
                    itemNameText = item.CatalogItem.DisplayName;

                if (itemName != null)
                    itemName.text = itemNameText;

                if (itemQuantity != null)
                    itemQuantity.text = $"Quantity: {item.Quantity}";

                if (itemId != null)
                    itemId.text = $"ID: {item.CatalogItemId}";

                if (selectButton != null)
                    selectButton.onClick.AddListener(() => { _itemIdInput.text = item.CatalogItemId; });

                if (consumeButton != null)
                    consumeButton.onClick.AddListener(() =>
                    {
                        _itemIdInput.text = item.CatalogItemId;
                        ConsumeItem();
                    });
            }
        }

        private void UpdateOffersUI()
        {
            if (_offersContent == null || _offerItemPrefab == null)
                return;

            // Clear existing content
            foreach (Transform child in _offersContent) Destroy(child.gameObject);

            // Create UI for each offer
            foreach (var offer in _offers)
            {
                var offerObject = Instantiate(_offerItemPrefab, _offersContent);

                // Set offer details
                var offerName = offerObject.transform.Find("OfferName")?.GetComponent<Text>();
                var offerPrice = offerObject.transform.Find("OfferPrice")?.GetComponent<Text>();
                var offerItems = offerObject.transform.Find("OfferItems")?.GetComponent<Text>();
                var offerId = offerObject.transform.Find("OfferId")?.GetComponent<Text>();
                var selectButton = offerObject.transform.Find("SelectButton")?.GetComponent<Button>();
                var purchaseButton = offerObject.transform.Find("PurchaseButton")?.GetComponent<Button>();

                if (offerName != null)
                    offerName.text = offer.DisplayName;

                if (offerPrice != null)
                    offerPrice.text = $"{offer.CurrentPrice} {offer.CurrencyCode}";

                if (offerItems != null)
                {
                    string itemsText = "Items: ";
                    foreach (var item in offer.Items) itemsText += $"{item.ItemId} (x{item.Quantity}), ";

                    if (offer.Items.Count > 0)
                        itemsText = itemsText.Substring(0, itemsText.Length - 2);

                    offerItems.text = itemsText;
                }

                if (offerId != null)
                    offerId.text = $"ID: {offer.OfferId}";

                if (selectButton != null)
                    selectButton.onClick.AddListener(() => { _offerIdInput.text = offer.OfferId; });

                if (purchaseButton != null)
                    purchaseButton.onClick.AddListener(() =>
                    {
                        _offerIdInput.text = offer.OfferId;
                        PurchaseOffer();
                    });
            }
        }
    }
}