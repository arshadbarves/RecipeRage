using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using KitchenClash.Application.Models;
using KitchenClash.Application.Models.RemoteConfigs;

namespace KitchenClash.Application.Services
{
    public sealed class MapRegistry
    {
        private readonly Dictionary<string, MapDefinition> _maps = new();

        public MapRegistry(MapDatabaseSO database)
        {
            foreach (MapDefinition map in database.ToDomainList())
            {
                _maps[map.MapId] = map;
            }
        }

        public MapDefinition Get(string mapId)
        {
            return _maps.TryGetValue(mapId, out MapDefinition def) ? def : null;
        }

        public IReadOnlyList<MapDefinition> GetAll()
        {
            return _maps.Values.ToList();
        }

        public IReadOnlyList<MapDefinition> GetByKitchenTheme(string kitchenTheme)
        {
            return _maps.Values.Where(m => m.KitchenTheme == kitchenTheme).ToList();
        }

        public int Count => _maps.Count;

        public void ApplyRemoteConfig(MapConfig config)
        {
            if (config?.Overrides == null) return;

            foreach (MapOverride ov in config.Overrides)
            {
                if (string.IsNullOrEmpty(ov.MapId)) continue;
                if (!_maps.TryGetValue(ov.MapId, out MapDefinition map)) continue;

                if (ov.FireChanceMultiplier >= 0)
                    map.Hazards.FireChanceMultiplier = ov.FireChanceMultiplier;

                if (ov.HasSpecialHazards.HasValue)
                    map.Hazards.HasSpecialHazards = ov.HasSpecialHazards.Value;

                GameLogger.Log($"[MapRegistry] Applied remote overrides for {ov.MapId}");
            }
        }
    }
}
