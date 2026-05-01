using System;

namespace KitchenClash.Application.Services
{
    public static class NTPTime
    {
        private static INTPTimeService _instance;

        public static DateTime UtcNow => _instance?.IsSynced == true
            ? _instance.GetServerTime()
            : DateTime.UtcNow;

        public static bool IsSynced => _instance?.IsSynced ?? false;
        public static TimeSpan TimeOffset => _instance?.GetTimeOffset() ?? TimeSpan.Zero;

        public static void SetInstance(INTPTimeService service) => _instance = service;
        public static void ClearInstance() => _instance = null;
    }
}
