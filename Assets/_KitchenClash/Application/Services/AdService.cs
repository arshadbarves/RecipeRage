using System;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class AdService : IAdService
    {
        private readonly IConfigService _cfg;
        private int _matchesSinceLastAd;
        private DateTime _lastAdTime = DateTime.MinValue;

        public AdService(IConfigService cfg) => _cfg = cfg;

        public bool IsInterstitialEnabled => _cfg.Get("ad_interstitial_enabled", true);
        public bool IsRewardedEnabled => _cfg.Get("ad_rewarded_enabled", true);

        public bool CanShowInterstitial()
        {
            if (!IsInterstitialEnabled) return false;
            int freq = _cfg.Get("ad_interstitial_frequency", 3);
            int minGap = _cfg.Get("ad_interstitial_min_gap_sec", 180);
            return _matchesSinceLastAd >= freq
                && (DateTime.UtcNow - _lastAdTime).TotalSeconds >= minGap;
        }

        public Task ShowInterstitialAsync()
        {
            // Stub: integrate with ad SDK
            _matchesSinceLastAd = 0;
            _lastAdTime = DateTime.UtcNow;
            GameLogger.Log("[AdService] Interstitial shown (stub)");
            return Task.CompletedTask;
        }

        public Task<bool> ShowRewardedAsync()
        {
            // Stub: integrate with ad SDK
            GameLogger.Log("[AdService] Rewarded ad shown (stub)");
            return Task.FromResult(true);
        }

        public void OnMatchComplete(int matchCount)
        {
            _matchesSinceLastAd++;
        }
    }
}
