using Playcenter;

namespace RecipeRage
{
    /// <summary>
    /// Validates the held plate against the current recipe and serves it.
    /// </summary>
    public sealed class ServingStation : StationBase
    {
        private IEventBus _eventBus;
        private MatchController _match;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _match = ServiceLocator.Get<MatchController>();
            _stationName = "Serving Counter";
        }

        public override bool CanInteract(PlayerController player)
        {
            return player.Carry.HasPlate;
        }

        public override void Interact(PlayerController player)
        {
            var plate = player.Carry.ReleasePlate();
            if (_match.TryServePlate(plate))
            {
                _eventBus.Publish(new RecipeServedEvent(_match.CurrentRecipeId));
            }
            else
            {
                // Validation failed — hand the plate back so nothing is lost
                player.Carry.TakePlate(plate);
            }
        }

        public override string GetPrompt() => "Serve dish";
    }
}
