using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IIAPService
    {
        Task<IAPResult> PurchaseAsync(string productId);
        bool IsInitialized { get; }
    }

    public sealed class IAPResult
    {
        public bool Success { get; }
        public string ProductId { get; }
        public string Error { get; }

        public IAPResult(bool success, string productId, string error = null)
        {
            Success = success;
            ProductId = productId;
            Error = error;
        }
    }
}
