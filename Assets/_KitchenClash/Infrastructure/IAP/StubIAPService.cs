using System;
using System.Threading.Tasks;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.IAP
{
    /// <summary>
    /// Stub IAP service: succeeds in editor, fails in production builds.
    /// Delivers gems via IEconomyService on successful purchase.
    /// </summary>
    public sealed class StubIAPService : IIAPService
    {
        private readonly IEconomyService _economy;

        public bool IsInitialized => true;
        public event Action<string> OnPurchaseCompleted;

        public StubIAPService(IEconomyService economy)
        {
            _economy = economy;
        }

        public async Task<IAPResult> PurchaseAsync(string productId)
        {
            await Task.Yield();

#if UNITY_EDITOR
            GameLogger.Log($"[StubIAPService] Simulating purchase: {productId}");

            var item = IAPCatalog.GetById(productId);
            if (item == null)
                return new IAPResult(false, productId, "Product not found in catalog");

            if (item.Gems > 0 && _economy != null)
            {
                _economy.AddGems(item.Gems);
                GameLogger.Log($"[StubIAPService] Delivered {item.Gems} gems for {productId}");
            }

            OnPurchaseCompleted?.Invoke(productId);
            return new IAPResult(true, productId);
#else
            GameLogger.Log($"[StubIAPService] IAP not available in production stub: {productId}");
            return new IAPResult(false, productId, "IAP service not configured for production");
#endif
        }
    }
}
