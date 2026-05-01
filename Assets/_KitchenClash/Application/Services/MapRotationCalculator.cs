using System;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class MapRotationCalculator
    {
        private readonly IRemoteConfigService _configService;
        private readonly INTPTimeService _ntpTimeService;
        private readonly MapRegistry _mapRegistry;

        public MapRotationCalculator(IRemoteConfigService configService, INTPTimeService ntpTimeService, MapRegistry mapRegistry)
        {
            _configService = configService;
            _ntpTimeService = ntpTimeService;
            _mapRegistry = mapRegistry;
        }

        public TimeSpan GetTimeUntilRotationChange()
        {
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Returns the full MapDefinition for the current map of the given queue.
        /// </summary>
        public MapDefinition GetCurrentMapDefinition(string queueId)
        {
            string mapId = GetCurrentMap(queueId);
            return _mapRegistry.Get(mapId);
        }

        public string GetCurrentMap(string queueId)
        {
            // Default scene per queue; rotation logic can be expanded later
            return queueId switch
            {
                "quick_2v2" => "sushi_shuffle",
                "quick_3v3" => "pirate_pot",
                "ranked" => "burger_boulevard",
                "event" => "haunted_kitchen",
                _ => "sushi_shuffle"
            };
        }
    }
}
