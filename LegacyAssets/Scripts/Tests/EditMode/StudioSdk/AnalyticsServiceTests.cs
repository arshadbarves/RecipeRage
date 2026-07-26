using System.Collections.Generic;
using NUnit.Framework;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.StudioSdk
{
    public sealed class AnalyticsServiceTests
    {
        private sealed class SpySink : IAnalyticsSink
        {
            public List<(string name, Dictionary<string, object> ps)> Events = new();
            public List<(string n, string v)> Props = new();
            public void LogEvent(string e, Dictionary<string, object> p) => Events.Add((e, p));
            public void SetUserProperty(string n, string v) => Props.Add((n, v));
        }

        [Test]
        public void LogEvent_ForwardsToSink_WithSameParameters()
        {
            var sink = new SpySink();
            var svc = new AnalyticsService(sink);
            var ps = new Dictionary<string, object> { { "k", 1 } };
            svc.LogEvent("test_event", ps);
            Assert.AreEqual(1, sink.Events.Count);
            Assert.AreEqual("test_event", sink.Events[0].name);
            Assert.AreSame(ps, sink.Events[0].ps);
        }

        [Test]
        public void LogEvent_NullParameters_ForwardsEmptyDictionaryNotNull()
        {
            var sink = new SpySink();
            var svc = new AnalyticsService(sink);
            svc.LogEvent("e", null);
            Assert.IsNotNull(sink.Events[0].ps);
            Assert.AreEqual(0, sink.Events[0].ps.Count);
        }

        [Test]
        public void SetUserProperty_ForwardsToSink()
        {
            var sink = new SpySink();
            var svc = new AnalyticsService(sink);
            svc.SetUserProperty("level", "3");
            Assert.AreEqual(("level", "3"), sink.Props[0]);
        }

        [Test]
        public void LogEvent_NullSink_DoesNotThrow()
        {
            var svc = new AnalyticsService(null);
            Assert.DoesNotThrow(() => svc.LogEvent("e"));
            Assert.DoesNotThrow(() => svc.SetUserProperty("a", "b"));
        }
    }
}
