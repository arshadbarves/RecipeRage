using System;
using System.Collections.Generic;
using Core.RemoteConfig.Interfaces;
using Newtonsoft.Json;

namespace Core.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for shop system with rotation support
    /// </summary>
    [Serializable]
    public class ShopConfig : IConfigModel
    {
        [JsonProperty("categories")]
        public List<ShopCategory> Categories { get; set; }

        [JsonProperty("rotationSchedule")]
        public ShopRotationSchedule RotationSchedule { get; set; }

        [JsonProperty("specialOffers")]
        public List<SpecialOffer> SpecialOffers { get; set; }

        public ShopConfig()
        {
            Categories = new List<ShopCategory>();
            SpecialOffers = new List<SpecialOffer>();
        }

        public bool Validate()
        {
            if (Categories == null || Categories.Count == 0)
            {
                return false;
            }

            foreach (var category in Categories)
            {
                if (category.Items == null || category.Items.Count == 0)
                {
                    return false;
                }

                foreach (var item in category.Items)
                {
                    if (item.Price < 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    [Serializable]
    public class ShopCategory
    {
        [JsonProperty("categoryId")]
        public string CategoryId { get; set; }

        [JsonProperty("categoryName")]
        public string CategoryName { get; set; }

        [JsonProperty("items")]
        public List<ShopItem> Items { get; set; }

        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }

        public ShopCategory()
        {
            Items = new List<ShopItem>();
        }
    }

    [Serializable]
    public class ShopItem
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("itemName")]
        public string ItemName { get; set; }

        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("currencyType")]
        public string CurrencyType { get; set; }

        [JsonProperty("iconAddress")]
        public string IconAddress { get; set; }

        [JsonProperty("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonProperty("isLimitedTime")]
        public bool IsLimitedTime { get; set; }

        [JsonProperty("stock")]
        public int Stock { get; set; }

        [JsonProperty("maxPurchasePerPlayer")]
        public int MaxPurchasePerPlayer { get; set; }
    }

    [Serializable]
    public class ShopRotationSchedule
    {
        [JsonProperty("rotationPeriods")]
        public List<RotationPeriod> RotationPeriods { get; set; }

        [JsonProperty("defaultRotationDurationHours")]
        public int DefaultRotationDurationHours { get; set; }

        public ShopRotationSchedule()
        {
            RotationPeriods = new List<RotationPeriod>();
            DefaultRotationDurationHours = 24;
        }
    }

    [Serializable]
    public class RotationPeriod
    {
        [JsonProperty("periodId")]
        public string PeriodId { get; set; }

        [JsonProperty("startTimestamp")]
        public long StartTimestamp { get; set; }

        [JsonProperty("endTimestamp")]
        public long EndTimestamp { get; set; }

        [JsonProperty("featuredItemIds")]
        public List<string> FeaturedItemIds { get; set; }

        public RotationPeriod()
        {
            FeaturedItemIds = new List<string>();
        }

        public DateTime GetStartTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(StartTimestamp).UtcDateTime;
        }

        public DateTime GetEndTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(EndTimestamp).UtcDateTime;
        }
    }

    [Serializable]
    public class SpecialOffer
    {
        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("offerName")]
        public string OfferName { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("originalPrice")]
        public int OriginalPrice { get; set; }

        [JsonProperty("discountedPrice")]
        public int DiscountedPrice { get; set; }

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage { get; set; }

        [JsonProperty("startTimestamp")]
        public long StartTimestamp { get; set; }

        [JsonProperty("endTimestamp")]
        public long EndTimestamp { get; set; }

        [JsonProperty("currencyType")]
        public string CurrencyType { get; set; }

        public DateTime GetStartTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(StartTimestamp).UtcDateTime;
        }

        public DateTime GetEndTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(EndTimestamp).UtcDateTime;
        }

        public bool IsActive(DateTime currentTime)
        {
            return currentTime >= GetStartTime() && currentTime <= GetEndTime();
        }
    }
}
