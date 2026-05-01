using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// In-memory catalog of all purchasable shop items, grouped by category.
    /// </summary>
    public class ShopCatalog
    {
        public const string CategoryCharacters = "characters";
        public const string CategorySkins = "skins";
        public const string CategoryBoosters = "boosters";

        private readonly List<ShopItem> _items;
        private readonly Dictionary<string, ShopItem> _lookup;

        public ShopCatalog()
        {
            _items = BuildDefaultCatalog();
            _lookup = _items.ToDictionary(i => i.id);
        }

        public IReadOnlyList<ShopItem> GetAllItems() => _items;

        public IReadOnlyList<ShopItem> GetByCategory(string category)
            => _items.Where(i => i.category == category).ToList();

        public ShopItem GetItem(string itemId)
            => _lookup.TryGetValue(itemId, out var item) ? item : null;

        private static List<ShopItem> BuildDefaultCatalog()
        {
            return new List<ShopItem>
            {
                // ── Characters (coins) ──
                new ShopItem
                {
                    id = "char_grandpa", name = "Grandpa", description = "A seasoned chef with decades of experience.",
                    price = 500, currency = EconomyKeys.CurrencyCoins, rarity = "rare", badge = "", category = CategoryCharacters
                },
                new ShopItem
                {
                    id = "char_raj", name = "Raj", description = "A spice master from Mumbai.",
                    price = 1000, currency = EconomyKeys.CurrencyCoins, rarity = "epic", badge = "", category = CategoryCharacters
                },

                // ── Skins (gems) ──
                new ShopItem
                {
                    id = "skin_chef_golden", name = "Golden Chef Outfit", description = "A shimmering golden outfit for any chef.",
                    price = 200, currency = EconomyKeys.CurrencyGems, rarity = "epic", badge = "new", category = CategorySkins
                },
                new ShopItem
                {
                    id = "skin_chef_neon", name = "Neon Chef Outfit", description = "Light up the kitchen with neon style.",
                    price = 150, currency = EconomyKeys.CurrencyGems, rarity = "rare", badge = "", category = CategorySkins
                },
                new ShopItem
                {
                    id = "skin_chef_retro", name = "Retro Chef Outfit", description = "Classic 50s diner look.",
                    price = 100, currency = EconomyKeys.CurrencyGems, rarity = "common", badge = "", category = CategorySkins
                },
                new ShopItem
                {
                    id = "skin_chef_ice", name = "Frozen Chef Outfit", description = "Cool as ice in the kitchen.",
                    price = 300, currency = EconomyKeys.CurrencyGems, rarity = "legendary", badge = "limited", category = CategorySkins
                },
                new ShopItem
                {
                    id = "skin_chef_flame", name = "Flame Chef Outfit", description = "Bring the heat with this fiery look.",
                    price = 500, currency = EconomyKeys.CurrencyGems, rarity = "legendary", badge = "sale", category = CategorySkins
                },

                // ── Boosters (gems) ──
                new ShopItem
                {
                    id = "boost_xp", name = "XP Boost", description = "Double XP for one match.",
                    price = 50, currency = EconomyKeys.CurrencyGems, rarity = "common", badge = "", category = CategoryBoosters
                },
                new ShopItem
                {
                    id = "boost_coin_mult", name = "Coin Multiplier", description = "2x coin rewards for one match.",
                    price = 100, currency = EconomyKeys.CurrencyGems, rarity = "rare", badge = "", category = CategoryBoosters
                },
                new ShopItem
                {
                    id = "boost_score", name = "Score Boost", description = "+25% score for one match.",
                    price = 75, currency = EconomyKeys.CurrencyGems, rarity = "rare", badge = "new", category = CategoryBoosters
                },
            };
        }
    }
}
