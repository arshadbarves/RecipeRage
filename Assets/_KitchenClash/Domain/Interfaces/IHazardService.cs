using System;

namespace KitchenClash.Domain
{
    public interface IHazardService
    {
        void RegisterFire(string stationId);
        bool TryExtinguish(string stationId, float currentTime);
        float GetFireExtinguishWindow();
        int GetActiveFires();
        event Action<string> OnFireStarted;
        event Action<string> OnFireExtinguished;
        event Action<string> OnFirePenalty;
    }
}
