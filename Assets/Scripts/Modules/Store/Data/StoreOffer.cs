using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Store
{
    /// <summary>
    /// Represents a payment method for purchases
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Unknown or undefined payment method
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Real money purchase
        /// </summary>
        RealMoney,
        
        /// <summary>
        /// Virtual currency purchase
        /// </summary>
        VirtualCurrency,
        
        /// <summary>
        /// Free (no payment required)
        /// </summary>
        Free,
        
        /// <summary>
        /// Hybrid payment method (combination of real money and virtual currency)
        /// </summary>
        Hybrid
    }

    /// <summary>
    /// Represents a purchase offer in the store
    /// </summary>
    [Serializable]
    public class StoreOffer
    {
        /// <summary>
        /// Unique identifier for the offer
        /// </summary>
        public string OfferId { get; set; }
        
        /// <summary>
        /// Display name of the offer
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description of the offer
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Long description of the offer
        /// </summary>
        public string LongDescription { get; set; }
        
        /// <summary>
        /// External ID for the offer (e.g., product ID in platform store)
        /// </summary>
        public string ExternalId { get; set; }
        
        /// <summary>
        /// Items included in this offer
        /// </summary>
        public List<OfferItem> Items { get; set; } = new List<OfferItem>();
        
        /// <summary>
        /// Regular price in real money
        /// </summary>
        public decimal RegularPrice { get; set; }
        
        /// <summary>
        /// Current price in real money (may include discounts)
        /// </summary>
        public decimal CurrentPrice { get; set; }
        
        /// <summary>
        /// Currency code for the real money price (e.g., "USD")
        /// </summary>
        public string CurrencyCode { get; set; }
        
        /// <summary>
        /// Price in virtual currency
        /// </summary>
        public decimal VirtualPrice { get; set; }
        
        /// <summary>
        /// Virtual currency code for the virtual price (e.g., "GOLD")
        /// </summary>
        public string VirtualCurrencyCode { get; set; }
        
        /// <summary>
        /// Payment method required for this offer
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }
        
        /// <summary>
        /// Start date when the offer becomes available
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// End date when the offer expires
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether the offer is currently available for purchase
        /// </summary>
        public bool IsAvailable { get; set; } = true;
        
        /// <summary>
        /// Whether the offer is featured in the store
        /// </summary>
        public bool IsFeatured { get; set; }
        
        /// <summary>
        /// Tags for filtering and categorization
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Category of the offer for grouping
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Icon for the offer
        /// </summary>
        public Sprite Icon { get; set; }
        
        /// <summary>
        /// URL to the icon image (if not loaded)
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// Banner image for the offer
        /// </summary>
        public Sprite BannerImage { get; set; }
        
        /// <summary>
        /// URL to the banner image (if not loaded)
        /// </summary>
        public string BannerImageUrl { get; set; }
        
        /// <summary>
        /// Name of the provider this offer came from
        /// </summary>
        public string ProviderName { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }
        
        /// <summary>
        /// Custom properties for the offer
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public StoreOffer()
        {
            IsAvailable = true;
        }

        /// <summary>
        /// Constructor with basic properties
        /// </summary>
        /// <param name="offerId">ID of the offer</param>
        /// <param name="displayName">Display name of the offer</param>
        /// <param name="providerName">Name of the provider</param>
        public StoreOffer(string offerId, string displayName, string providerName)
        {
            OfferId = offerId;
            DisplayName = displayName;
            ProviderName = providerName;
            IsAvailable = true;
        }

        /// <summary>
        /// Gets whether the offer is currently active based on start/end dates
        /// </summary>
        public bool IsActive
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                bool afterStart = !StartDate.HasValue || StartDate.Value <= now;
                bool beforeEnd = !EndDate.HasValue || EndDate.Value >= now;
                return afterStart && beforeEnd && IsAvailable;
            }
        }

        /// <summary>
        /// Gets the discount percentage (if any)
        /// </summary>
        public decimal DiscountPercentage
        {
            get
            {
                if (RegularPrice <= 0)
                    return 0;
                
                decimal discount = RegularPrice - CurrentPrice;
                return Math.Round((discount / RegularPrice) * 100, 2);
            }
        }

        /// <summary>
        /// Gets whether the offer is discounted
        /// </summary>
        public bool IsDiscounted
        {
            get
            {
                return CurrentPrice < RegularPrice && RegularPrice > 0;
            }
        }

        /// <summary>
        /// Formats the current price for display
        /// </summary>
        /// <returns>Formatted price string</returns>
        public string FormatPrice()
        {
            switch (PaymentMethod)
            {
                case PaymentMethod.RealMoney:
                    if (string.IsNullOrEmpty(CurrencyCode))
                        return CurrentPrice.ToString("N2");
                        
                    switch (CurrencyCode.ToUpper())
                    {
                        case "USD":
                            return $"${CurrentPrice:N2}";
                        case "EUR":
                            return $"€{CurrentPrice:N2}";
                        case "GBP":
                            return $"£{CurrentPrice:N2}";
                        case "JPY":
                            return $"¥{CurrentPrice:N0}";
                        default:
                            return $"{CurrentPrice:N2} {CurrencyCode}";
                    }
                
                case PaymentMethod.VirtualCurrency:
                    if (VirtualCurrencyCode == null)
                        return VirtualPrice.ToString("N0");
                    
                    return $"{VirtualPrice:N0} {VirtualCurrencyCode}";
                
                case PaymentMethod.Free:
                    return "FREE";
                
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Formats the regular price for display
        /// </summary>
        /// <returns>Formatted regular price string</returns>
        public string FormatRegularPrice()
        {
            if (PaymentMethod != PaymentMethod.RealMoney)
                return FormatPrice();
                
            if (string.IsNullOrEmpty(CurrencyCode))
                return RegularPrice.ToString("N2");
                
            switch (CurrencyCode.ToUpper())
            {
                case "USD":
                    return $"${RegularPrice:N2}";
                case "EUR":
                    return $"€{RegularPrice:N2}";
                case "GBP":
                    return $"£{RegularPrice:N2}";
                case "JPY":
                    return $"¥{RegularPrice:N0}";
                default:
                    return $"{RegularPrice:N2} {CurrencyCode}";
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
    /// Represents an item in an offer
    /// </summary>
    [Serializable]
    public class OfferItem
    {
        /// <summary>
        /// ID of the item in the offer
        /// </summary>
        public string ItemId { get; set; }
        
        /// <summary>
        /// Quantity of the item in the offer
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Reference to the full catalog item (if available)
        /// </summary>
        public CatalogItem CatalogItem { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OfferItem()
        {
            Quantity = 1;
        }

        /// <summary>
        /// Constructor with item ID and quantity
        /// </summary>
        /// <param name="itemId">ID of the item</param>
        /// <param name="quantity">Quantity of the item</param>
        public OfferItem(string itemId, int quantity)
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
        public OfferItem(string itemId, int quantity, CatalogItem catalogItem)
        {
            ItemId = itemId;
            Quantity = quantity;
            CatalogItem = catalogItem;
        }
    }
} 