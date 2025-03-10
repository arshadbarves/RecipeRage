# In-Game Store Module

The In-Game Store Module provides a flexible and robust framework for implementing in-game purchases, virtual currencies, and inventory management in RecipeRage. It integrates with Epic Online Services (EOS) for handling real-money purchases and supports a wide range of microtransaction models.

## Features

- **Multi-provider architecture**: Easily integrate with different store providers (currently supports EOS)
- **Catalog management**: Browse available virtual goods and their details
- **Purchase processing**: Handle purchases of virtual goods with various payment methods
- **Inventory management**: Track player-owned items and their quantities
- **Consumable items**: Support for items that can be consumed by the player
- **Currency management**: Handle virtual currencies and real-money transactions
- **Purchase restoration**: Restore previously purchased items
- **Offer management**: Create and manage special offers and bundles
- **Event-based architecture**: Subscribe to store events for UI updates

## Directory Structure

```
Assets/Scripts/Modules/Store/
├── Core/                     - Core service implementations
│   └── StoreService.cs       - Primary service implementation
├── Data/                     - Data models
│   ├── ItemType.cs           - Enum for different types of store items
│   ├── CatalogItem.cs        - Represents an item in the store catalog
│   ├── Currency.cs           - Represents a virtual currency
│   ├── InventoryItem.cs      - Represents an owned item in player inventory
│   ├── StoreOffer.cs         - Represents a purchasable offer
│   └── PurchaseResult.cs     - Result of a purchase operation
├── Interfaces/               - Interface definitions
│   ├── IStoreProvider.cs     - Interface for store provider implementations
│   └── IStoreService.cs      - Core store service interface
├── Providers/                - Store provider implementations
│   └── EOS/                  - Epic Online Services implementation
│       └── EOSStoreProvider.cs - EOS-specific store provider
├── StoreHelper.cs            - Static helper for easy access to store functionality
└── README.md                 - This documentation file
```

## Getting Started

### Prerequisites

- The Epic Online Services SDK must be installed in your project
- The user must be authenticated with EOS before using store features

### Initialization

```csharp
// The simplest way to initialize the store module
StoreHelper.Initialize((success) => {
    if (success) {
        Debug.Log("Store module initialized successfully");
    } else {
        Debug.LogError("Failed to initialize store module");
    }
});

// For more control, you can work directly with the service
IStoreService storeService = new StoreService();
storeService.AddProvider(new EOSStoreProvider());
storeService.Initialize((success) => {
    if (success) {
        Debug.Log("Store service initialized successfully");
    }
});
```

## Basic Usage

### Querying the Catalog

```csharp
StoreHelper.QueryCatalog((items, success) => {
    if (success) {
        Debug.Log($"Found {items.Count} items in catalog");
        foreach (var item in items) {
            Debug.Log($"Item: {item.DisplayName}, Type: {item.ItemType}");
        }
    }
});
```

### Querying the Player's Inventory

```csharp
StoreHelper.QueryInventory((items, success) => {
    if (success) {
        Debug.Log($"Found {items.Count} items in inventory");
        foreach (var item in items) {
            Debug.Log($"Item: {item.CatalogItemId}, Quantity: {item.Quantity}");
        }
    }
});
```

### Making a Purchase

```csharp
string offerId = "premium_sword_offer";
StoreHelper.PurchaseOffer(offerId, (result) => {
    if (result.Status == PurchaseStatus.Success) {
        Debug.Log($"Purchase successful! Transaction ID: {result.TransactionId}");
        Debug.Log($"Received {result.GrantedItems.Count} items");
    } else {
        Debug.LogError($"Purchase failed: {result.ErrorMessage}");
    }
});
```

### Consuming an Item

```csharp
string inventoryItemId = "healing_potion_123";
int quantity = 1;
StoreHelper.ConsumeItem(inventoryItemId, quantity, (success) => {
    if (success) {
        Debug.Log($"Successfully consumed {quantity} of item {inventoryItemId}");
    } else {
        Debug.LogError("Failed to consume item");
    }
});
```

### Restoring Purchases

```csharp
StoreHelper.RestorePurchases((success, restoredItems) => {
    if (success) {
        Debug.Log($"Successfully restored {restoredItems.Count} purchases");
    } else {
        Debug.LogError("Failed to restore purchases");
    }
});
```

## Event Handling

The Store module provides events for various operations to support reactive UI updates.

```csharp
// Register for purchase success events
StoreHelper.RegisterPurchaseSuccessCallback((result) => {
    Debug.Log($"Purchase succeeded: {result.OfferId}");
    // Update UI
});

// Register for inventory updates
StoreHelper.RegisterInventoryQueryCallback((items, success) => {
    if (success) {
        // Update inventory UI
    }
});

// Register for item consumption
StoreHelper.RegisterItemConsumedCallback((itemId, quantity) => {
    Debug.Log($"Item {itemId} was consumed, quantity: {quantity}");
    // Update UI
});
```

## Epic Online Services Integration

The Store module integrates with Epic Online Services to provide a secure and reliable purchasing system. The EOS provider handles:

- Catalog management through EOS offers
- Purchase processing with real-money transactions
- Ownership verification
- Purchase restoration

### EOS Setup

Before using the EOS store provider, make sure:

1. Your game is properly set up with EOS
2. You have configured your catalog items in the EOS Developer Portal
3. The user is authenticated with EOS

## Advanced Usage

### Working with Currencies

```csharp
StoreHelper.GetCurrencies((currencies, success) => {
    if (success) {
        foreach (var currency in currencies) {
            Debug.Log($"Currency: {currency.Code}, Balance: {currency.Balance}");
        }
    }
});
```

### Validating Receipts

```csharp
string receipt = "purchase_receipt_data";
StoreHelper.ValidateReceipt(receipt, (isValid) => {
    Debug.Log($"Receipt is valid: {isValid}");
});
```

### Implementing Custom Store Providers

To add a new store provider (e.g., Steam, Google Play, etc.):

1. Create a new class that implements `IStoreProvider`
2. Implement all required methods for your platform
3. Add your provider to the store service:

```csharp
storeService.AddProvider(new YourCustomProvider());
```

## Error Handling

The Store module provides detailed error information through the `PurchaseResult` object and callback parameters:

```csharp
StoreHelper.PurchaseOffer(offerId, (result) => {
    if (result.Status != PurchaseStatus.Success) {
        Debug.LogError($"Purchase error: {result.ErrorMessage}");
        
        switch (result.Status) {
            case PurchaseStatus.Failed:
                // Handle general failure
                break;
            case PurchaseStatus.Cancelled:
                // Handle user cancellation
                break;
            case PurchaseStatus.Pending:
                // Handle pending transaction
                break;
        }
    }
});
```

## Performance Considerations

- Cache catalog and inventory data when appropriate to reduce network calls
- Use batch operations when available for better performance
- Consider the UI implications of network latency in the purchase flow

## Thread Safety

The Store module is designed to be used from the main thread. All callbacks are guaranteed to be called on the main thread.

## Example

The module includes a comprehensive example scene in `Assets/Scripts/Examples/StoreExample.cs` that demonstrates all the core functionality.

## License

© 2023 RecipeRage. All rights reserved. 