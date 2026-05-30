using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class HazardService : IHazardService
    {
        private readonly IConfigService _cfg;
        private readonly IEventBus _eventBus;
        private readonly Dictionary<string, float> _activeFires = new();

        public event Action<string> OnFireStarted;
        public event Action<string> OnFireExtinguished;
        public event Action<string> OnFirePenalty;

        private float _currentTime;

        public HazardService(IConfigService cfg, IEventBus eventBus)
        {
            _cfg = cfg;
            _eventBus = eventBus;
        }

        public void RegisterFire(string stationId)
        {
            _activeFires[stationId] = _currentTime;
            OnFireStarted?.Invoke(stationId);
            _eventBus?.Publish(new SFXEvent(SFXType.FireStart));
        }

        public bool TryExtinguish(string stationId, float currentTime)
        {
            if (!_activeFires.TryGetValue(stationId, out float startTime))
            {
                return false;
            }

            float window = GetFireExtinguishWindow();
            if (currentTime - startTime > window)
            {
                _activeFires.Remove(stationId);
                OnFirePenalty?.Invoke(stationId);
                return false;
            }

            _activeFires.Remove(stationId);
            OnFireExtinguished?.Invoke(stationId);
            _eventBus?.Publish(new SFXEvent(SFXType.FireExtinguish));
            return true;
        }

        public float GetFireExtinguishWindow() =>
            _cfg.Get("fire_extinguish_window_sec", 5f);

        public int GetActiveFires() => _activeFires.Count;

        /// <summary>
        /// Call each frame/tick to update internal time tracking.
        /// </summary>
        public void SetCurrentTime(float time) => _currentTime = time;

        /// <summary>
        /// Clears all active fires. Call on match end.
        /// </summary>
        public void ClearAll() => _activeFires.Clear();
    }
}
