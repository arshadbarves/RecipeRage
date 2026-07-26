using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Application.Models
{
    [CreateAssetMenu(fileName = "ChefDatabase", menuName = "KitchenClash/Chef Database")]
    public class ChefDatabaseSO : ScriptableObject
    {
        public List<ChefDefinitionSO> Chefs = new();

        private Dictionary<ChefId, ChefDefinitionSO> _lookup;

        private void OnEnable() => _lookup = null;

        public void Initialize()
        {
            _lookup = Chefs
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToDictionary(c => c.Id);
        }

        public ChefDefinitionSO Get(ChefId id)
        {
            if (_lookup == null) Initialize();
            return _lookup.TryGetValue(id, out var chef) ? chef : null;
        }

        public IReadOnlyList<ChefDefinitionSO> GetAll() => Chefs;

        public List<ChefDefinition> ToDomainList()
        {
            return Chefs
                .Where(c => c != null)
                .Select(c => c.ToDomain())
                .ToList();
        }

        private void OnValidate()
        {
            var duplicates = Chefs
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                Debug.LogError($"Duplicate ChefIds: {string.Join(", ", duplicates.Select(g => g.Key))}", this);
            }
        }
    }
}
