using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public interface IForceUpdatePolicy
    {
        Task<ForceUpdateDecision> EvaluateAsync(CancellationToken ct);
    }

    public readonly struct ForceUpdateDecision
    {
        public bool Required { get; }
        public string Message { get; }
        public string StoreUrl { get; }

        public ForceUpdateDecision(bool required, string message, string storeUrl)
        {
            Required = required;
            Message = message;
            StoreUrl = storeUrl;
        }
    }
}
