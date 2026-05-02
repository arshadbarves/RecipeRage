using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IAdService
    {
        bool IsInterstitialEnabled { get; }
        bool IsRewardedEnabled { get; }
        bool CanShowInterstitial();
        Task ShowInterstitialAsync();
        Task<bool> ShowRewardedAsync();
        void OnMatchComplete(int matchCount);
    }
}
