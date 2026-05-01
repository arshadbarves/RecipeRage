using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Domain.Interfaces;

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
    }
}
