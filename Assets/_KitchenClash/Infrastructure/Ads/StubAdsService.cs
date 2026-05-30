using System;
using System.Threading.Tasks;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Ads
{
    public sealed class StubAdsService : IAdsService
    {
        private readonly IConfigService _cfg;
        private bool _interstitialsDisabled;
        private DateTime _lastInterstitialUtc = DateTime.MinValue;

        public StubAdsService(IConfigService cfg)
        {
            _cfg = cfg;
        }

        public bool IsInterstitialReady => false;
        public bool IsRewardedReady => false;

        public Task<bool> ShowInterstitialAsync()
        {
            Debug.Log("[StubAds] ShowInterstitial — no SDK");
            return Task.FromResult(false);
        }

        public Task<AdRewardResult> ShowRewardedAsync(string placement)
        {
            Debug.Log($"[StubAds] ShowRewarded placement={placement} — no SDK");
            return Task.FromResult(new AdRewardResult(false, placement));
        }

        public bool ShouldShowInterstitial(int matchCount)
        {
            if (_interstitialsDisabled)
            {
                return false;
            }

            if (!_cfg.Get("ad_interstitial_enabled", true))
            {
                return false;
            }

            int frequency = _cfg.Get("ad_interstitial_frequency", 3);
            if (matchCount % frequency != 0)
            {
                return false;
            }

            int minGapSec = _cfg.Get("ad_interstitial_min_gap_sec", 180);
            if ((DateTime.UtcNow - _lastInterstitialUtc).TotalSeconds < minGapSec)
            {
                return false;
            }

            return true;
        }

        public void DisableInterstitials()
        {
            _interstitialsDisabled = true;
            Debug.Log("[StubAds] Interstitials disabled (Battle Pass owner)");
        }
    }
}
