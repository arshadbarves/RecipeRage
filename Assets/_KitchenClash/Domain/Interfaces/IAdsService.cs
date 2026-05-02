using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IAdsService
    {
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }
        Task<bool> ShowInterstitialAsync();
        Task<AdRewardResult> ShowRewardedAsync(string placement);
        bool ShouldShowInterstitial(int matchCount);
        void DisableInterstitials();
    }

    public sealed class AdRewardResult
    {
        public bool Granted { get; }
        public string Placement { get; }

        public AdRewardResult(bool granted, string placement)
        {
            Granted = granted;
            Placement = placement;
        }
    }
}
