using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Ecom;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Store
{
    /// <summary>
    /// Provider for in-game store using Epic Online Services
    /// </summary>
    public class EOSStoreProvider : IStoreProvider
    {
        private const string PROVIDER_NAME = "EOSStore";
        private const string LOG_TAG = "EOSStoreProvider";

        // Cache for catalog items
        private readonly Dictionary<string, CatalogItem> _catalogCache = new Dictionary<string, CatalogItem>();

        // Cache for inventory items
        private readonly Dictionary<string, InventoryItem> _inventoryCache = new Dictionary<string, InventoryItem>();

        // Cache for offers
        private readonly Dictionary<string, StoreOffer> _offersCache = new Dictionary<string, StoreOffer>();

        // EOS ECOM interface
        private EcomInterface _ecomInterface;

        // Initialization status
        private bool _isInitialized;

        // Flag to track if a purchase is in progress
        private bool _purchaseInProgress;

        /// <summary>
        /// Creates a new EOS store provider
        /// </summary>
        public EOSStoreProvider()
        {
            LogHelper.Debug(LOG_TAG, "EOSStoreProvider created");
        }

        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        public string ProviderName => PROVIDER_NAME;

        /// <summary>
        /// Indicates if the provider is available and initialized
        /// </summary>
        public bool IsAvailable => _isInitialized && IsUserLoggedIn();

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Initializes the store provider
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        public void Initialize(Action<bool> callback)
        {
            if (_isInitialized)
            {
                LogHelper.Warning(LOG_TAG, "EOSStoreProvider already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info(LOG_TAG, "Initializing EOSStoreProvider");

            try
            {
                // Check if the EOS manager exists
                if (EOSManager.Instance == null)
                {
                    LastError = "EOS Manager is not available";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Get the ECOM interface
                _ecomInterface = EOSManager.Instance.GetEOSPlatformInterface().GetEcomInterface();
                if (_ecomInterface == null)
                {
                    LastError = "Failed to get EOS ECOM interface";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Check if user is signed in
                if (!IsUserLoggedIn())
                {
                    LastError = "User is not logged in to EOS";
                    LogHelper.Warning(LOG_TAG, LastError);
                    // We still mark as initialized, but will check IsAvailable before operations
                    _isInitialized = true;
                    callback?.Invoke(true);
                    return;
                }

                _isInitialized = true;
                LogHelper.Info(LOG_TAG, "EOSStoreProvider initialized successfully");
                callback?.Invoke(true);
            }
            catch (Exception ex)
            {
                LastError = $"Error initializing EOSStoreProvider: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to initialize EOSStoreProvider");
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Queries the catalog of available items from the provider
        /// </summary>
        /// <param name="callback"> Callback with the list of catalog items </param>
        public void QueryCatalog(Action<List<CatalogItem>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<CatalogItem>(), false);
                return;
            }

            LogHelper.Debug(LOG_TAG, "Querying catalog from EOS");

            try
            {
                var options = new QueryOffersOptions
                {
                    LocalUserId = EOSManager.Instance.GetLocalUserId()
                };

                _ecomInterface.QueryOffers(ref options, null, (ref QueryOffersCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        LogHelper.Info(LOG_TAG, "Successfully queried offers from EOS");

                        // Get the number of offers
                        var countOptions = new GetOfferCountOptions
                        {
                            LocalUserId = EOSManager.Instance.GetLocalUserId()
                        };

                        uint offerCount = _ecomInterface.GetOfferCount(ref countOptions);

                        LogHelper.Debug(LOG_TAG, $"Found {offerCount} offers");

                        var catalogItems = new List<CatalogItem>();

                        // Convert each offer to a catalog item
                        for (uint i = 0; i < offerCount; i++)
                        {
                            var offerOptions = new CopyOfferByIndexOptions
                            {
                                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                                OfferIndex = i
                            };

                            CatalogOffer? offer = null;
                            var result = _ecomInterface.CopyOfferByIndex(ref offerOptions, out offer);

                            if (result == Result.Success && offer.HasValue)
                            {
                                // Convert to our catalog item model
                                var catalogItem = ConvertEOSOfferToCatalogItem(offer.Value);
                                if (catalogItem != null)
                                {
                                    catalogItems.Add(catalogItem);

                                    // Cache the item
                                    _catalogCache[catalogItem.ItemId] = catalogItem;

                                    // Also create and cache a store offer
                                    var storeOffer = ConvertEOSOfferToStoreOffer(offer.Value);
                                    if (storeOffer != null) _offersCache[storeOffer.OfferId] = storeOffer;
                                }
                            }
                            else
                            {
                                LogHelper.Warning(LOG_TAG, $"Failed to copy offer at index {i}: {result}");
                            }
                        }

                        callback?.Invoke(catalogItems, true);
                    }
                    else
                    {
                        LastError = $"Failed to query offers: {callbackInfo.ResultCode}";
                        LogHelper.Error(LOG_TAG, LastError);
                        callback?.Invoke(new List<CatalogItem>(), false);
                    }
                });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying catalog: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query catalog");
                callback?.Invoke(new List<CatalogItem>(), false);
            }
        }

        /// <summary>
        /// Queries the player's owned items (inventory)
        /// </summary>
        /// <param name="callback"> Callback with the list of owned items </param>
        public void QueryInventory(Action<List<InventoryItem>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<InventoryItem>(), false);
                return;
            }

            LogHelper.Debug(LOG_TAG, "Querying inventory from EOS");

            try
            {
                var catalogItemIds = _catalogCache.Keys.ToArray();

                var options = new QueryOwnershipOptions
                {
                    LocalUserId = EOSManager.Instance.GetLocalUserId(),
                    CatalogItemIds = ConvertToUtf8StringArray(catalogItemIds),
                    CatalogNamespace = null // Use default namespace
                };

                _ecomInterface.QueryOwnership(ref options, null, (ref QueryOwnershipCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        LogHelper.Info(LOG_TAG, "Successfully queried ownership from EOS");

                        var inventoryItems = new List<InventoryItem>();

                        // Get ownership results directly from the ItemOwnership member
                        var ownershipData = callbackInfo.ItemOwnership;
                        if (ownershipData != null && ownershipData.Length > 0)
                        {
                            foreach (var ownership in ownershipData)
                            {
                                if (ownership.OwnershipStatus == OwnershipStatus.Owned)
                                {
                                    // Create an inventory item for this owned item
                                    CatalogItem catalogItem = null;
                                    _catalogCache.TryGetValue(ownership.Id, out catalogItem);

                                    var inventoryItem = new InventoryItem
                                    {
                                        InventoryItemId = Guid.NewGuid().ToString(), // Generate a unique ID
                                        CatalogItemId = ownership.Id,
                                        CatalogItem = catalogItem,
                                        Quantity = 1, // EOS doesn't track quantities
                                        AcquisitionDate = DateTime.UtcNow, // EOS doesn't provide acquisition date
                                        ProviderName = PROVIDER_NAME,
                                        ProviderData = ownership
                                    };

                                    inventoryItems.Add(inventoryItem);

                                    // Cache the item
                                    _inventoryCache[inventoryItem.InventoryItemId] = inventoryItem;
                                }
                            }
                        }

                        callback?.Invoke(inventoryItems, true);
                    }
                    else
                    {
                        LastError = $"Failed to query ownership: {callbackInfo.ResultCode}";
                        LogHelper.Error(LOG_TAG, LastError);
                        callback?.Invoke(new List<InventoryItem>(), false);
                    }
                });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying inventory: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query inventory");
                callback?.Invoke(new List<InventoryItem>(), false);
            }
        }

        /// <summary>
        /// Queries the available offers for purchasing items
        /// </summary>
        /// <param name="callback"> Callback with the list of offers </param>
        public void QueryOffers(Action<List<StoreOffer>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<StoreOffer>(), false);
                return;
            }

            LogHelper.Debug(LOG_TAG, "Querying offers from EOS");

            try
            {
                var options = new QueryOffersOptions
                {
                    LocalUserId = EOSManager.Instance.GetLocalUserId()
                };

                _ecomInterface.QueryOffers(ref options, null, (ref QueryOffersCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        LogHelper.Info(LOG_TAG, "Successfully queried offers from EOS");

                        // Get the number of offers
                        var countOptions = new GetOfferCountOptions
                        {
                            LocalUserId = EOSManager.Instance.GetLocalUserId()
                        };

                        uint offerCount = _ecomInterface.GetOfferCount(ref countOptions);

                        LogHelper.Debug(LOG_TAG, $"Found {offerCount} offers");

                        var storeOffers = new List<StoreOffer>();

                        // Convert each offer to a store offer
                        for (uint i = 0; i < offerCount; i++)
                        {
                            var offerOptions = new CopyOfferByIndexOptions
                            {
                                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                                OfferIndex = i
                            };

                            CatalogOffer? offer = null;
                            var result = _ecomInterface.CopyOfferByIndex(ref offerOptions, out offer);

                            if (result == Result.Success)
                            {
                                // Convert to our store offer model
                                var storeOffer = ConvertEOSOfferToStoreOffer(offer.Value);
                                if (storeOffer != null)
                                {
                                    storeOffers.Add(storeOffer);

                                    // Cache the offer
                                    _offersCache[storeOffer.OfferId] = storeOffer;
                                }
                            }
                            else
                            {
                                LogHelper.Warning(LOG_TAG, $"Failed to copy offer at index {i}: {result}");
                            }
                        }

                        callback?.Invoke(storeOffers, true);
                    }
                    else
                    {
                        LastError = $"Failed to query offers: {callbackInfo.ResultCode}";
                        LogHelper.Error(LOG_TAG, LastError);
                        callback?.Invoke(new List<StoreOffer>(), false);
                    }
                });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying offers: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query offers");
                callback?.Invoke(new List<StoreOffer>(), false);
            }
        }

        /// <summary>
        /// Initiates a purchase for a specific offer
        /// </summary>
        /// <param name="offerId"> ID of the offer to purchase </param>
        /// <param name="callback"> Callback with the purchase result </param>
        public void PurchaseOffer(string offerId, Action<PurchaseResult> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(PurchaseResult.CreateFailedResult(offerId, "Provider not available", PROVIDER_NAME));
                return;
            }

            if (string.IsNullOrEmpty(offerId))
            {
                LastError = "Offer ID cannot be empty";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME));
                return;
            }

            if (_purchaseInProgress)
            {
                LastError = "A purchase is already in progress";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME));
                return;
            }

            StoreOffer offer = null;
            if (!_offersCache.TryGetValue(offerId, out offer))
            {
                LastError = $"Offer {offerId} not found in cache";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME));
                return;
            }

            LogHelper.Info(LOG_TAG, $"Initiating purchase for offer {offerId}");

            try
            {
                _purchaseInProgress = true;

                // First, check if we already own this offer
                var queryOptions = new QueryOwnershipOptions
                {
                    LocalUserId = EOSManager.Instance.GetLocalUserId(),
                    CatalogItemIds = ConvertToUtf8StringArray(offer.Items.Select(i => i.ItemId).ToArray()),
                    CatalogNamespace = null // Use default namespace
                };

                _ecomInterface.QueryOwnership(ref queryOptions, null,
                    (ref QueryOwnershipCallbackInfo queryCallbackInfo) =>
                    {
                        if (queryCallbackInfo.ResultCode == Result.Success)
                        {
                            // Check if we already own all items in this offer
                            var ownershipData = queryCallbackInfo.ItemOwnership;
                            bool allOwned = false;

                            if (ownershipData != null && ownershipData.Length > 0)
                            {
                                allOwned = ownershipData.All(item => item.OwnershipStatus == OwnershipStatus.Owned);
                            }

                            if (allOwned)
                            {
                                _purchaseInProgress = false;
                                LastError = "All items in this offer are already owned";
                                LogHelper.Warning(LOG_TAG, LastError);

                                var ownedResult = PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME);
                                ownedResult.Offer = offer;
                                callback?.Invoke(ownedResult);
                                return;
                            }

                            // Proceed with purchase
                            var checkoutOptions = new CheckoutOptions
                            {
                                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                                OverrideCatalogNamespace = null, // Use default namespace
                                Entries = new[]
                                {
                                    new CheckoutEntry
                                    {
                                        OfferId = offerId
                                    }
                                }
                            };

                            _ecomInterface.Checkout(ref checkoutOptions, null,
                                (ref CheckoutCallbackInfo checkoutCallbackInfo) =>
                                {
                                    _purchaseInProgress = false;

                                    if (checkoutCallbackInfo.ResultCode == Result.Success)
                                    {
                                        LogHelper.Info(LOG_TAG, $"Checkout successful for offer {offerId}");

                                        // Create success result
                                        var result = PurchaseResult.CreateSuccessResult(offerId,
                                            checkoutCallbackInfo.TransactionId, PROVIDER_NAME);
                                        result.Offer = offer;

                                        // Create inventory items for purchased items
                                        foreach (var offerItem in offer.Items)
                                        {
                                            CatalogItem catalogItem = null;
                                            _catalogCache.TryGetValue(offerItem.ItemId, out catalogItem);

                                            var inventoryItem = new InventoryItem
                                            {
                                                InventoryItemId = Guid.NewGuid().ToString(), // Generate a unique ID
                                                CatalogItemId = offerItem.ItemId,
                                                CatalogItem = catalogItem,
                                                Quantity = offerItem.Quantity,
                                                AcquisitionDate = DateTime.UtcNow,
                                                TransactionId = checkoutCallbackInfo.TransactionId,
                                                ProviderName = PROVIDER_NAME
                                            };

                                            result.GrantedItems.Add(inventoryItem);

                                            // Cache the item
                                            _inventoryCache[inventoryItem.InventoryItemId] = inventoryItem;
                                        }

                                        callback?.Invoke(result);
                                    }
                                    else
                                    {
                                        LastError = $"Checkout failed: {checkoutCallbackInfo.ResultCode}";
                                        LogHelper.Error(LOG_TAG, LastError);

                                        var failedResult =
                                            PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME);
                                        failedResult.Offer = offer;
                                        callback?.Invoke(failedResult);
                                    }
                                });
                        }
                        else
                        {
                            _purchaseInProgress = false;
                            LastError = $"Failed to check ownership: {queryCallbackInfo.ResultCode}";
                            LogHelper.Error(LOG_TAG, LastError);

                            var failedResult = PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME);
                            failedResult.Offer = offer;
                            callback?.Invoke(failedResult);
                        }
                    });
            }
            catch (Exception ex)
            {
                _purchaseInProgress = false;
                LastError = $"Error during purchase: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to purchase offer");

                var failedResult = PurchaseResult.CreateFailedResult(offerId, LastError, PROVIDER_NAME);
                failedResult.Offer = offer;
                callback?.Invoke(failedResult);
            }
        }

        /// <summary>
        /// Consumes an inventory item (for consumable items)
        /// </summary>
        /// <param name="inventoryItemId"> ID of the inventory item to consume </param>
        /// <param name="quantity"> Quantity to consume </param>
        /// <param name="callback"> Callback indicating success or failure </param>
        public void ConsumeItem(string inventoryItemId, int quantity, Action<bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(false);
                return;
            }

            // EOS doesn't have a direct consume API, so we'll just update our local cache
            LogHelper.Info(LOG_TAG, $"Consuming item {inventoryItemId}, quantity {quantity}");

            try
            {
                // Check if the item exists in our cache
                if (!_inventoryCache.TryGetValue(inventoryItemId, out var item))
                {
                    LastError = $"Item {inventoryItemId} not found in inventory";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Check if there's enough quantity to consume
                if (item.Quantity < quantity)
                {
                    LastError = $"Not enough quantity to consume. Have {item.Quantity}, need {quantity}";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Update the quantity
                item.Quantity -= quantity;

                // Remove the item if quantity is 0
                if (item.Quantity <= 0) _inventoryCache.Remove(inventoryItemId);

                LogHelper.Info(LOG_TAG, $"Successfully consumed {quantity} of item {inventoryItemId}");
                callback?.Invoke(true);
            }
            catch (Exception ex)
            {
                LastError = $"Error consuming item: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to consume item");
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Validates a purchase receipt
        /// </summary>
        /// <param name="receipt"> Receipt to validate </param>
        /// <param name="callback"> Callback indicating if the receipt is valid </param>
        public void ValidateReceipt(string receipt, Action<bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(false);
                return;
            }

            // EOS doesn't have a simple receipt validation API that we can use here
            // In a real implementation, we would validate this on a server
            LogHelper.Warning(LOG_TAG, "Receipt validation not implemented in EOS provider");
            callback?.Invoke(true); // Assume valid for now
        }

        /// <summary>
        /// Gets detailed information about a specific catalog item
        /// </summary>
        /// <param name="itemId"> ID of the item to query </param>
        /// <param name="callback"> Callback with the catalog item details </param>
        public void GetCatalogItemDetails(string itemId, Action<CatalogItem, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(null, false);
                return;
            }

            // Check if the item is in our cache
            if (_catalogCache.TryGetValue(itemId, out var cachedItem))
            {
                callback?.Invoke(cachedItem, true);
                return;
            }

            // EOS doesn't have a direct API to get a single catalog item
            // We need to query all offers and find the one with the matching item
            LogHelper.Debug(LOG_TAG, $"Getting catalog item details for {itemId}");

            QueryCatalog((items, success) =>
            {
                if (success)
                {
                    var item = items.FirstOrDefault(i => i.ItemId == itemId);
                    if (item != null)
                    {
                        callback?.Invoke(item, true);
                    }
                    else
                    {
                        LastError = $"Item {itemId} not found in catalog";
                        LogHelper.Error(LOG_TAG, LastError);
                        callback?.Invoke(null, false);
                    }
                }
                else
                {
                    callback?.Invoke(null, false);
                }
            });
        }

        /// <summary>
        /// Gets the available currencies and their balances
        /// </summary>
        /// <param name="callback"> Callback with the list of currencies </param>
        public void GetCurrencies(Action<List<Currency>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<Currency>(), false);
                return;
            }

            // EOS doesn't have a direct currency API
            // For now, we'll just return an empty list
            LogHelper.Warning(LOG_TAG, "Currency management not implemented in EOS provider");
            callback?.Invoke(new List<Currency>(), true);
        }

        /// <summary>
        /// Opens the platform-specific store UI for a specific item (if supported)
        /// </summary>
        /// <param name="itemId"> ID of the item to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        public bool DisplayStoreUI(string itemId = null)
        {
            if (!CheckAvailability()) return false;

            // EOS doesn't have a built-in store UI that we can open
            LogHelper.Warning(LOG_TAG, "Store UI not implemented in EOS provider");
            LastError = "Store UI not supported by EOS";
            return false;
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
        /// <param name="callback"> Callback indicating success or failure, with restored items </param>
        public void RestorePurchases(Action<bool, List<InventoryItem>> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(false, new List<InventoryItem>());
                return;
            }

            // For EOS, restoring purchases is the same as querying ownership
            LogHelper.Info(LOG_TAG, "Restoring purchases through EOS ownership query");

            QueryInventory((items, success) => { callback?.Invoke(success, items); });
        }

        /// <summary>
        /// Converts an EOS catalog offer to a catalog item
        /// </summary>
        /// <param name="offer"> EOS catalog offer </param>
        /// <returns> Converted catalog item </returns>
        private CatalogItem ConvertEOSOfferToCatalogItem(CatalogOffer catalogOffer)
        {
            // For structs, check if it's the default value instead of null
            if (catalogOffer.Equals(default(CatalogOffer)))
                return null;

            // For simplicity, we'll create a catalog item with the same ID as the offer
            var catalogItem = new CatalogItem
            {
                ItemId = catalogOffer.Id,
                DisplayName = catalogOffer.TitleText,
                Description = catalogOffer.DescriptionText,
                // Use the description for long description if there's no specific field
                LongDescription = string.IsNullOrEmpty(catalogOffer.DescriptionText) ? catalogOffer.DescriptionText : null,
                ItemType = GetItemTypeFromOffer(catalogOffer),
                ProviderName = PROVIDER_NAME,
                ProviderData = catalogOffer,
                IsAvailable = true
            };

            // Note: In this EOS SDK version, we might not have direct access to bundle items
            // or categories. If your CatalogItem model has properties for these,
            // you'll need to handle them differently or remove them.

            return catalogItem;
        }

        /// <summary>
        /// Converts an EOS catalog offer to a store offer
        /// </summary>
        /// <param name="offer"> EOS catalog offer </param>
        /// <returns> Converted store offer </returns>
        private StoreOffer ConvertEOSOfferToStoreOffer(CatalogOffer catalogOffer)
        {
            if (catalogOffer.Equals(default(CatalogOffer)))
                return null;

            // Create a store offer from the catalog offer
            var offer = new StoreOffer
            {
                OfferId = catalogOffer.Id,
                DisplayName = catalogOffer.TitleText,
                Description = catalogOffer.DescriptionText,
                // Use the description for long description if there's no specific field
                LongDescription = string.IsNullOrEmpty(catalogOffer.DescriptionText) ? catalogOffer.DescriptionText : null,
                // Set price related properties if your StoreOffer model has them
                CurrencyCode = catalogOffer.CurrencyCode,
                ProviderName = PROVIDER_NAME,
                ProviderData = catalogOffer
            };

            // Add a single item to the offer based on the offer ID itself
            // If your StoreOffer.Items is a List<OfferItem>, adjust accordingly
            var offerItem = new OfferItem
            {
                ItemId = catalogOffer.Id,
                Quantity = 1
            };

            offer.Items.Add(offerItem);

            return offer;
        }

        /// <summary>
        /// Gets the item type from an EOS catalog offer
        /// </summary>
        /// <param name="offer"> EOS catalog offer </param>
        /// <returns> Item type </returns>
        private ItemType GetItemTypeFromOffer(CatalogOffer offer)
        {
            // In this EOS SDK version, we might not have a direct way to determine item type
            // You could use properties like Categories or check specific identifiers in the offer

            // For now, return a default type
            return ItemType.Consumable;
        }

        /// <summary>
        /// Checks if a user is currently logged in to EOS
        /// </summary>
        /// <returns> True if a user is logged in </returns>
        private bool IsUserLoggedIn()
        {
            return EOSManager.Instance != null &&
                   EOSManager.Instance.GetProductUserId() != null &&
                   EOSManager.Instance.GetProductUserId().IsValid();
        }

        /// <summary>
        /// Helper method to convert string[] to Utf8String[]
        /// </summary>
        /// <param name="strings">String array to convert</param>
        /// <returns>Utf8String array</returns>
        private Utf8String[] ConvertToUtf8StringArray(string[] strings)
        {
            if (strings == null)
                return null;

            Utf8String[] utf8Strings = new Utf8String[strings.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                utf8Strings[i] = new Utf8String(strings[i]);
            }
            return utf8Strings;
        }

        /// <summary>
        /// Checks if the provider is available for use
        /// </summary>
        /// <returns> True if the provider is available </returns>
        private bool CheckAvailability()
        {
            if (!_isInitialized)
            {
                LastError = "EOSStoreProvider is not initialized";
                LogHelper.Error(LOG_TAG, LastError);
                return false;
            }

            if (!IsUserLoggedIn())
            {
                LastError = "User is not logged in to EOS";
                LogHelper.Warning(LOG_TAG, LastError);
                return false;
            }

            if (_ecomInterface == null)
            {
                LastError = "EOS ECOM interface is not available";
                LogHelper.Error(LOG_TAG, LastError);
                return false;
            }

            return true;
        }
    }
}