using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Store
{
    /// <summary>
    /// Represents an item in the store catalog
    /// </summary>
    [Serializable]
    public class CatalogItem
    {
        /// <summary>
        /// Unique identifier for the catalog item
        /// </summary>
        public string ItemId { get; set; }
        
        /// <summary>
        /// Display name of the item
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Long description of the item
        /// </summary>
        public string LongDescription { get; set; }
        
        /// <summary>
        /// Type of the item
        /// </summary>
        public ItemType ItemType { get; set; }
        
        /// <summary>
        /// Category of the item for grouping
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Tags for filtering and categorization
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Icon for the item
        /// </summary>
        public Sprite Icon { get; set; }
        
        /// <summary>
        /// URL to the icon image (if not loaded)
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// Preview image for the item
        /// </summary>
        public Sprite PreviewImage { get; set; }
        
        /// <summary>
        /// URL to the preview image (if not loaded)
        /// </summary>
        public string PreviewImageUrl { get; set; }
        
        /// <summary>
        /// Additional images for the item
        /// </summary>
        public List<Sprite> AdditionalImages { get; set; } = new List<Sprite>();
        
        /// <summary>
        /// URLs to additional images (if not loaded)
        /// </summary>
        public List<string> AdditionalImageUrls { get; set; } = new List<string>();
        
        /// <summary>
        /// Items contained in this item (for bundles)
        /// </summary>
        public List<BundleItem> BundleItems { get; set; } = new List<BundleItem>();
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }
        
        /// <summary>
        /// Name of the provider this item came from
        /// </summary>
        public string ProviderName { get; set; }
        
        /// <summary>
        /// Release date of the item
        /// </summary>
        public DateTime? ReleaseDate { get; set; }
        
        /// <summary>
        /// Expiration date of the item (if applicable)
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Whether the item is currently available for purchase
        /// </summary>
        public bool IsAvailable { get; set; } = true;
        
        /// <summary>
        /// Whether the item is featured in the store
        /// </summary>
        public bool IsFeatured { get; set; }
        
        /// <summary>
        /// Custom properties for the item
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public CatalogItem()
        {
        }

        /// <summary>
        /// Constructor with basic properties
        /// </summary>
        /// <param name="itemId">Unique ID of the item</param>
        /// <param name="displayName">Display name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="itemType">Type of the item</param>
        /// <param name="providerName">Name of the provider</param>
        public CatalogItem(string itemId, string displayName, string description, ItemType itemType, string providerName)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Description = description;
            ItemType = itemType;
            ProviderName = providerName;
            IsAvailable = true;
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

    /// <summary>
    /// Represents an item in a bundle
    /// </summary>
    [Serializable]
    public class BundleItem
    {
        /// <summary>
        /// ID of the item in the bundle
        /// </summary>
        public string ItemId { get; set; }
        
        /// <summary>
        /// Quantity of the item in the bundle
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Reference to the full catalog item (if available)
        /// </summary>
        public CatalogItem CatalogItem { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BundleItem()
        {
            Quantity = 1;
        }

        /// <summary>
        /// Constructor with item ID and quantity
        /// </summary>
        /// <param name="itemId">ID of the item</param>
        /// <param name="quantity">Quantity of the item</param>
        public BundleItem(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }

        /// <summary>
        /// Constructor with item ID, quantity, and catalog item
        /// </summary>
        /// <param name="itemId">ID of the item</param>
        /// <param name="quantity">Quantity of the item</param>
        /// <param name="catalogItem">Full catalog item</param>
        public BundleItem(string itemId, int quantity, CatalogItem catalogItem)
        {
            ItemId = itemId;
            Quantity = quantity;
            CatalogItem = catalogItem;
        }
    }
} 