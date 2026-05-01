using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class AbilityService : IAbilityService
    {
        private readonly IConfigService _cfg;
        private readonly Dictionary<ChefId, Dictionary<AbilitySlot, IAbility>> _abilities = new();
        private readonly Dictionary<ChefId, Dictionary<AbilitySlot, float>> _cooldowns = new();
        private readonly Dictionary<ChefId, int> _superCharges = new();

        public AbilityService(IConfigService cfg) => _cfg = cfg;

        public void RegisterAbility(ChefId chef, IAbility ability)
        {
            if (!_abilities.ContainsKey(chef))
                _abilities[chef] = new Dictionary<AbilitySlot, IAbility>();

            _abilities[chef][ability.Slot] = ability;
        }

        public AbilityResult TryActivate(AbilitySlot slot, ChefId chef, AbilityContext ctx)
        {
            if (!_abilities.TryGetValue(chef, out var slots) || !slots.TryGetValue(slot, out var ability))
                return AbilityResult.Failed;

            if (GetCooldownRemaining(chef, slot) > 0)
                return AbilityResult.Failed;

            if (slot == AbilitySlot.Super && (_superCharges.GetValueOrDefault(chef) < 3))
                return AbilityResult.Failed;

            if (!ability.CanActivate(ctx))
                return AbilityResult.Failed;

            var result = ability.Activate(ctx);

            if (result.Success)
            {
                SetCooldown(chef, slot, ability.Cooldown);
                if (slot == AbilitySlot.Super)
                    _superCharges[chef] = 0;
            }

            return result;
        }

        public void ChargeSuper(ChefId chef, int dishesServed)
        {
            _superCharges[chef] = _superCharges.GetValueOrDefault(chef) + 1;
        }

        public float GetCooldownRemaining(ChefId chef, AbilitySlot slot)
        {
            if (!_cooldowns.TryGetValue(chef, out var slots) || !slots.TryGetValue(slot, out var remaining))
                return 0f;
            return Math.Max(0f, remaining);
        }

        public void UpdateCooldowns(float deltaTime)
        {
            foreach (var chef in _cooldowns)
            {
                var keys = new List<AbilitySlot>(chef.Value.Keys);
                foreach (var slot in keys)
                {
                    chef.Value[slot] -= deltaTime;
                    if (chef.Value[slot] <= 0)
                        chef.Value.Remove(slot);
                }
            }
        }

        private void SetCooldown(ChefId chef, AbilitySlot slot, float cooldown)
        {
            if (!_cooldowns.ContainsKey(chef))
                _cooldowns[chef] = new Dictionary<AbilitySlot, float>();
            _cooldowns[chef][slot] = cooldown;
        }
    }
}
