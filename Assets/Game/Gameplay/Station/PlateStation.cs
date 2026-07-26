using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Dispenses empty plates (one at a time per player). Also accepts ingredients
    /// onto the held plate (arrange) when the player already holds one.
    /// </summary>
    public sealed class PlateStation : StationBase
    {
        private IEventBus _eventBus;
        private int _plateCapacity;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _plateCapacity = ServiceLocator.Get<IConfigService>().Get(ConfigKeys.PlateCapacity, ConfigKeys.Defaults.PlateCapacity);
            _stationName = "Plate Station";
        }

        public override bool CanInteract(PlayerController player)
        {
            return !player.Carry.HasPlate || HasArrangeableItem(player);
        }

        public override void Interact(PlayerController player)
        {
            if (!player.Carry.HasPlate)
            {
                player.Carry.TakePlate(new Plate(_plateCapacity));
                _eventBus.Publish(new PlateTakenEvent());
                return;
            }

            // Arrange one carried item onto the held plate
            foreach (var item in player.Carry.Items)
            {
                if (player.Carry.Plate.TryArrange(item))
                {
                    player.Carry.Remove(item);
                    _eventBus.Publish(new IngredientPlatedEvent(item.Definition.Type));
                    return;
                }
            }
        }

        public override string GetPrompt() => "Take plate / Arrange ingredient";

        private static bool HasArrangeableItem(PlayerController player)
        {
            return player.Carry.Items.Count > 0 && !player.Carry.Plate.IsFull;
        }
    }
}
