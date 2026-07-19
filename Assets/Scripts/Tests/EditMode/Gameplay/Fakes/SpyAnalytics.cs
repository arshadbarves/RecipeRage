using System.Collections.Generic;
using Playcenter.Services;

namespace RecipeRage.Tests.EditMode.Gameplay.Fakes
{
    public sealed class SpyAnalytics : IAnalyticsService
    {
        public List<string> Events { get; } = new();
        public List<Dictionary<string, object>> Parameters { get; } = new();

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            Events.Add(eventName);
            Parameters.Add(parameters);
        }

        public void SetUserProperty(string name, string value)
        {
        }
    }
}
