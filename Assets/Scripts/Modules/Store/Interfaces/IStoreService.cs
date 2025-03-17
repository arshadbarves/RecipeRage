using System;
using System.Collections.Generic;

namespace RecipeRage.Store
{
    /// <summary>
    /// Interface for the store service, which manages store providers
    /// and provides a unified API for working with in-game purchases.
    /// </summary>
    public interface IStoreService
    {
        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Event triggered when the catalog is queried
        /// </summary>
        event Action<List<CatalogItem>> OnCatalogQueried;

        /// <summary>
        /// Event triggered when inventory is queried
        /// </summary>
        event Action<List<InventoryItem>> OnInventoryQueried;

        /// <summary>
        /// Event triggered when offers are queried
        /// </summary>
        event Action<List<StoreOffer>> OnOffersQueried;

        /// <summary>
        /// Event triggered when a purchase succeeds
        /// </summary>
        event Action<PurchaseResult> OnPurchaseSuccess;

        /// <summary>
        /// Event triggered when a purchase fails
        /// </summary>
        event Action<string, string> OnPurchaseFailed;

        /// <summary>
        /// Event triggered when an item is added to inventory
        /// </summary>
        event Action<InventoryItem> OnItemAdded;

        /// <summary>
        /// Event triggered when an item is consumed
        /// </summary>
        event Action<string, int> OnItemConsumed;

        /// <summary>
        /// Event triggered when currency balances change
        /// </summary>
        event Action<List<Currency>> OnCurrencyBalanceChanged;

        /// <summary>
        /// Initializes the store service and all available providers
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        void Initialize(Action<bool> callback);

        /// <summary>
        /// Queries the catalog of available items directly from the provider
        /// </summary>
        /// <param name="callback"> Callback with the list of catalog items and success flag </param>
        void QueryCatalog(Action<List<CatalogItem>, bool> callback);

        /// <summary>
        /// Adds a store provider to the service
        /// </summary>
        /// <param name="provider"> Provider to add </param>
        /// <returns> True if the provider was added successfully </returns>
        bool AddProvider(IStoreProvider provider);

        /// <summary>
        /// Gets a store provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider to get </param>
        /// <returns> The provider instance, or null if not found </returns>
        IStoreProvider GetProvider(string providerName);

        /// <summary>
        /// Gets the catalog of available items
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of catalog items </param>
        void GetCatalog(bool forceRefresh, Action<List<CatalogItem>> callback);

        /// <summary>
        /// Gets the player's owned items (inventory)
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of owned items </param>
        void GetInventory(bool forceRefresh, Action<List<InventoryItem>> callback);

        /// <summary>
        /// Gets the available offers for purchasing items
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of offers </param>
        void GetOffers(bool forceRefresh, Action<List<StoreOffer>> callback);

        /// <summary>
        /// Initiates a purchase for a specific offer
        /// </summary>
        /// <param name="offerId"> ID of the offer to purchase </param>
        /// <param name="callback"> Callback with the purchase result </param>
        void PurchaseOffer(string offerId, Action<PurchaseResult> callback);

        /// <summary>
        /// Consumes an inventory item (for consumable items)
        /// </summary>
        /// <param name="inventoryItemId"> ID of the inventory item to consume </param>
        /// <param name="quantity"> Quantity to consume </param>
        /// <param name="callback"> Callback indicating success or failure </param>
        void ConsumeItem(string inventoryItemId, int quantity, Action<bool> callback);

        /// <summary>
        /// Gets detailed information about a specific catalog item
        /// </summary>
        /// <param name="itemId"> ID of the item to query </param>
        /// <param name="callback"> Callback with the catalog item details </param>
        void GetCatalogItemDetails(string itemId, Action<CatalogItem> callback);

        /// <summary>
        /// Gets the available currencies and their balances
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="callback"> Callback with the list of currencies </param>
        void GetCurrencies(bool forceRefresh, Action<List<Currency>> callback);

        /// <summary>
        /// Gets the player's owned items of a specific type
        /// </summary>
        /// <param name="itemType"> Type of items to get </param>
        /// <param name="callback"> Callback with the list of owned items </param>
        void GetInventoryByType(ItemType itemType, Action<List<InventoryItem>> callback);

        /// <summary>
        /// Checks if the player owns a specific item
        /// </summary>
        /// <param name="itemId"> ID of the item to check </param>
        /// <param name="callback"> Callback with the result </param>
        void OwnsItem(string itemId, Action<bool> callback);

        /// <summary>
        /// Gets the quantity of a specific item in the player's inventory
        /// </summary>
        /// <param name="itemId"> ID of the item to check </param>
        /// <param name="callback"> Callback with the quantity </param>
        void GetItemQuantity(string itemId, Action<int> callback);

        /// <summary>
        /// Gets the balance of a specific currency
        /// </summary>
        /// <param name="currencyCode"> Currency code to check </param>
        /// <param name="callback"> Callback with the balance </param>
        void GetCurrencyBalance(string currencyCode, Action<decimal> callback);

        /// <summary>
        /// Opens the platform-specific store UI for a specific item (if supported)
        /// </summary>
        /// <param name="itemId"> ID of the item to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        bool DisplayStoreUI(string itemId = null);

        /// <summary>
        /// Checks if a purchase is in progress
        /// </summary>
        /// <returns> True if a purchase is in progress </returns>
        bool IsPurchaseInProgress();

        /// <summary>
        /// Restores previous purchases (useful for mobile platforms)
        /// </summary>
        /// <param name="callback"> Callback indicating success or failure </param>
        void RestorePurchases(Action<bool> callback);
    }
}