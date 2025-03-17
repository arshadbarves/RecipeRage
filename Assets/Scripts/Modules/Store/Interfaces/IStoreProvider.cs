using System;
using System.Collections.Generic;

namespace RecipeRage.Store
{
    /// <summary>
    /// Interface for store providers (e.g., EOS, Steam, custom, etc.)
    /// </summary>
    public interface IStoreProvider
    {
        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Indicates if the provider is available and initialized
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Initializes the store provider
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        void Initialize(Action<bool> callback);

        /// <summary>
        /// Queries the catalog of available items from the provider
        /// </summary>
        /// <param name="callback"> Callback with the list of catalog items </param>
        void QueryCatalog(Action<List<CatalogItem>, bool> callback);

        /// <summary>
        /// Queries the player's owned items (inventory)
        /// </summary>
        /// <param name="callback"> Callback with the list of owned items </param>
        void QueryInventory(Action<List<InventoryItem>, bool> callback);

        /// <summary>
        /// Queries the available offers for purchasing items
        /// </summary>
        /// <param name="callback"> Callback with the list of offers </param>
        void QueryOffers(Action<List<StoreOffer>, bool> callback);

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
        /// Validates a purchase receipt
        /// </summary>
        /// <param name="receipt"> Receipt to validate </param>
        /// <param name="callback"> Callback indicating if the receipt is valid </param>
        void ValidateReceipt(string receipt, Action<bool> callback);

        /// <summary>
        /// Gets detailed information about a specific catalog item
        /// </summary>
        /// <param name="itemId"> ID of the item to query </param>
        /// <param name="callback"> Callback with the catalog item details </param>
        void GetCatalogItemDetails(string itemId, Action<CatalogItem, bool> callback);

        /// <summary>
        /// Gets the available currencies and their balances
        /// </summary>
        /// <param name="callback"> Callback with the list of currencies </param>
        void GetCurrencies(Action<List<Currency>, bool> callback);

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
        /// <param name="callback"> Callback indicating success or failure, with restored items </param>
        void RestorePurchases(Action<bool, List<InventoryItem>> callback);
    }
}