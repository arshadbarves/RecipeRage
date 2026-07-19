using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Application.Config;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.IAP
{
    /// <summary>
    /// Stub IAP service: succeeds in editor, fails in production builds.
    /// Delivers gems via IEconomyService on successful purchase.
    /// </summary>
    public sealed class StubIAPService : IIAPService
    {
        private readonly IEconomyService _economy;
        private readonly IAnalyticsService _analytics;

        public bool IsInitialized => true;

        public StubIAPService(IEconomyService economy, IAnalyticsService analytics = null)
        {
            _economy = economy;
            _analytics = analytics;
        }

        public async Task<IAPResult> PurchaseAsync(string productId)
        {
            await Task.Yield();

#if UNITY_EDITOR
            GameLogger.Log($"[StubIAPService] Simulating purchase: {productId}");

            IAPItem item = IAPCatalog.GetById(productId);
            if (item == null)
            {
                LogPurchaseFail(productId, "Product not found in catalog");
                return new IAPResult(false, productId, "Product not found in catalog");
            }

            if (item.Gems > 0 && _economy != null)
            {
                _economy.AddGems(item.Gems);
                GameLogger.Log($"[StubIAPService] Delivered {item.Gems} gems for {productId}");
            }

            _analytics?.LogEvent(AnalyticsEvents.PurchaseSuccess, new Dictionary<string, object>
            {
                { AnalyticsEvents.Params.ProductId, productId ?? string.Empty },
                { AnalyticsEvents.Params.Success, true },
                { AnalyticsEvents.Params.Amount, item.Gems }
            });
            _analytics?.LogEvent(AnalyticsEvents.IapCompleted, new Dictionary<string, object>
            {
                { AnalyticsEvents.Params.ItemId, productId ?? string.Empty },
                { AnalyticsEvents.Params.Success, true }
            });

            return new IAPResult(true, productId);
#else
            GameLogger.Log($"[StubIAPService] IAP not available in production stub: {productId}");
            LogPurchaseFail(productId, "IAP service not configured for production");
            return new IAPResult(false, productId, "IAP service not configured for production");
#endif
        }

        private void LogPurchaseFail(string productId, string reason)
        {
            _analytics?.LogEvent(AnalyticsEvents.PurchaseFail, new Dictionary<string, object>
            {
                { AnalyticsEvents.Params.ProductId, productId ?? string.Empty },
                { AnalyticsEvents.Params.Success, false },
                { AnalyticsEvents.Params.Reason, reason ?? string.Empty }
            });
        }
    }
}
