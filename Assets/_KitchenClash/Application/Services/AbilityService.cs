using System;
using System.Collections.Generic;
using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class AbilityService : IAbilityService
    {
        private readonly IConfigService _cfg;
        private readonly ChefRegistry _chefRegistry;
        private readonly AbilityFactory _abilityFactory;
        private readonly Dictionary<ChefId, Dictionary<AbilitySlot, IAbility>> _abilities = new();
        private readonly Dictionary<ChefId, Dictionary<AbilitySlot, float>> _cooldowns = new();
        private readonly Dictionary<ChefId, Dictionary<AbilitySlot, float>> _activeDurations = new();
        private readonly Dictionary<ChefId, int> _superCharges = new();

        public AbilityService(IConfigService cfg, ChefRegistry chefRegistry, AbilityFactory abilityFactory)
        {
            _cfg = cfg;
            _chefRegistry = chefRegistry;
            _abilityFactory = abilityFactory;
        }

        /// <summary>
        /// Looks up the chef's passive + active abilities from ChefRegistry,
        /// creates concrete IAbility instances via AbilityFactory, and registers them.
        /// </summary>
        public void RegisterChefAbilities(ChefId chefId)
        {
            ChefDefinition def = _chefRegistry.Get(chefId);
            if (def == null)
            {
                return;
            }

            IAbility passive = _abilityFactory.CreateAbility(def.PassiveAbility);
            if (passive != null)
            {
                RegisterAbility(chefId, passive);
            }

            IAbility active = _abilityFactory.CreateAbility(def.ActiveAbility);
            if (active != null)
            {
                RegisterAbility(chefId, active);
            }
        }

        public void RegisterAbility(ChefId chef, IAbility ability)
        {
            if (!_abilities.ContainsKey(chef))
            {
                _abilities[chef] = new Dictionary<AbilitySlot, IAbility>();
            }

            _abilities[chef][ability.Slot] = ability;
        }

        public AbilityResult TryActivate(AbilitySlot slot, ChefId chef, AbilityContext ctx)
        {
            if (!_abilities.TryGetValue(chef, out Dictionary<AbilitySlot, IAbility> slots) || !slots.TryGetValue(slot, out IAbility ability))
            {
                return AbilityResult.Failed;
            }

            if (GetCooldownRemaining(chef, slot) > 0)
            {
                return AbilityResult.Failed;
            }

            if (slot == AbilitySlot.Super && (_superCharges.GetValueOrDefault(chef) < 3))
            {
                return AbilityResult.Failed;
            }

            if (!ability.CanActivate(ctx))
            {
                return AbilityResult.Failed;
            }

            AbilityResult result = ability.Activate(ctx);

            if (result.Success)
            {
                SetCooldown(chef, slot, ability.Cooldown);

                // Track active duration for auto-deactivation
                if (result.Duration > 0f)
                {
                    SetActiveDuration(chef, slot, result.Duration);
                }

                if (slot == AbilitySlot.Super)
                {
                    _superCharges[chef] = 0;
                }
            }

            return result;
        }

        public void ChargeSuper(ChefId chef, int dishesServed)
        {
            _superCharges[chef] = _superCharges.GetValueOrDefault(chef) + 1;
        }

        public int GetSuperCharges(ChefId chef) => _superCharges.GetValueOrDefault(chef);

        public bool IsSuperReady(ChefId chef) => _superCharges.GetValueOrDefault(chef) >= 3;

        public float GetCooldownRemaining(ChefId chef, AbilitySlot slot)
        {
            if (!_cooldowns.TryGetValue(chef, out Dictionary<AbilitySlot, float> slots) || !slots.TryGetValue(slot, out float remaining))
            {
                return 0f;
            }

            return Math.Max(0f, remaining);
        }

        /// <summary>
        /// Returns the passive ability definition for a chef, or null if none registered.
        /// Useful for gameplay systems that need to read passive values (e.g., speed multiplier).
        /// </summary>
        public AbilityDefinition GetPassiveDefinition(ChefId chef)
        {
            if (_abilities.TryGetValue(chef, out Dictionary<AbilitySlot, IAbility> slots) &&
                slots.TryGetValue(AbilitySlot.Passive, out IAbility ability) &&
                ability is PassiveAbility passive)
            {
                return passive.Definition;
            }
            return null;
        }

        /// <summary>
        /// Tick cooldowns and active durations. Call each frame with Time.deltaTime.
        /// Auto-deactivates abilities when their duration expires.
        /// </summary>
        public void UpdateCooldowns(float deltaTime)
        {
            // Tick cooldowns
            foreach (KeyValuePair<ChefId, Dictionary<AbilitySlot, float>> chef in _cooldowns)
            {
                var keys = new List<AbilitySlot>(chef.Value.Keys);
                foreach (AbilitySlot slot in keys)
                {
                    chef.Value[slot] -= deltaTime;
                    if (chef.Value[slot] <= 0)
                    {
                        chef.Value.Remove(slot);
                    }
                }
            }

            // Tick active durations and auto-deactivate
            foreach (KeyValuePair<ChefId, Dictionary<AbilitySlot, float>> chef in _activeDurations)
            {
                var keys = new List<AbilitySlot>(chef.Value.Keys);
                foreach (AbilitySlot slot in keys)
                {
                    chef.Value[slot] -= deltaTime;
                    if (chef.Value[slot] <= 0)
                    {
                        chef.Value.Remove(slot);
                        // Auto-deactivate the ability
                        if (_abilities.TryGetValue(chef.Key, out Dictionary<AbilitySlot, IAbility> slots) &&
                            slots.TryGetValue(slot, out IAbility ability))
                        {
                            ability.Deactivate();
                        }
                    }
                }
            }
        }

        /// <summary>Removes all registered abilities and resets state for all chefs.</summary>
        public void ClearAll()
        {
            _abilities.Clear();
            _cooldowns.Clear();
            _activeDurations.Clear();
            _superCharges.Clear();
        }

        private void SetCooldown(ChefId chef, AbilitySlot slot, float cooldown)
        {
            if (!_cooldowns.ContainsKey(chef))
            {
                _cooldowns[chef] = new Dictionary<AbilitySlot, float>();
            }

            _cooldowns[chef][slot] = cooldown;
        }

        private void SetActiveDuration(ChefId chef, AbilitySlot slot, float duration)
        {
            if (!_activeDurations.ContainsKey(chef))
            {
                _activeDurations[chef] = new Dictionary<AbilitySlot, float>();
            }

            _activeDurations[chef][slot] = duration;
        }
    }
}
