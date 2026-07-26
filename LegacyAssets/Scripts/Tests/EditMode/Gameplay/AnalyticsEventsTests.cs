using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KitchenClash.Application.Config;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public sealed class AnalyticsEventsTests
    {
        [Test]
        public void FunnelEventConstants_AreNonEmptyAndUnique()
        {
            string[] required =
            {
                AnalyticsEvents.BootGateOffline,
                AnalyticsEvents.LoginSuccess,
                AnalyticsEvents.MatchStart,
                AnalyticsEvents.MatchEnd,
                AnalyticsEvents.WalletCredit,
                AnalyticsEvents.PurchaseSuccess,
                AnalyticsEvents.PurchaseFail
            };

            foreach (string name in required)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(name), "Event constant must be non-empty");
            }

            Assert.AreEqual(required.Length, required.Distinct().Count(),
                "Funnel event names must be unique");
        }

        [Test]
        public void AllEventConstants_AreUnique()
        {
            FieldInfo[] fields = typeof(AnalyticsEvents)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .ToArray();

            var values = new List<string>();
            foreach (FieldInfo field in fields)
            {
                values.Add((string)field.GetRawConstantValue());
            }

            Assert.AreEqual(values.Count, values.Distinct().Count(),
                "All AnalyticsEvents string constants must be unique");
        }

        [Test]
        public void MatchEnd_AndMatchComplete_AreDistinctForCompat()
        {
            Assert.AreEqual("match_end", AnalyticsEvents.MatchEnd);
            Assert.AreEqual("match_complete", AnalyticsEvents.MatchComplete);
            Assert.AreNotEqual(AnalyticsEvents.MatchEnd, AnalyticsEvents.MatchComplete);
        }
    }
}
