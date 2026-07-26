using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application.Models
{
    [CreateAssetMenu(fileName = "MapDatabase", menuName = "KitchenClash/Map Database")]
    public class MapDatabaseSO : ScriptableObject
    {
        public List<MapDefinitionSO> Maps = new();

        private Dictionary<string, MapDefinitionSO> _lookup;

        private void OnEnable() => _lookup = null;

        public void Initialize()
        {
            _lookup = Maps
                .Where(m => m != null)
                .GroupBy(m => m.MapId)
                .Select(g => g.First())
                .ToDictionary(m => m.MapId);
        }

        public MapDefinitionSO Get(string mapId)
        {
            if (_lookup == null) Initialize();
            return _lookup.TryGetValue(mapId, out var map) ? map : null;
        }

        public IReadOnlyList<MapDefinitionSO> GetAll() => Maps;

        public List<MapDefinition> ToDomainList()
        {
            return Maps
                .Where(m => m != null)
                .Select(m => m.ToDomain())
                .ToList();
        }

        private void OnValidate()
        {
            var duplicates = Maps
                .Where(m => m != null)
                .GroupBy(m => m.MapId)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                Debug.LogError($"Duplicate MapIds: {string.Join(", ", duplicates.Select(g => g.Key))}", this);
            }
        }
    }
}
