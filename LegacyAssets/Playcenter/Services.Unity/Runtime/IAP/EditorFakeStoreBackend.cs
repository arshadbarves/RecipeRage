using System.Threading.Tasks;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>Editor/dev store: always succeeds so purchases are testable without a store SDK.</summary>
    public sealed class EditorFakeStoreBackend : IStoreBackend
    {
        public bool IsInitialized { get; private set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task<StorePurchaseResult> PurchaseAsync(string productId)
        {
            Debug.Log($"[EditorFakeStoreBackend] Simulating purchase: {productId}");
            return Task.FromResult(new StorePurchaseResult(true, productId));
        }
    }
}
