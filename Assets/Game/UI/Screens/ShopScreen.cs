using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Shop: coin packs (IAP), chef unlocks (coins), daily ad deal, starter pack.
    /// Purchases grant via IIAPService.OnPurchaseCompleted; stub mode auto-completes.
    /// </summary>
    [UIScreen]
    public sealed class ShopScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();

            Root.Q<Button>("coin-pack-500").clicked += () => BuyCoinPack("coins_500", 500);
            Root.Q<Button>("coin-pack-1200").clicked += () => BuyCoinPack("coins_1200", 1200);
            Root.Q<Button>("coin-pack-3000").clicked += () => BuyCoinPack("coins_3000", 3000);
            Root.Q<Button>("coin-pack-8000").clicked += () => BuyCoinPack("coins_8000", 8000);

            Root.Q<Button>("starter-pack").clicked += () => BuyStarterPack();

            var progression = ServiceLocator.Get<IChefProgressionService>();
            Root.Q<Button>("unlock-marco").clicked += () => progression.TryUnlock(ChefId.Marco);
            Root.Q<Button>("unlock-gustavo").clicked += () => progression.TryUnlock(ChefId.Gustavo);

            Root.Q<Button>("daily-ad").clicked += () =>
            {
                ServiceLocator.Get<IAdsService>().ShowRewardedAd("shop_daily_deal", success =>
                {
                    if (success)
                    {
                        ServiceLocator.Get<IWalletService>().AddCoins(100);
                    }
                });
            };
        }

        private void BuyCoinPack(string productId, int coins)
        {
            var iap = ServiceLocator.Get<IIAPService>();
            void Handler(string purchased)
            {
                if (purchased == productId)
                {
                    ServiceLocator.Get<IWalletService>().AddCoins(coins);
                    iap.OnPurchaseCompleted -= Handler;
                }
            }
            iap.OnPurchaseCompleted += Handler;
            iap.Purchase(productId);
        }

        private void BuyStarterPack()
        {
            var iap = ServiceLocator.Get<IIAPService>();
            void Handler(string purchased)
            {
                if (purchased == "starter_pack")
                {
                    ServiceLocator.Get<IWalletService>().AddCoins(1000);
                    ServiceLocator.Get<IChefProgressionService>().TryUnlock(ChefId.Marco);
                    iap.OnPurchaseCompleted -= Handler;
                }
            }
            iap.OnPurchaseCompleted += Handler;
            iap.Purchase("starter_pack");
        }
    }
}
