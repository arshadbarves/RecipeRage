using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Store
{
    /// <summary>
    /// Main service for managing store providers and providing a unified API for working with the store
    /// </summary>
    public class StoreService : IStoreService
    {

        // Cache for catalog items
        private readonly Dictionary<string, CatalogItem> _catalogCache = new Dictionary<string, CatalogItem>();

        // Cache for currencies
        private readonly Dictionary<string, Currency> _currenciesCache = new Dictionary<string, Currency>();

        // Cache for inventory items
        private readonly Dictionary<string, InventoryItem> _inventoryCache = new Dictionary<string, InventoryItem>();

        // Object for thread safety
        private readonly object _lock = new object();

        // Cache for offers
        private readonly Dictionary<string, StoreOffer> _offersCache = new Dictionary<string, StoreOffer>();

        // List of registered providers
        private readonly List<IStoreProvider> _providers = new List<IStoreProvider>();

        // Flag to track if a purchase is in progress
        private bool _purchaseInProgress;

        /// <summary>
        /// Creates a new store service
        /// </summary>
        public StoreService()
        {
            LogHelper.Debug("StoreService", "StoreService created");
        }

        /// <summary>
        /// Event triggered when the catalog is queried
        /// </summary>
        public event Action<List<CatalogItem>> OnCatalogQueried;

        /// <summary>
        /// Event triggered when inventory is queried
        /// </summary>
        public event Action<List<InventoryItem>> OnInventoryQueried;

        /// <summary>
        /// Event triggered when offers are queried
        /// </summary>
        public event Action<List<StoreOffer>> OnOffersQueried;

        /// <summary>
        /// Event triggered when a purchase succeeds
        /// </summary>
        public event Action<PurchaseResult> OnPurchaseSuccess;

        /// <summary>
        /// Event triggered when a purchase fails
        /// </summary>
        public event Action<string, string> OnPurchaseFailed;

        /// <summary>
        /// Event triggered when an item is added to inventory
        /// </summary>
        public event Action<InventoryItem> OnItemAdded;

        /// <summary>
        /// Event triggered when an item is consumed
        /// </summary>
        public event Action<string, int> OnItemConsumed;

        /// <summary>
        /// Event triggered when currency balances change
        /// </summary>
        public event Action<List<Currency>> OnCurrencyBalanceChanged;

        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Initializes the store service and all available providers
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        public void Initialize(Action<bool> callback)
        {
            if (IsInitialized)
            {
                LogHelper.Warning("StoreService", "StoreService already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info("StoreService", "Initializing StoreService");

            if (_providers.Count == 0)
            {
                LogHelper.Warning("StoreService", "No store providers registered");
                LastError = "No store providers registered";
                callback?.Invoke(false);
                return;
            }

            int providersToInitialize = _providers.Count;
            int providersInitialized = 0;
            bool anySuccess = false;

            foreach (var provider in _providers)
            {
                provider.Initialize(success =>
                {
                    lock (_lock)
                    {
                        providersInitialized++;
                        if (success)
                        {
                            anySuccess = true;
                            LogHelper.Info("StoreService", $"Provider {provider.ProviderName} initialized successfully");
                        }
                        else
                        {
                            LogHelper.Warning("StoreService", $"Provider {provider.ProviderName} failed to initialize: {provider.LastError}");
                        }

                        if (providersInitialized >= providersToInitialize)
                        {
                            if (anySuccess)
                            {
                                IsInitialized = true;
                                LogHelper.Info("StoreService", "StoreService initialized successfully");
                                callback?.Invoke(true);
                            }
                            else
                            {
                                LastError = "All providers failed to initialize";
                                LogHelper.Error("StoreService", LastError);
                                callback?.Invoke(false);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Adds a store provider to the service
        /// </summary>
        /// <param name="provider"> Provider to add </param>
        /// <returns> True if the provider was added successfully </returns>
        public bool AddProvider(IStoreProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("StoreService", "Cannot add null provider");
                return false;
            }

            lock (_lock)
            {
                // Check if a provider with the same name already exists
                if (_providers.Any(p => p.ProviderName == provider.ProviderName))
                {
                    LogHelper.Warning("StoreService", $"Provider {provider.ProviderName} is already registered");
                    return false;
                }

                _providers.Add(provider);
                LogHelper.Info("StoreService", $"Provider {provider.ProviderName} registered");
                return true;
            }
        }

        /// <summary>
        /// Gets a store provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider to get </param>
        /// <returns> The provider instance, or null if not found </returns>
        public IStoreProvider GetProvider(string providerName)
        {
            lock (_lock)
            {
                return _providers.FirstOrDefault(p => p.ProviderName == providerName);
            }
        }

        /// <summary>
        /// Gets the catalog of available items
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of catalog items </param>
        public void GetCatalog(bool forceRefresh, Action<List<CatalogItem>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<CatalogItem>());
                return;
            }

            // Return from cache if we have items and aren't forcing a refresh
            if (!forceRefresh && _catalogCache.Count > 0)
            {
                LogHelper.Debug("StoreService", $"Returning catalog from cache ({_catalogCache.Count} items)");
                callback?.Invoke(_catalogCache.Values.ToList());
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for getting catalog";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<CatalogItem>());
                return;
            }

            LogHelper.Info("StoreService", $"Querying catalog from provider {provider.ProviderName}");

            provider.QueryCatalog((items, success) =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Clear existing cache if refreshing
                        if (forceRefresh)
                            _catalogCache.Clear();

                        // Add all items to cache
                        foreach (var item in items)
                        {
                            _catalogCache[item.ItemId] = item;
                        }
                    }

                    LogHelper.Debug("StoreService", $"Catalog query successful, got {items.Count} items");
                    OnCatalogQueried?.Invoke(items);
                    callback?.Invoke(items);
                }
                else
                {
                    LastError = $"Failed to query catalog from provider {provider.ProviderName}";
                    LogHelper.Error("StoreService", LastError);
                    callback?.Invoke(new List<CatalogItem>());
                }
            });
        }

        /// <summary>
        /// Queries the catalog of available items directly from the provider
        /// </summary>
        /// <param name="callback">Callback with the list of catalog items and success flag</param>
        public void QueryCatalog(Action<List<CatalogItem>, bool> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<CatalogItem>(), false);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for querying catalog";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<CatalogItem>(), false);
                return;
            }

            LogHelper.Info("StoreService", $"Querying catalog directly from provider {provider.ProviderName}");

            // Forward the call directly to the provider
            provider.QueryCatalog((items, success) =>
            {
                if (success)
                {
                    // Update cache with new items
                    lock (_lock)
                    {
                        foreach (var item in items)
                        {
                            _catalogCache[item.ItemId] = item;
                        }
                    }

                    LogHelper.Debug("StoreService", $"Catalog query successful, got {items.Count} items");
                    OnCatalogQueried?.Invoke(items);
                }
                else
                {
                    LastError = $"Failed to query catalog from provider {provider.ProviderName}";
                    LogHelper.Error("StoreService", LastError);
                }

                // Forward the result to the callback
                callback?.Invoke(items, success);
            });
        }

        /// <summary>
        /// Gets the player's owned items (inventory)
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of owned items </param>
        public void GetInventory(bool forceRefresh, Action<List<InventoryItem>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<InventoryItem>());
                return;
            }

            // If we have cached inventory items and don't need to refresh, return them
            if (!forceRefresh && _inventoryCache.Count > 0)
            {
                var inventoryItems = _inventoryCache.Values.ToList();
                callback?.Invoke(inventoryItems);
                OnInventoryQueried?.Invoke(inventoryItems);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for getting inventory";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<InventoryItem>());
                return;
            }

            LogHelper.Info("StoreService", $"Querying inventory from provider {provider.ProviderName}");

            provider.QueryInventory((items, success) =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Clear existing cache if refreshing
                        if (forceRefresh)
                            _inventoryCache.Clear();

                        // Add all items to cache
                        foreach (var item in items)
                        {
                            _inventoryCache[item.InventoryItemId] = item;
                        }
                    }

                    LogHelper.Info("StoreService", $"Retrieved {items.Count} inventory items");
                    OnInventoryQueried?.Invoke(items);
                    callback?.Invoke(items);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("StoreService", $"Failed to query inventory: {LastError}");
                    callback?.Invoke(new List<InventoryItem>());
                }
            });
        }

        /// <summary>
        /// Queries the player's owned items directly from the provider
        /// </summary>
        /// <param name="callback">Callback with the list of inventory items and success flag</param>
        public void QueryInventory(Action<List<InventoryItem>, bool> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<InventoryItem>(), false);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for querying inventory";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<InventoryItem>(), false);
                return;
            }

            LogHelper.Info("StoreService", $"Querying inventory directly from provider {provider.ProviderName}");

            // Forward the call directly to the provider
            provider.QueryInventory((items, success) =>
            {
                if (success)
                {
                    // Update cache with new items
                    lock (_lock)
                    {
                        foreach (var item in items)
                        {
                            _inventoryCache[item.InventoryItemId] = item;
                        }
                    }

                    LogHelper.Debug("StoreService", $"Inventory query successful, got {items.Count} items");
                    OnInventoryQueried?.Invoke(items);
                }
                else
                {
                    LastError = $"Failed to query inventory from provider {provider.ProviderName}";
                    LogHelper.Error("StoreService", LastError);
                }

                // Forward the result to the callback
                callback?.Invoke(items, success);
            });
        }

        /// <summary>
        /// Gets the available offers for purchasing items
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of offers </param>
        public void GetOffers(bool forceRefresh, Action<List<StoreOffer>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<StoreOffer>());
                return;
            }

            // If we have cached offers and don't need to refresh, return them
            if (!forceRefresh && _offersCache.Count > 0)
            {
                var offers = _offersCache.Values.ToList();
                callback?.Invoke(offers);
                OnOffersQueried?.Invoke(offers);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for getting offers";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<StoreOffer>());
                return;
            }

            LogHelper.Info("StoreService", $"Querying offers from provider {provider.ProviderName}");

            provider.QueryOffers((offers, success) =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Clear existing cache if refreshing
                        if (forceRefresh)
                            _offersCache.Clear();

                        // Add all offers to cache
                        foreach (var offer in offers)
                        {
                            _offersCache[offer.OfferId] = offer;
                        }
                    }

                    LogHelper.Info("StoreService", $"Retrieved {offers.Count} offers");
                    OnOffersQueried?.Invoke(offers);
                    callback?.Invoke(offers);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("StoreService", $"Failed to query offers: {LastError}");
                    callback?.Invoke(new List<StoreOffer>());
                }
            });
        }

        /// <summary>
        /// Queries the available offers for purchasing items directly from the provider
        /// </summary>
        /// <param name="callback">Callback with the list of offers and success flag</param>
        public void QueryOffers(Action<List<StoreOffer>, bool> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<StoreOffer>(), false);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for querying offers";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<StoreOffer>(), false);
                return;
            }

            LogHelper.Info("StoreService", $"Querying offers directly from provider {provider.ProviderName}");

            // Forward the call directly to the provider
            provider.QueryOffers((offers, success) =>
            {
                if (success)
                {
                    // Update cache with new offers
                    lock (_lock)
                    {
                        foreach (var offer in offers)
                        {
                            _offersCache[offer.OfferId] = offer;
                        }
                    }

                    LogHelper.Debug("StoreService", $"Offers query successful, got {offers.Count} offers");
                    OnOffersQueried?.Invoke(offers);
                }
                else
                {
                    LastError = $"Failed to query offers from provider {provider.ProviderName}";
                    LogHelper.Error("StoreService", LastError);
                }

                // Forward the result to the callback
                callback?.Invoke(offers, success);
            });
        }

        /// <summary>
        /// Initiates a purchase for a specific offer
        /// </summary>
        /// <param name="offerId"> ID of the offer to purchase </param>
        /// <param name="callback"> Callback with the purchase result </param>
        public void PurchaseOffer(string offerId, Action<PurchaseResult> callback)
        {
            if (!CheckInitialized())
            {
                var result = PurchaseResult.CreateFailedResult(offerId, "StoreService is not initialized", "");
                callback?.Invoke(result);
                return;
            }

            if (string.IsNullOrEmpty(offerId))
            {
                LastError = "Offer ID cannot be empty";
                LogHelper.Error("StoreService", LastError);
                var result = PurchaseResult.CreateFailedResult(offerId, LastError, "");
                callback?.Invoke(result);
                return;
            }

            if (_purchaseInProgress)
            {
                LastError = "A purchase is already in progress";
                LogHelper.Error("StoreService", LastError);
                var result = PurchaseResult.CreateFailedResult(offerId, LastError, "");
                callback?.Invoke(result);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for making purchases";
                LogHelper.Error("StoreService", LastError);
                var result = PurchaseResult.CreateFailedResult(offerId, LastError, "");
                callback?.Invoke(result);
                return;
            }

            LogHelper.Info("StoreService", $"Initiating purchase for offer {offerId} with provider {provider.ProviderName}");

            _purchaseInProgress = true;

            provider.PurchaseOffer(offerId, result =>
            {
                _purchaseInProgress = false;

                if (result.IsSuccess)
                {
                    LogHelper.Info("StoreService", $"Purchase successful for offer {offerId}");

                    // Update inventory with granted items
                    lock (_lock)
                    {
                        foreach (var item in result.GrantedItems)
                        {
                            _inventoryCache[item.InventoryItemId] = item;
                            OnItemAdded?.Invoke(item);
                        }
                    }

                    OnPurchaseSuccess?.Invoke(result);
                }
                else
                {
                    LastError = result.ErrorMessage;
                    LogHelper.Error("StoreService", $"Purchase failed for offer {offerId}: {LastError}");
                    OnPurchaseFailed?.Invoke(offerId, LastError);
                }

                callback?.Invoke(result);
            });
        }

        /// <summary>
        /// Consumes an inventory item (for consumable items)
        /// </summary>
        /// <param name="inventoryItemId"> ID of the inventory item to consume </param>
        /// <param name="quantity"> Quantity to consume </param>
        /// <param name="callback"> Callback indicating success or failure </param>
        public void ConsumeItem(string inventoryItemId, int quantity, Action<bool> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(inventoryItemId))
            {
                LastError = "Inventory item ID cannot be empty";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(false);
                return;
            }

            if (quantity <= 0)
            {
                LastError = "Quantity must be greater than zero";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(false);
                return;
            }

            // Check if the item exists in our cache
            InventoryItem item = null;
            lock (_lock)
            {
                if (!_inventoryCache.TryGetValue(inventoryItemId, out item))
                {
                    LastError = $"Item {inventoryItemId} not found in inventory";
                    LogHelper.Error("StoreService", LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Check if there's enough quantity to consume
                if (item.Quantity < quantity)
                {
                    LastError = $"Not enough quantity to consume. Have {item.Quantity}, need {quantity}";
                    LogHelper.Error("StoreService", LastError);
                    callback?.Invoke(false);
                    return;
                }
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for consuming items";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(false);
                return;
            }

            LogHelper.Info("StoreService", $"Consuming {quantity} of item {inventoryItemId} with provider {provider.ProviderName}");

            provider.ConsumeItem(inventoryItemId, quantity, success =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Update the cache
                        if (_inventoryCache.TryGetValue(inventoryItemId, out item))
                        {
                            item.Quantity -= quantity;
                            if (item.Quantity <= 0)
                            {
                                _inventoryCache.Remove(inventoryItemId);
                            }
                        }
                    }

                    LogHelper.Info("StoreService", $"Successfully consumed {quantity} of item {inventoryItemId}");
                    OnItemConsumed?.Invoke(inventoryItemId, quantity);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("StoreService", $"Failed to consume item {inventoryItemId}: {LastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Gets detailed information about a specific catalog item
        /// </summary>
        /// <param name="itemId"> ID of the item to query </param>
        /// <param name="callback"> Callback with the catalog item details </param>
        public void GetCatalogItemDetails(string itemId, Action<CatalogItem> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(itemId))
            {
                LastError = "Item ID cannot be empty";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(null);
                return;
            }

            // Check if the item is in our cache
            lock (_lock)
            {
                if (_catalogCache.TryGetValue(itemId, out var cachedItem))
                {
                    callback?.Invoke(cachedItem);
                    return;
                }
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for getting item details";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(null);
                return;
            }

            LogHelper.Info("StoreService", $"Getting details for item {itemId} from provider {provider.ProviderName}");

            provider.GetCatalogItemDetails(itemId, (item, success) =>
            {
                if (success && item != null)
                {
                    lock (_lock)
                    {
                        // Add to cache
                        _catalogCache[item.ItemId] = item;
                    }

                    LogHelper.Info("StoreService", $"Retrieved details for item {itemId}");
                    callback?.Invoke(item);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("StoreService", $"Failed to get details for item {itemId}: {LastError}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Gets the available currencies and their balances
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of currencies </param>
        public void GetCurrencies(bool forceRefresh, Action<List<Currency>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<Currency>());
                return;
            }

            // If we have cached currencies and don't need to refresh, return them
            if (!forceRefresh && _currenciesCache.Count > 0)
            {
                var currencies = _currenciesCache.Values.ToList();
                callback?.Invoke(currencies);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for getting currencies";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(new List<Currency>());
                return;
            }

            LogHelper.Info("StoreService", $"Querying currencies from provider {provider.ProviderName}");

            provider.GetCurrencies((currencies, success) =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Clear existing cache if refreshing
                        if (forceRefresh)
                            _currenciesCache.Clear();

                        // Add all currencies to cache
                        foreach (var currency in currencies)
                        {
                            _currenciesCache[currency.CurrencyCode] = currency;
                        }
                    }

                    LogHelper.Info("StoreService", $"Retrieved {currencies.Count} currencies");
                    OnCurrencyBalanceChanged?.Invoke(currencies);
                    callback?.Invoke(currencies);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("StoreService", $"Failed to query currencies: {LastError}");
                    callback?.Invoke(new List<Currency>());
                }
            });
        }

        /// <summary>
        /// Gets the player's owned items of a specific type
        /// </summary>
        /// <param name="itemType"> Type of items to get </param>
        /// <param name="callback"> Callback with the list of owned items </param>
        public void GetInventoryByType(ItemType itemType, Action<List<InventoryItem>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<InventoryItem>());
                return;
            }

            // First make sure we have the inventory
            GetInventory(false, inventory =>
            {
                // Filter by type
                var filteredItems = inventory
                    .Where(item => item.ItemType == itemType)
                    .ToList();

                callback?.Invoke(filteredItems);
            });
        }

        /// <summary>
        /// Checks if the player owns a specific item
        /// </summary>
        /// <param name="itemId"> ID of the item to check </param>
        /// <param name="callback"> Callback with the result </param>
        public void OwnsItem(string itemId, Action<bool> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(itemId))
            {
                LastError = "Item ID cannot be empty";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(false);
                return;
            }

            // First make sure we have the inventory
            GetInventory(false, inventory =>
            {
                bool ownsItem = inventory.Any(item => item.CatalogItemId == itemId && item.IsAvailable);
                callback?.Invoke(ownsItem);
            });
        }

        /// <summary>
        /// Gets the quantity of a specific item in the player's inventory
        /// </summary>
        /// <param name="itemId"> ID of the item to check </param>
        /// <param name="callback"> Callback with the quantity </param>
        public void GetItemQuantity(string itemId, Action<int> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(0);
                return;
            }

            if (string.IsNullOrEmpty(itemId))
            {
                LastError = "Item ID cannot be empty";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(0);
                return;
            }

            // First make sure we have the inventory
            GetInventory(false, inventory =>
            {
                int quantity = inventory
                    .Where(item => item.CatalogItemId == itemId && item.IsAvailable)
                    .Sum(item => item.Quantity);

                callback?.Invoke(quantity);
            });
        }

        /// <summary>
        /// Gets the balance of a specific currency
        /// </summary>
        /// <param name="currencyCode"> Currency code to check </param>
        /// <param name="callback"> Callback with the balance </param>
        public void GetCurrencyBalance(string currencyCode, Action<decimal> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(0);
                return;
            }

            if (string.IsNullOrEmpty(currencyCode))
            {
                LastError = "Currency code cannot be empty";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(0);
                return;
            }

            // First make sure we have the currencies
            GetCurrencies(false, currencies =>
            {
                var currency = currencies.FirstOrDefault(c => c.CurrencyCode == currencyCode);
                decimal balance = currency?.Balance ?? 0;
                callback?.Invoke(balance);
            });
        }

        /// <summary>
        /// Opens the platform-specific store UI for a specific item (if supported)
        /// </summary>
        /// <param name="itemId"> ID of the item to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        public bool DisplayStoreUI(string itemId = null)
        {
            if (!CheckInitialized())
            {
                return false;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for displaying store UI";
                LogHelper.Error("StoreService", LastError);
                return false;
            }

            LogHelper.Info("StoreService", $"Displaying store UI with provider {provider.ProviderName}");
            bool success = provider.DisplayStoreUI(itemId);

            if (!success)
            {
                LastError = provider.LastError;
                LogHelper.Error("StoreService", $"Failed to display store UI: {LastError}");
            }

            return success;
        }

        /// <summary>
        /// Checks if a purchase is in progress
        /// </summary>
        /// <returns> True if a purchase is in progress </returns>
        public bool IsPurchaseInProgress()
        {
            return _purchaseInProgress;
        }

        /// <summary>
        /// Restores previous purchases (useful for mobile platforms)
        /// </summary>
        /// <param name="callback"> Callback indicating success or failure </param>
        public void RestorePurchases(Action<bool> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            // Get the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable);
            if (provider == null)
            {
                LastError = "No available provider for restoring purchases";
                LogHelper.Error("StoreService", LastError);
                callback?.Invoke(false);
                return;
            }

            LogHelper.Info("StoreService", $"Restoring purchases with provider {provider.ProviderName}");

            provider.RestorePurchases((success, restoredItems) =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Add restored items to inventory cache
                        foreach (var item in restoredItems)
                        {
                            _inventoryCache[item.InventoryItemId] = item;
                            OnItemAdded?.Invoke(item);
                        }
                    }

                    LogHelper.Info("StoreService", $"Successfully restored {restoredItems.Count} items");
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("StoreService", $"Failed to restore purchases: {LastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Checks if the service is initialized
        /// </summary>
        /// <returns> True if initialized, false otherwise </returns>
        private bool CheckInitialized()
        {
            if (!IsInitialized)
            {
                LastError = "StoreService is not initialized";
                LogHelper.Error("StoreService", LastError);
                return false;
            }
            return true;
        }
    }
}