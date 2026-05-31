using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using KitchenClash.Domain.Events;
using KitchenClash.Application.Models;
using KitchenClash.Application.Models.RemoteConfigs;

namespace KitchenClash.Application.Services
{
    public class ChefRegistry
    {
        private readonly Dictionary<ChefId, ChefDefinition> _chefs;

        public ChefRegistry(ChefDatabaseSO database, IEventBus eventBus)
        {
            _chefs = database.ToDomainList().ToDictionary(c => c.Id);
            eventBus.Subscribe<ConfigUpdatedEvent>(OnConfigUpdated);
        }

        private void OnConfigUpdated(ConfigUpdatedEvent evt)
        {
            if (evt.Config is CharacterConfig characterConfig)
                ApplyRemoteConfig(characterConfig);
        }

        public ChefDefinition Get(ChefId id) => _chefs.TryGetValue(id, out ChefDefinition c) ? c : null;

        public IReadOnlyList<ChefDefinition> GetAll() => _chefs.Values.ToList();

        public IReadOnlyList<ChefDefinition> GetUnlockedChefs(int playerLevel, IEconomyService economy)
        {
            var result = new List<ChefDefinition>();
            foreach (ChefDefinition chef in _chefs.Values)
            {
                if (IsUnlocked(chef, playerLevel, economy))
                {
                    result.Add(chef);
                }
            }
            return result;
        }

        public bool IsUnlocked(ChefDefinition chef, int playerLevel, IEconomyService economy)
        {
            switch (chef.Unlock.Type)
            {
                case UnlockType.Starter:
                    return true;
                case UnlockType.Level:
                    return playerLevel >= chef.Unlock.Value;
                case UnlockType.Shop:
                    return economy != null && economy.HasItem($"chef_{chef.Id}");
                case UnlockType.Wins:
                case UnlockType.Trophies:
                case UnlockType.Matches:
                case UnlockType.BattlePass:
                    return economy != null && economy.HasItem($"chef_{chef.Id}");
                default:
                    return false;
            }
        }

        private void ApplyRemoteConfig(CharacterConfig config)
        {
            if (config?.Overrides == null) return;

            foreach (CharacterStatOverride ov in config.Overrides)
            {
                if (string.IsNullOrEmpty(ov.ChefId)) continue;
                if (!System.Enum.TryParse<ChefId>(ov.ChefId, true, out ChefId chefId)) continue;
                if (!_chefs.TryGetValue(chefId, out ChefDefinition chef)) continue;

                ChefStatBlock stats = chef.Stats;
                var newStats = new ChefStatBlock(
                    ov.SpeedMultiplier >= 0 ? ov.SpeedMultiplier : stats.MoveSpeed,
                    ov.CookingSpeedMultiplier >= 0 ? ov.CookingSpeedMultiplier : stats.CookSpeedMult,
                    ov.FireResistance >= 0 ? ov.FireResistance : stats.BurnResistance,
                    ov.CarryCapacity >= 0 ? ov.CarryCapacity : stats.CarryCapacity,
                    stats.InteractRange,
                    ov.ScoreMultiplier >= 0 ? ov.ScoreMultiplier : stats.ScoreMultiplier
                );

                _chefs[chefId] = new ChefDefinition(
                    chef.Id, chef.DisplayName, chef.Description, newStats,
                    chef.Unlock, chef.PassiveAbility, chef.ActiveAbility,
                    chef.SuperAbility, chef.GadgetAbility);

                GameLogger.Log($"[ChefRegistry] Applied remote overrides for {chefId}");
            }
        }
    }
}
