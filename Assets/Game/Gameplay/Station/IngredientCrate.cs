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
            return _ingredient != null && player.Carry.Items.Count < 4;
        }

        public override void Interact(PlayerController player)
        {
            if (_ingredient == null)
            {
                return;
            }

            var item = new IngredientItem(_ingredient);
            if (player.Carry.TryAdd(item))
            {
                _eventBus.Publish(new IngredientFetchedEvent(_ingredient.Type));
            }
        }

        public override string GetPrompt() => _ingredient != null ? $"Take {_ingredient.DisplayName}" : "Empty Crate";
    }
}
