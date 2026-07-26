using Playcenter;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dispenses raw ingredients. Which ingredient each crate holds is set per-map.
    /// </summary>
    public sealed class IngredientCrate : StationBase
    {
        [SerializeField] private IngredientDefinition _ingredient;

        private IEventBus _eventBus;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        public override bool CanInteract(PlayerController player)
        {
            return player.Carry.Items.Count < 4; // hard cap guard; capacity check inside TryAdd
        }

        public override void Interact(PlayerController player)
        {
            var item = new IngredientItem(_ingredient);
            if (player.Carry.TryAdd(item))
            {
                _eventBus.Publish(new IngredientFetchedEvent(_ingredient.Type));
            }
        }

        public override string GetPrompt() => $"Take {_ingredient.DisplayName}";
    }
}
