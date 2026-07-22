using System.Threading.Tasks;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>AppLovin MAX ad network. No MAX SDK → nothing is ready and shows log only.</summary>
    public sealed class MaxAdNetwork : IAdNetwork
    {
        public bool IsInterstitialReady
        {
            get
            {
#if APPLOVIN_MAX
                return MaxSdk.IsInterstitialReady(AdUnitIds.Interstitial);
#else
                return false;
#endif
            }
        }

        public bool IsRewardedReady
        {
            get
            {
#if APPLOVIN_MAX
                return MaxSdk.IsRewardedAdReady(AdUnitIds.Rewarded);
#else
                return false;
#endif
            }
        }

        public Task<bool> ShowInterstitialAsync()
        {
#if APPLOVIN_MAX
            // TODO(wire): subscribe MaxSdkCallbacks.Interstitial.OnAdHiddenEvent to complete the task.
            MaxSdk.ShowInterstitial(AdUnitIds.Interstitial);
            return Task.FromResult(true);
#else
            Debug.Log("[MaxAdNetwork] ShowInterstitial — AppLovin MAX not integrated");
            return Task.FromResult(false);
#endif
        }

        public Task<AdRewardResult> ShowRewardedAsync(string placement)
        {
#if APPLOVIN_MAX
            // TODO(wire): subscribe MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent.
            MaxSdk.ShowRewardedAd(AdUnitIds.Rewarded);
            return Task.FromResult(new AdRewardResult(true, placement));
#else
            Debug.Log($"[MaxAdNetwork] ShowRewarded placement={placement} — AppLovin MAX not integrated");
            return Task.FromResult(new AdRewardResult(false, placement));
#endif
        }
    }
}
