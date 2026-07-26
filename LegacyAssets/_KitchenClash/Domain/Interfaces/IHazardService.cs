namespace KitchenClash.Domain
{
    public interface IHazardService
    {
        void RegisterFire(string stationId);
        bool TryExtinguish(string stationId, float currentTime);
        float GetFireExtinguishWindow();
        int GetActiveFires();
        void SetCurrentTime(float time);
        void ClearAll();
    }
}
