#if FIREBASE_ANALYTICS
using System.Collections.Generic;
using KitchenClash.Domain;
using Firebase.Analytics;

namespace KitchenClash.Infrastructure.Firebase
{
    /// <summary>
    /// GDD Section 16: Firebase Analytics event tracking.
    /// </summary>
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParams = new List<Parameter>();
            foreach (var kvp in parameters)
            {
                if (kvp.Value is int intVal)
                    firebaseParams.Add(new Parameter(kvp.Key, intVal));
                else if (kvp.Value is float floatVal)
                    firebaseParams.Add(new Parameter(kvp.Key, floatVal));
                else if (kvp.Value is double doubleVal)
                    firebaseParams.Add(new Parameter(kvp.Key, doubleVal));
                else if (kvp.Value is long longVal)
                    firebaseParams.Add(new Parameter(kvp.Key, longVal));
                else
                    firebaseParams.Add(new Parameter(kvp.Key, kvp.Value?.ToString() ?? ""));
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
        }

        public void SetUserProperty(string name, string value)
        {
            FirebaseAnalytics.SetUserProperty(name, value);
        }
    }
}
#else
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Firebase
{
    /// <summary>
    /// Stub Firebase Analytics service when Firebase is not available.
    /// </summary>
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            GameLogger.Log($"[Analytics] {eventName}");
        }

        public void SetUserProperty(string name, string value)
        {
            GameLogger.Log($"[Analytics] SetUserProperty: {name}={value}");
        }
    }
}
#endif
