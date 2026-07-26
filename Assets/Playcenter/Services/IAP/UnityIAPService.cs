using System;
using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>
    /// Unity IAP provider. Until store billing is wired (Slice 5), purchases
    /// immediately complete so the purchase flow is testable end-to-end.
    /// Product catalog lives in Slice 5.
    /// </summary>
    public sealed class UnityIAPService : IIAPService
    {
        private readonly ILoggingService _log;
        private readonly IAnalyticsService _analytics;

        public bool IsReady { get; private set; }
        public event Action<string> OnPurchaseCompleted;

        public UnityIAPService(ILoggingService log, IAnalyticsService analytics)
        {
            _log = log;
            _analytics = analytics;
        }

        public IEnumerator Initialize()
        {
            IsReady = true;
            _log.Log("[IAP] Initialized (stub mode, Unity IAP pending)");
            yield break;
        }

        public void Purchase(string productId)
        {
            _log.Log($"[IAP] Purchase requested: {productId} (stub — auto-complete)");
            _analytics.TrackEvent("iap_purchase", new Dictionary<string, object> { { "productId", productId } });
            OnPurchaseCompleted?.Invoke(productId);
        }

        public bool IsProductAvailable(string productId) => true;

        public string GetLocalizedPrice(string productId) => "$0.99";
    }
}
