using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Listens for AbilityActivatedEvent from the domain event bus and dispatches
    /// to concrete ability implementations that require Unity/Infrastructure APIs.
    /// </summary>
    public sealed class AbilityEffectHandler : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Dictionary<AbilityType, ActiveAbilityBase> _handlers = new();

        public AbilityEffectHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<AbilityActivatedEvent>(OnAbilityActivated);

            // Pre-build handler instances for each concrete ability type.
            // These are lightweight stubs; real effect logic lives in ApplyEffect.
            RegisterDefaults();
        }

        private void RegisterDefaults()
        {
            Register(AbilityType.Dash, def => new DashAbility(def));
            Register(AbilityType.FlavorBoost, def => new FlavorBoostAbility(def));
            Register(AbilityType.PerfectSlice, def => new PerfectSliceAbility(def));
            Register(AbilityType.KitchenWisdom, def => new KitchenWisdomAbility(def));
            Register(AbilityType.IngredientSwap, def => new IngredientSwapAbility(def));
            Register(AbilityType.SpiceRush, def => new SpiceRushAbility(def));
        }

        private void Register(AbilityType type, Func<AbilityDefinition, ActiveAbilityBase> factory)
        {
            // Create with a minimal definition; the event carries the real values.
            var def = new AbilityDefinition(type, AbilitySlot.Active, type.ToString(), "", 0f, 0f, 0f);
            _handlers[type] = factory(def);
        }

        private void OnAbilityActivated(AbilityActivatedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_handlers.TryGetValue(evt.AbilityType, out ActiveAbilityBase handler))
            {
                // Activate the concrete handler with the event's context
                handler.Activate(evt.Context);
                Debug.Log($"[AbilityEffectHandler] Dispatched {evt.AbilityType} to concrete handler");
            }
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<AbilityActivatedEvent>(OnAbilityActivated);
            _handlers.Clear();
        }
    }
}
