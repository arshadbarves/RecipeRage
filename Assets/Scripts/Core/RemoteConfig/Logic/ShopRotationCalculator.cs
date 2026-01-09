using System;
using System.Collections.Generic;
using System.Linq;
using Core.Core.Logging;
using Core.Core.RemoteConfig.Interfaces;
using Core.Core.RemoteConfig.Models;
using Core.Core.RemoteConfig.Services;

namespace Core.Core.RemoteConfig.Logic
{
    /// <summary>
    /// Calculates current shop rotation based on NTP time
    /// </summary>
    public class ShopRotationCalculator
    {
        private readonly IRemoteConfigService _configService;
        private readonly INTPTimeService _ntpTimeService;

        public ShopRotationCalculator(
            IRemoteConfigService configService,
            INTPTimeService ntpTimeService)
        {
            _configService = configService;
            _ntpTimeService = ntpTimeService;
        }

        /// <summary>
        /// Gets the current rotation period
        /// </summary>
        public RotationPeriod GetCurrentRotation()
        {
            try
            {
                if (!_configService.TryGetConfig<ShopConfig>(out var shopConfig))
                {
                    GameLogger.LogWarning("ShopConfig not available");
                    return null;
                }

                if (shopConfig.RotationSchedule == null ||
                    shopConfig.RotationSchedule.RotationPeriods == null ||
                    shopConfig.RotationSchedule.RotationPeriods.Count == 0)
                {
                    GameLogger.LogWarning("No rotation schedule configured");
                    return null;
                }

                DateTime serverTime = NTPTime.UtcNow;

                // Find active rotation period
                foreach (var period in shopConfig.RotationSchedule.RotationPeriods)
                {
                    var startTime = period.GetStartTime();
                    var endTime = period.GetEndTime();

                    if (serverTime >= startTime && serverTime <= endTime)
                    {
                        GameLogger.Log($"Current rotation: {period.PeriodId}");
                        return period;
                    }
                }

                GameLogger.LogWarning("No active rotation period found");
                return null;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get current rotation: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets featured items for current rotation
        /// </summary>
        public List<ShopItem> GetFeaturedItems()
        {
            try
            {
                var currentRotation = GetCurrentRotation();

                if (currentRotation == null || currentRotation.FeaturedItemIds == null)
                {
                    return new List<ShopItem>();
                }

                if (!_configService.TryGetConfig<ShopConfig>(out var shopConfig))
                {
                    return new List<ShopItem>();
                }

                var featuredItems = new List<ShopItem>();

                // Find items by ID
                foreach (var category in shopConfig.Categories)
                {
                    foreach (var item in category.Items)
                    {
                        if (currentRotation.FeaturedItemIds.Contains(item.ItemId))
                        {
                            featuredItems.Add(item);
                        }
                    }
                }

                GameLogger.Log($"Found {featuredItems.Count} featured items");
                return featuredItems;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get featured items: {ex.Message}");
                return new List<ShopItem>();
            }
        }

        /// <summary>
        /// Gets active special offers
        /// </summary>
        public List<SpecialOffer> GetActiveSpecialOffers()
        {
            try
            {
                if (!_configService.TryGetConfig<ShopConfig>(out var shopConfig))
                {
                    return new List<SpecialOffer>();
                }

                if (shopConfig.SpecialOffers == null)
                {
                    return new List<SpecialOffer>();
                }

                DateTime serverTime = NTPTime.UtcNow;

                var activeOffers = shopConfig.SpecialOffers
                    .Where(offer => offer.IsActive(serverTime))
                    .ToList();

                GameLogger.Log($"Found {activeOffers.Count} active special offers");
                return activeOffers;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get active special offers: {ex.Message}");
                return new List<SpecialOffer>();
            }
        }

        /// <summary>
        /// Gets time until next rotation
        /// </summary>
        public TimeSpan GetTimeUntilNextRotation()
        {
            try
            {
                var currentRotation = GetCurrentRotation();

                if (currentRotation == null)
                {
                    return TimeSpan.Zero;
                }

                DateTime serverTime = NTPTime.UtcNow;

                var endTime = currentRotation.GetEndTime();
                var timeRemaining = endTime - serverTime;

                return timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get time until next rotation: {ex.Message}");
                return TimeSpan.Zero;
            }
        }
    }
}
