namespace KitchenClash.Application.Models
{
    public sealed class IAPItem
    {
        public string ProductId { get; }
        public string DisplayName { get; }
        public decimal PriceUSD { get; }
        public int Gems { get; }
        public string BonusContent { get; }
        public bool IsOneTimePurchase { get; }

        public IAPItem(string productId, string displayName, decimal priceUSD, int gems,
            string bonusContent = null, bool isOneTimePurchase = false)
        {
            ProductId = productId;
            DisplayName = displayName;
            PriceUSD = priceUSD;
            Gems = gems;
            BonusContent = bonusContent;
            IsOneTimePurchase = isOneTimePurchase;
        }
    }
}
