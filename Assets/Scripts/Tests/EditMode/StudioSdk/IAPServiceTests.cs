using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class IAPServiceTests
    {
        private sealed class FakeStore : IStoreBackend
        {
            public bool InitResult = true;
            public StorePurchaseResult Next = new StorePurchaseResult(true, "x");
            public bool IsInitialized { get; private set; }
            public Task InitializeAsync() { IsInitialized = InitResult; return Task.CompletedTask; }
            public Task<StorePurchaseResult> PurchaseAsync(string id) => Task.FromResult(Next);
        }

        private sealed class SpyGrantor : IIapRewardGrantor
        {
            public List<string> Granted = new();
            public Task GrantAsync(string productId) { Granted.Add(productId); return Task.CompletedTask; }
        }

        private sealed class SpyAnalytics : IAnalyticsService
        {
            public List<string> Events = new();
            public void LogEvent(string e, Dictionary<string, object> p = null) => Events.Add(e);
            public void SetUserProperty(string n, string v) { }
        }

        [Test]
        public async Task Purchase_Success_GrantsReward_AndLogsSuccess()
        {
            var store = new FakeStore { Next = new StorePurchaseResult(true, "gem_pack_s") };
            var grantor = new SpyGrantor();
            var analytics = new SpyAnalytics();
            var svc = new IAPService(store, grantor, analytics);
            var result = await svc.PurchaseAsync("gem_pack_s");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(new[] { "gem_pack_s" }, grantor.Granted.ToArray());
            Assert.Contains("iap_purchase_success", analytics.Events);
        }

        [Test]
        public async Task Purchase_StoreFails_DoesNotGrant_AndLogsFail()
        {
            var store = new FakeStore { Next = new StorePurchaseResult(false, "x", "declined") };
            var grantor = new SpyGrantor();
            var analytics = new SpyAnalytics();
            var svc = new IAPService(store, grantor, analytics);
            var result = await svc.PurchaseAsync("x");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("declined", result.Error);
            Assert.IsEmpty(grantor.Granted);
            Assert.Contains("iap_purchase_fail", analytics.Events);
        }

        [Test]
        public async Task Purchase_InitializesStoreOnce()
        {
            var store = new FakeStore();
            var svc = new IAPService(store, new SpyGrantor());
            await svc.PurchaseAsync("a");
            Assert.IsTrue(store.IsInitialized);
            Assert.IsTrue(svc.IsInitialized);
        }
    }
}
