using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Logging;

namespace RecipeRage.Store
{
    /// <summary>
    /// Static helper class for easy access to store functionality
    /// </summary>
    public static class StoreHelper
    {
        private const string LOG_TAG = "StoreHelper";
        
        // The store service instance
        private static IStoreService _storeService;
        
        // Initialization status
        private static bool _isInitialized;

        /// <summary>
        /// Static constructor
        /// </summary>
        static StoreHelper()
        {
            if (_storeService == null)
            {
                LogHelper.Debug(LOG_TAG, "Creating store service");
                _storeService = new StoreService();
            }
        }

        /// <summary>
        /// Gets whether the store service is initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Initializes the store service with the default providers
        /// </summary>
        /// <param name="callback">Callback when initialization completes</param>
        public static void Initialize(Action<bool> callback = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store service already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info(LOG_TAG, "Initializing store service");

            // Add the EOS provider
            _storeService.AddProvider(new EOSStoreProvider());

            // Initialize the service
            _storeService.Initialize((success) =>
            {
                _isInitialized = success;

                if (success)
                {
                    LogHelper.Info(LOG_TAG, "Store service initialized successfully");
                }
                else
                {
                    LogHelper.Error(LOG_TAG, "Failed to initialize store service");
                }
                
                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Queries the catalog of available items
        /// </summary>
        /// <param name="callback">Callback with list of catalog items</param>
        public static void QueryCatalog(Action<List<CatalogItem>, bool> callback)
        {
            EnsureInitialized();
            _storeService.QueryCatalog(callback);
        }

        /// <summary>
        /// Queries the player's owned items
        /// </summary>
        /// <param name="callback">Callback with list of inventory items</param>
        public static void QueryInventory(Action<List<InventoryItem>, bool> callback)
        {
            EnsureInitialized();
            _storeService.QueryInventory(callback);
        }

        /// <summary>
        /// Queries the available offers for purchasing items
        /// </summary>
        /// <param name="callback">Callback with list of offers</param>
        public static void QueryOffers(Action<List<StoreOffer>, bool> callback)
        {
            EnsureInitialized();
            _storeService.QueryOffers(callback);
        }

        /// <summary>
        /// Initiates a purchase for a specific offer
        /// </summary>
        /// <param name="offerId">ID of the offer to purchase</param>
        /// <param name="callback">Callback with purchase result</param>
        public static void PurchaseOffer(string offerId, Action<PurchaseResult> callback)
        {
            EnsureInitialized();
            _storeService.PurchaseOffer(offerId, callback);
        }

        /// <summary>
        /// Consumes an inventory item (for consumable items)
        /// </summary>
        /// <param name="inventoryItemId">ID of the inventory item to consume</param>
        /// <param name="quantity">Quantity to consume</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public static void ConsumeItem(string inventoryItemId, int quantity, Action<bool> callback)
        {
            EnsureInitialized();
            _storeService.ConsumeItem(inventoryItemId, quantity, callback);
        }

        /// <summary>
        /// Gets detailed information about a specific catalog item
        /// </summary>
        /// <param name="itemId">ID of the item to query</param>
        /// <param name="callback">Callback with catalog item details</param>
        public static void GetCatalogItemDetails(string itemId, Action<CatalogItem, bool> callback)
        {
            EnsureInitialized();
            _storeService.GetCatalogItemDetails(itemId, callback);
        }

        /// <summary>
        /// Gets the available currencies and their balances
        /// </summary>
        /// <param name="callback">Callback with list of currencies</param>
        public static void GetCurrencies(Action<List<Currency>, bool> callback)
        {
            EnsureInitialized();
            _storeService.GetCurrencies(callback);
        }

        /// <summary>
        /// Opens the platform-specific store UI for a specific item (if supported)
        /// </summary>
        /// <param name="itemId">ID of the item to display</param>
        /// <returns>True if the UI was opened successfully</returns>
        public static bool DisplayStoreUI(string itemId = null)
        {
            EnsureInitialized();
            return _storeService.DisplayStoreUI(itemId);
        }

        /// <summary>
        /// Checks if a purchase is in progress
        /// </summary>
        /// <returns>True if a purchase is in progress</returns>
        public static bool IsPurchaseInProgress()
        {
            EnsureInitialized();
            return _storeService.IsPurchaseInProgress();
        }

        /// <summary>
        /// Restores previous purchases (useful for mobile platforms)
        /// </summary>
        /// <param name="callback">Callback indicating success or failure, with restored items</param>
        public static void RestorePurchases(Action<bool, List<InventoryItem>> callback)
        {
            EnsureInitialized();
            _storeService.RestorePurchases(callback);
        }

        /// <summary>
        /// Validates a purchase receipt
        /// </summary>
        /// <param name="receipt">Receipt to validate</param>
        /// <param name="callback">Callback indicating if the receipt is valid</param>
        public static void ValidateReceipt(string receipt, Action<bool> callback)
        {
            EnsureInitialized();
            _storeService.ValidateReceipt(receipt, callback);
        }

        /// <summary>
        /// Registers a callback for when the catalog is queried
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterCatalogQueryCallback(Action<List<CatalogItem>, bool> callback)
        {
            EnsureInitialized();
            _storeService.OnCatalogQueried += callback;
        }

        /// <summary>
        /// Unregisters a callback for when the catalog is queried
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterCatalogQueryCallback(Action<List<CatalogItem>, bool> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnCatalogQueried -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when the inventory is queried
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterInventoryQueryCallback(Action<List<InventoryItem>, bool> callback)
        {
            EnsureInitialized();
            _storeService.OnInventoryQueried += callback;
        }

        /// <summary>
        /// Unregisters a callback for when the inventory is queried
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterInventoryQueryCallback(Action<List<InventoryItem>, bool> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnInventoryQueried -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when offers are queried
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterOffersQueryCallback(Action<List<StoreOffer>, bool> callback)
        {
            EnsureInitialized();
            _storeService.OnOffersQueried += callback;
        }

        /// <summary>
        /// Unregisters a callback for when offers are queried
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterOffersQueryCallback(Action<List<StoreOffer>, bool> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnOffersQueried -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when a purchase succeeds
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterPurchaseSuccessCallback(Action<PurchaseResult> callback)
        {
            EnsureInitialized();
            _storeService.OnPurchaseSuccess += callback;
        }

        /// <summary>
        /// Unregisters a callback for when a purchase succeeds
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterPurchaseSuccessCallback(Action<PurchaseResult> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnPurchaseSuccess -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when a purchase fails
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterPurchaseFailureCallback(Action<PurchaseResult> callback)
        {
            EnsureInitialized();
            _storeService.OnPurchaseFailure += callback;
        }

        /// <summary>
        /// Unregisters a callback for when a purchase fails
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterPurchaseFailureCallback(Action<PurchaseResult> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnPurchaseFailure -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when an item is added to inventory
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterItemAddedCallback(Action<InventoryItem> callback)
        {
            EnsureInitialized();
            _storeService.OnItemAdded += callback;
        }

        /// <summary>
        /// Unregisters a callback for when an item is added to inventory
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterItemAddedCallback(Action<InventoryItem> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnItemAdded -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when an item is consumed
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterItemConsumedCallback(Action<string, int> callback)
        {
            EnsureInitialized();
            _storeService.OnItemConsumed += callback;
        }

        /// <summary>
        /// Unregisters a callback for when an item is consumed
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterItemConsumedCallback(Action<string, int> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnItemConsumed -= callback;
            }
        }

        /// <summary>
        /// Registers a callback for when currency balance changes
        /// </summary>
        /// <param name="callback">Callback to register</param>
        public static void RegisterCurrencyBalanceChangedCallback(Action<string, decimal> callback)
        {
            EnsureInitialized();
            _storeService.OnCurrencyBalanceChanged += callback;
        }

        /// <summary>
        /// Unregisters a callback for when currency balance changes
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterCurrencyBalanceChangedCallback(Action<string, decimal> callback)
        {
            if (_isInitialized)
            {
                _storeService.OnCurrencyBalanceChanged -= callback;
            }
        }

        /// <summary>
        /// Ensures that the store service is initialized
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                LogHelper.Warning(LOG_TAG, "Store service not initialized. Initializing now.");
                Initialize();
            }
        }
    }
} 