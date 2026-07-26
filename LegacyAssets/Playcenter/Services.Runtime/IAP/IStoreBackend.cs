using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Store adapter (e.g. Unity IAP). SDK owns the purchase flow.</summary>
    public interface IStoreBackend
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        Task<StorePurchaseResult> PurchaseAsync(string productId);
    }

    public sealed class StorePurchaseResult
    {
        public bool Success { get; }
        public string ProductId { get; }
        public string Error { get; }

        public StorePurchaseResult(bool success, string productId, string error = null)
        {
            Success = success;
            ProductId = productId;
            Error = error;
        }
    }
}
