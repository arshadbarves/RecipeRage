using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class MapRotationCalculator
    {
        private readonly IRemoteConfigService _configService;
        private readonly INTPTimeService _ntpTimeService;

        public MapRotationCalculator(IRemoteConfigService configService, INTPTimeService ntpTimeService)
        {
            _configService = configService;
            _ntpTimeService = ntpTimeService;
        }

        public TimeSpan GetTimeUntilRotationChange()
        {
            return TimeSpan.Zero;
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
