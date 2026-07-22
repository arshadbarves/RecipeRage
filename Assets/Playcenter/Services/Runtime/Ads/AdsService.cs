using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Shared ads: owns interstitial frequency/min-gap/disable gating; renders via <see cref="IAdNetwork"/>.</summary>
    public sealed class AdsService : IAdsService
    {
        private readonly IAdNetwork _network;
        private readonly IConfigService _cfg;
        private bool _interstitialsDisabled;
        private DateTime _lastInterstitialUtc = DateTime.MinValue;

        public AdsService(IAdNetwork network, IConfigService cfg)
        {
            _network = network;
            _cfg = cfg;
        }

        public bool IsInterstitialReady => _network != null && _network.IsInterstitialReady;
        public bool IsRewardedReady => _network != null && _network.IsRewardedReady;

        public async Task<bool> ShowInterstitialAsync()
        {
            if (_network == null)
            {
                return false;
            }
            bool shown = await _network.ShowInterstitialAsync();
            if (shown)
            {
                _lastInterstitialUtc = DateTime.UtcNow;
            }
            return shown;
        }

        public Task<AdRewardResult> ShowRewardedAsync(string placement)
        {
            if (_network == null)
            {
                return Task.FromResult(new AdRewardResult(false, placement));
            }
            return _network.ShowRewardedAsync(placement);
        }

        public bool ShouldShowInterstitial(int matchCount)
        {
            if (_interstitialsDisabled)
            {
                return false;
            }
            if (_cfg != null && !_cfg.Get("ad_interstitial_enabled", true))
            {
                return false;
            }
            int frequency = _cfg != null ? _cfg.Get("ad_interstitial_frequency", 3) : 3;
            if (frequency <= 0)
            {
                frequency = 1;
            }
            if (matchCount % frequency != 0)
            {
                return false;
            }
            int minGapSec = _cfg != null ? _cfg.Get("ad_interstitial_min_gap_sec", 180) : 180;
            if ((DateTime.UtcNow - _lastInterstitialUtc).TotalSeconds < minGapSec)
            {
                return false;
            }
            return true;
        }

        public void DisableInterstitials()
        {
            _interstitialsDisabled = true;
        }
    }
}
