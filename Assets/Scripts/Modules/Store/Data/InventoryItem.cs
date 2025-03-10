using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Store
{
    /// <summary>
    /// Represents an item in a player's inventory
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        /// <summary>
        /// Unique identifier for this inventory instance
        /// </summary>
        public string InventoryItemId { get; set; }
        
        /// <summary>
        /// ID of the catalog item this inventory item represents
        /// </summary>
        public string CatalogItemId { get; set; }
        
        /// <summary>
        /// Reference to the catalog item (if available)
        /// </summary>
        public CatalogItem CatalogItem { get; set; }
        
        /// <summary>
        /// Quantity of the item owned
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Date when the item was acquired
        /// </summary>
        public DateTime AcquisitionDate { get; set; }
        
        /// <summary>
        /// Date when the item expires (if applicable)
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Whether the item is currently usable
        /// </summary>
        public bool IsUsable { get; set; } = true;
        
        /// <summary>
        /// Transaction ID associated with the purchase of this item
        /// </summary>
        public string TransactionId { get; set; }
        
        /// <summary>
        /// Name of the provider this item came from
        /// </summary>
        public string ProviderName { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }
        
        /// <summary>
        /// Custom properties for the inventory item
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public InventoryItem()
        {
            AcquisitionDate = DateTime.UtcNow;
            Quantity = 1;
        }

        /// <summary>
        /// Constructor with basic properties
        /// </summary>
        /// <param name="inventoryItemId">ID of the inventory item</param>
        /// <param name="catalogItemId">ID of the catalog item</param>
        /// <param name="quantity">Quantity of the item</param>
        /// <param name="providerName">Name of the provider</param>
        public InventoryItem(string inventoryItemId, string catalogItemId, int quantity, string providerName)
        {
            InventoryItemId = inventoryItemId;
            CatalogItemId = catalogItemId;
            Quantity = quantity;
            ProviderName = providerName;
            AcquisitionDate = DateTime.UtcNow;
            IsUsable = true;
        }

        /// <summary>
        /// Constructor with catalog item
        /// </summary>
        /// <param name="inventoryItemId">ID of the inventory item</param>
        /// <param name="catalogItem">Catalog item</param>
        /// <param name="quantity">Quantity of the item</param>
        public InventoryItem(string inventoryItemId, CatalogItem catalogItem, int quantity)
        {
            InventoryItemId = inventoryItemId;
            CatalogItemId = catalogItem.ItemId;
            CatalogItem = catalogItem;
            Quantity = quantity;
            ProviderName = catalogItem.ProviderName;
            AcquisitionDate = DateTime.UtcNow;
            IsUsable = true;
        }

        /// <summary>
        /// Gets whether the item is expired
        /// </summary>
        public bool IsExpired
        {
            get
            {
                return ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets whether the item is available for use
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return IsUsable && !IsExpired && Quantity > 0;
            }
        }

        /// <summary>
        /// Gets the display name of the item
        /// </summary>
        public string DisplayName
        {
            get
            {
                return CatalogItem != null ? CatalogItem.DisplayName : CatalogItemId;
            }
        }

        /// <summary>
        /// Gets the description of the item
        /// </summary>
        public string Description
        {
            get
            {
                return CatalogItem != null ? CatalogItem.Description : string.Empty;
            }
        }

        /// <summary>
        /// Gets the icon for the item
        /// </summary>
        public Sprite Icon
        {
            get
            {
                return CatalogItem != null ? CatalogItem.Icon : null;
            }
        }

        /// <summary>
        /// Gets the item type
        /// </summary>
        public ItemType ItemType
        {
            get
            {
                return CatalogItem != null ? CatalogItem.ItemType : ItemType.Unknown;
            }
        }

        /// <summary>
        /// Gets a property as a string
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist</param>
        /// <returns>The property value or default value</returns>
        public string GetProperty(string key, string defaultValue = "")
        {
            if (Properties.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a property as an integer
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist or isn't a valid integer</param>
        /// <returns>The property value as an integer or default value</returns>
        public int GetPropertyAsInt(string key, int defaultValue = 0)
        {
            if (Properties.TryGetValue(key, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a property as a float
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist or isn't a valid float</param>
        /// <returns>The property value as a float or default value</returns>
        public float GetPropertyAsFloat(string key, float defaultValue = 0f)
        {
            if (Properties.TryGetValue(key, out string value) && float.TryParse(value, out float result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a property as a boolean
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist or isn't a valid boolean</param>
        /// <returns>The property value as a boolean or default value</returns>
        public bool GetPropertyAsBool(string key, bool defaultValue = false)
        {
            if (Properties.TryGetValue(key, out string value) && bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets a property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, string value)
        {
            Properties[key] = value;
        }

        /// <summary>
        /// Sets a property from an integer
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, int value)
        {
            Properties[key] = value.ToString();
        }

        /// <summary>
        /// Sets a property from a float
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, float value)
        {
            Properties[key] = value.ToString();
        }

        /// <summary>
        /// Sets a property from a boolean
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, bool value)
        {
            Properties[key] = value.ToString();
        }
    }
} 