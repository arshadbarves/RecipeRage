using System;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Daily map rotation (Brawl Stars-style). Map of the day = dayIndex % mapCount,
    /// overridable via remote config key current_map for events.
    /// </summary>
    public sealed class MapRotationService
    {
        private readonly MapDefinition[] _maps;
        private readonly IConfigService _config;

        public MapRotationService(MapDefinition[] maps, IConfigService config)
        {
            _maps = maps;
            _config = config;
        }

        public MapDefinition CurrentMap
        {
            get
            {
                var forced = _config.Get("current_map", string.Empty);
                if (!string.IsNullOrEmpty(forced))
                {
                    foreach (var map in _maps)
                    {
                        if (map.Id == forced)
                        {
                            return map;
                        }
                    }
                }

                var dayIndex = (int)(DateTime.UtcNow - new DateTime(2026, 1, 1)).TotalDays;
                return _maps[dayIndex % _maps.Length];
            }
        }
    }
}
