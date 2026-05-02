using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Analytics
{
    public class StubAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            Debug.Log($"[Analytics] {eventName}");
        }

        public void SetUserProperty(string name, string value)
        {
            Debug.Log($"[Analytics] UserProperty {name}={value}");
        }

        public void TrackMatchStart(string mapId, string mode, string chefId, int trophyCount)
        {
            Debug.Log($"[Analytics] MatchStart map={mapId} mode={mode} chef={chefId} trophies={trophyCount}");
        }

        public void TrackMatchComplete(bool won, int score, float durationSec, string mapId)
        {
            Debug.Log($"[Analytics] MatchComplete won={won} score={score} duration={durationSec:F1}s map={mapId}");
        }

        public void TrackDishServed(string recipeId, float timeTaken, int comboCount)
        {
            Debug.Log($"[Analytics] DishServed recipe={recipeId} time={timeTaken:F1}s combo={comboCount}");
        }

        public void TrackIapCompleted(string itemId, float usdValue)
        {
            Debug.Log($"[Analytics] IapCompleted item={itemId} usd={usdValue:F2}");
        }

        public void TrackAdShown(string adType, string placement)
        {
            Debug.Log($"[Analytics] AdShown type={adType} placement={placement}");
        }

        public void TrackDailyStreakClaimed(int dayNumber, string rewardType)
        {
            Debug.Log($"[Analytics] DailyStreakClaimed day={dayNumber} reward={rewardType}");
        }

        public void TrackConnectivityLost(string context)
        {
            Debug.Log($"[Analytics] ConnectivityLost context={context}");
        }

        public void TrackAuthCompleted(string method, bool wasGuest)
        {
            Debug.Log($"[Analytics] AuthCompleted method={method} wasGuest={wasGuest}");
        }
    }
}
