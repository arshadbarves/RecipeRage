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

        public void TrackMatchStart(string mapId, string mode, string chefId, int trophyCount)
            => LogEvent("match_start", new Dictionary<string, object> { {"map_id",mapId}, {"mode",mode}, {"chef_id",chefId}, {"trophy_count",trophyCount} });
        public void TrackMatchComplete(bool won, int score, float durationSec, string mapId)
            => LogEvent("match_complete", new Dictionary<string, object> { {"won",won}, {"score",score}, {"duration_sec",durationSec}, {"map_id",mapId} });
        public void TrackDishServed(string recipeId, float timeTaken, int comboCount)
            => LogEvent("dish_served", new Dictionary<string, object> { {"recipe_id",recipeId}, {"time_taken",timeTaken}, {"combo_count",comboCount} });
        public void TrackIapCompleted(string itemId, float usdValue)
            => LogEvent("iap_completed", new Dictionary<string, object> { {"item_id",itemId}, {"usd_value",usdValue} });
        public void TrackAdShown(string adType, string placement)
            => LogEvent("ad_shown", new Dictionary<string, object> { {"ad_type",adType}, {"placement",placement} });
        public void TrackDailyStreakClaimed(int dayNumber, string rewardType)
            => LogEvent("daily_streak_claimed", new Dictionary<string, object> { {"day_number",dayNumber}, {"reward_type",rewardType} });
        public void TrackConnectivityLost(string context)
            => LogEvent("connectivity_lost", new Dictionary<string, object> { {"context",context} });
        public void TrackAuthCompleted(string method, bool wasGuest)
            => LogEvent("auth_completed", new Dictionary<string, object> { {"method",method}, {"was_guest",wasGuest} });
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

        public void TrackMatchStart(string mapId, string mode, string chefId, int trophyCount)
            => LogEvent("match_start");
        public void TrackMatchComplete(bool won, int score, float durationSec, string mapId)
            => LogEvent("match_complete");
        public void TrackDishServed(string recipeId, float timeTaken, int comboCount)
            => LogEvent("dish_served");
        public void TrackIapCompleted(string itemId, float usdValue)
            => LogEvent("iap_completed");
        public void TrackAdShown(string adType, string placement)
            => LogEvent("ad_shown");
        public void TrackDailyStreakClaimed(int dayNumber, string rewardType)
            => LogEvent("daily_streak_claimed");
        public void TrackConnectivityLost(string context)
            => LogEvent("connectivity_lost");
        public void TrackAuthCompleted(string method, bool wasGuest)
            => LogEvent("auth_completed");
    }
}
#endif
