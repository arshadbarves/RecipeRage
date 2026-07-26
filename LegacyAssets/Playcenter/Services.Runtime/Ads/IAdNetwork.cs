using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Ad-network adapter (e.g. AppLovin MAX). SDK keeps the gating logic.</summary>
    public interface IAdNetwork
    {
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }
        Task<bool> ShowInterstitialAsync();
        Task<AdRewardResult> ShowRewardedAsync(string placement);
    }
}
