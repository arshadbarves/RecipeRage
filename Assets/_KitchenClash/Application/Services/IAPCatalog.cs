using System.Collections.Generic;
using KitchenClash.Application.Models;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// Static IAP catalog matching GDD v3 monetisation table.
    /// </summary>
    public static class IAPCatalog
    {
        public static readonly IReadOnlyList<IAPItem> Items = new[]
        {
            new IAPItem("starter_bundle", "Starter Bundle", 2.99m, 200,
                "Chef Marco + 2 Rare skins", isOneTimePurchase: true),

            new IAPItem("gem_pack_s", "Small Gem Pack", 0.99m, 80),

            new IAPItem("gem_pack_m", "Medium Gem Pack", 4.99m, 500,
                "+50% bonus on first purchase"),

            new IAPItem("gem_pack_l", "Large Gem Pack", 9.99m, 1200),

            new IAPItem("gem_pack_xl", "XL Gem Pack", 49.99m, 6500),

            new IAPItem("battle_pass_premium", "Battle Pass Premium", 4.99m, 0,
                "Season Battle Pass"),

            new IAPItem("battle_pass_plus", "Battle Pass Plus", 8.99m, 0,
                "+20% XP boost for season"),
        };

        public static IAPItem GetById(string productId)
        {
            foreach (var item in Items)
                if (item.ProductId == productId)
                    return item;
            return null;
        }
    }
}
