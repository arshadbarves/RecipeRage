using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Default ad network when no vendor adapter is wired: nothing is ever ready/shown.</summary>
    public sealed class NullAdNetwork : IAdNetwork
    {
        public bool IsInterstitialReady => false;
        public bool IsRewardedReady => false;
        public Task<bool> ShowInterstitialAsync() => Task.FromResult(false);
        public Task<AdRewardResult> ShowRewardedAsync(string placement) => Task.FromResult(new AdRewardResult(false, placement));
    }
}
