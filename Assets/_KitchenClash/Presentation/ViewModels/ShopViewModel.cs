using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using KitchenClash.Application.Services;
using KitchenClash.Application;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.EOS;
using System.Collections.Generic;
using VContainer;

namespace KitchenClash.Presentation.ViewModels
{
    public class ShopViewModel : BaseViewModel
    {
        private readonly ISessionContext _sessionContext;
        private readonly ShopCatalog _shopCatalog;
        private EconomyService EconomyService => _sessionContext.EconomyService;
        private ICharacterService CharacterService => _sessionContext.CharacterService;

        public BindableProperty<string> CoinsText { get; } = new BindableProperty<string>("0");
        public BindableProperty<string> GemsText { get; } = new BindableProperty<string>("0");

        [Inject]
        public ShopViewModel(ISessionContext sessionContext, ShopCatalog shopCatalog)
        {
            _sessionContext = sessionContext;
            _shopCatalog = shopCatalog;
        }

        public override void Initialize()
        {
            base.Initialize();
            UpdateCurrency();
        }

        public void UpdateCurrency()
        {
            if (EconomyService == null) return;
            CoinsText.Value = EconomyService.GetBalance(EconomyKeys.CurrencyCoins).ToString();
            GemsText.Value = EconomyService.GetBalance(EconomyKeys.CurrencyGems).ToString();
        }

        public IReadOnlyList<ShopItem> GetCatalogItems(string category)
            => _shopCatalog?.GetByCategory(category);

        public IReadOnlyList<ShopItem> GetAllCatalogItems()
            => _shopCatalog?.GetAllItems();

        public bool IsOwned(string itemId) => EconomyService?.HasItem(itemId) ?? false;

        public bool BuyItem(ShopItem item)
        {
            if (EconomyService == null) return false;

            // Look up canonical item from catalog if available
            var catalogItem = _shopCatalog?.GetItem(item.id) ?? item;

            string currencyId = catalogItem.currency.ToLower();
            bool success = EconomyService.Purchase(catalogItem.id, catalogItem.price, currencyId);

            if (success)
            {
                // If character purchase, unlock via CharacterService
                if (catalogItem.category == ShopCatalog.CategoryCharacters && CharacterService != null)
                {
                    // Map item id to character unlock (e.g. "chef_Grandpa" → ChefId enum)
                    string chefName = catalogItem.id.Replace("chef_", "");
                    if (System.Enum.TryParse<ChefId>(chefName, false, out var chefId))
                        CharacterService.TryPurchaseChef(chefId);
                    else
                        CharacterService.Unlock(chefName.GetHashCode());
                }

                UpdateCurrency();
            }

            return success;
        }
    }
}
