using System.Threading.Tasks;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>Unity IAP store backend. Without UNITY_IAP, reports uninitialized (game falls back to editor fake).</summary>
    public sealed class UnityIapStoreBackend : IStoreBackend
    {
        public bool IsInitialized { get; private set; }

        public Task InitializeAsync()
        {
#if UNITY_IAP
            // TODO(wire): UnityEngine.Purchasing.StandardPurchasingModule + ConfigurationBuilder.Initialize.
            IsInitialized = true;
#else
            Debug.Log("[UnityIapStoreBackend] Unity IAP not integrated");
            IsInitialized = false;
#endif
            return Task.CompletedTask;
        }

        public Task<StorePurchaseResult> PurchaseAsync(string productId)
        {
#if UNITY_IAP
            // TODO(wire): IStoreController.InitiatePurchase; complete on ProcessPurchase.
            return Task.FromResult(new StorePurchaseResult(true, productId));
#else
            return Task.FromResult(new StorePurchaseResult(false, productId, "Unity IAP not integrated"));
#endif
        }
    }
}
