using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserProperty(string name, string value);
        void TrackMatchStart(string mapId, string mode, string chefId, int trophyCount);
        void TrackMatchComplete(bool won, int score, float durationSec, string mapId);
        void TrackDishServed(string recipeId, float timeTaken, int comboCount);
        void TrackIapCompleted(string itemId, float usdValue);
        void TrackAdShown(string adType, string placement);
        void TrackDailyStreakClaimed(int dayNumber, string rewardType);
        void TrackConnectivityLost(string context);
        void TrackAuthCompleted(string method, bool wasGuest);
    }
}
