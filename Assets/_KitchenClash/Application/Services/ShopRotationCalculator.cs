using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class ShopRotationCalculator
    {
        private readonly IRemoteConfigService _configService;
        private readonly INTPTimeService _ntpTimeService;

        public ShopRotationCalculator(IRemoteConfigService configService, INTPTimeService ntpTimeService)
        {
            _configService = configService;
            _ntpTimeService = ntpTimeService;
        }

        public TimeSpan GetTimeUntilNextRotation()
        {
            return TimeSpan.Zero;
        }
    }
}
