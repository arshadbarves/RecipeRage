using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Shared IAP flow: init store → purchase → grant reward → log outcome. Never throws into gameplay.</summary>
    public sealed class IAPService : IIAPService
    {
        private readonly IStoreBackend _store;
        private readonly IIapRewardGrantor _grantor;
        private readonly IAnalyticsService _analytics;
        private bool _initAttempted;

        public bool IsInitialized => _store != null && _store.IsInitialized;

        public IAPService(IStoreBackend store, IIapRewardGrantor grantor, IAnalyticsService analytics = null)
        {
            _store = store;
            _grantor = grantor;
            _analytics = analytics;
        }

        public async Task<IAPResult> PurchaseAsync(string productId)
        {
            if (_store == null)
            {
                return new IAPResult(false, productId, "no store backend");
            }

            if (!_initAttempted)
            {
                _initAttempted = true;
                await _store.InitializeAsync();
            }

            if (!_store.IsInitialized)
            {
                Log("iap_purchase_fail", productId, false, "store not initialized");
                return new IAPResult(false, productId, "store not initialized");
            }

            StorePurchaseResult storeResult = await _store.PurchaseAsync(productId);
            if (!storeResult.Success)
            {
                Log("iap_purchase_fail", productId, false, storeResult.Error);
                return new IAPResult(false, productId, storeResult.Error);
            }

            if (_grantor != null)
            {
                await _grantor.GrantAsync(productId);
            }

            Log("iap_purchase_success", productId, true, null);
            return new IAPResult(true, productId);
        }

        private void Log(string eventName, string productId, bool success, string reason)
        {
            _analytics?.LogEvent(eventName, new Dictionary<string, object>
            {
                { "product_id", productId ?? string.Empty },
                { "success", success },
                { "reason", reason ?? string.Empty }
            });
        }
    }
}
