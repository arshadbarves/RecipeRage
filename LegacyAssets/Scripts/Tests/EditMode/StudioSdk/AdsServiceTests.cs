using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class AdsServiceTests
    {
        private sealed class FakeNetwork : IAdNetwork
        {
            public bool InterstitialShown;
            public bool IsInterstitialReady => true;
            public bool IsRewardedReady => true;
            public Task<bool> ShowInterstitialAsync() { InterstitialShown = true; return Task.FromResult(true); }
            public Task<AdRewardResult> ShowRewardedAsync(string p) => Task.FromResult(new AdRewardResult(true, p));
        }

        private sealed class StubConfig : IConfigService
        {
            public System.Func<string, int> IntValue = key => 0;
            public System.Func<string, bool> BoolValue = key => true;
            public T Get<T>(string key, T fallback)
            {
                if (typeof(T) == typeof(int)) return (T)(object)IntValue(key);
                if (typeof(T) == typeof(bool)) return (T)(object)BoolValue(key);
                return fallback;
            }
            public Task FetchAsync() => Task.FromResult(true);
        }

        [Test]
        public void ShouldShowInterstitial_WhenDisabled_ReturnsFalse()
        {
            var svc = new AdsService(new FakeNetwork(), new StubConfig());
            svc.DisableInterstitials();
            Assert.IsFalse(svc.ShouldShowInterstitial(3));
        }

        [Test]
        public void ShouldShowInterstitial_WhenConfigDisabled_ReturnsFalse()
        {
            var cfg = new StubConfig { BoolValue = key => false };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsFalse(svc.ShouldShowInterstitial(3));
        }

        [Test]
        public void ShouldShowInterstitial_NotOnFrequencyBoundary_ReturnsFalse()
        {
            var cfg = new StubConfig { IntValue = key => key == "ad_interstitial_frequency" ? 3 : 0 };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsFalse(svc.ShouldShowInterstitial(4));
        }

        [Test]
        public void ShouldShowInterstitial_OnBoundaryAndGapElapsed_ReturnsTrue()
        {
            var cfg = new StubConfig { IntValue = key => key == "ad_interstitial_frequency" ? 3 : 0 };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsTrue(svc.ShouldShowInterstitial(3));
        }

        [Test]
        public async Task ShowInterstitial_RecordsShownTime_SoImmediateNextIsGated()
        {
            var cfg = new StubConfig { IntValue = key => key == "ad_interstitial_frequency" ? 1 : key == "ad_interstitial_min_gap_sec" ? 180 : 0 };
            var svc = new AdsService(new FakeNetwork(), cfg);
            Assert.IsTrue(svc.ShouldShowInterstitial(1));
            await svc.ShowInterstitialAsync();
            Assert.IsFalse(svc.ShouldShowInterstitial(2), "min-gap should block an immediate second interstitial");
        }
    }
}
