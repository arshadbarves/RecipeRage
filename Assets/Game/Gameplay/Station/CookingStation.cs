using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Autonomous cooking. Player places a chopped ingredient and walks away.
    /// Cooks on a timer (ALL players see the progress bar). After cooking, a
    /// burn-grace window starts; uncollected food burns (no score penalty,
    /// just wasted time). Interaction while ready collects the item.
    /// </summary>
    public sealed class CookingStation : StationBase
    {
        private enum Phase { Idle, Cooking, Ready, Burnt }

        private Phase _phase = Phase.Idle;
        private IngredientItem _current;
        private float _timer;
        private float _burnGrace;
        private IEventBus _eventBus;

        public int StationId => GetInstanceID();
        public float Progress01 { get; private set; }
        public bool IsBurning => _phase == Phase.Burnt;
        public bool HasReadyItem => _phase == Phase.Ready;
        public bool IsActive => _phase == Phase.Cooking || _phase == Phase.Ready;
        public string CurrentPhaseName => _phase.ToString();
        public bool LocalTickEnabled = true;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _burnGrace = ServiceLocator.Get<IConfigService>().Get(ConfigKeys.BurnGraceSec, ConfigKeys.Defaults.BurnGraceSec);
            _stationName = "Stove";
        }

        private void Update()
        {
            if (!LocalTickEnabled)
            {
                return;
            }
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            switch (_phase)
            {
                case Phase.Cooking:
                    _timer -= deltaTime;
                    Progress01 = 1f - Mathf.Clamp01(_timer / _current.Definition.CookSeconds);
                    if (_timer <= 0f)
                    {
                        _phase = Phase.Ready;
                        _timer = _burnGrace;
                        Progress01 = 1f;
                        _current.Cook();
                        _eventBus.Publish(new CookingCompletedEvent(StationId));
                    }
                    break;

                case Phase.Ready:
                    _timer -= deltaTime;
                    Progress01 = Mathf.Clamp01(_timer / _burnGrace); // drains = burn warning
                    if (_timer <= 0f)
                    {
                        _phase = Phase.Burnt;
                        _current.Burn();
                        _eventBus.Publish(new IngredientBurntEvent(StationId));
                    }
                    break;

                case Phase.Burnt:
                    // Burnt item sits until cleared by interaction (trash it)
                    break;
            }
        }

        public override bool CanInteract(PlayerController player)
        {
            if (_phase == Phase.Idle)
            {
                return HasCookableIngredient(player);
            }
            return _phase == Phase.Ready || _phase == Phase.Burnt;
        }

        public override void Interact(PlayerController player)
        {
            switch (_phase)
            {
                case Phase.Idle:
                    var item = TakeFirstCookable(player);
                    if (item != null)
                    {
                        _current = item;
                        _timer = item.Definition.CookSeconds;
                        _phase = Phase.Cooking;
                        Progress01 = 0f;
                        _eventBus.Publish(new CookingStartedEvent(StationId));
                    }
                    break;

                case Phase.Ready:
                    if (player.Carry.TryAdd(_current))
                    {
                        _current = null;
                        _phase = Phase.Idle;
                        Progress01 = 0f;
                    }
                    break;

                case Phase.Burnt:
                    // Clear burnt food (trash)
                    _current = null;
                    _phase = Phase.Idle;
                    Progress01 = 0f;
                    break;
            }
        }

        /// <summary>Server-side entry (from NetworkCookingStation RPC).</summary>
        public void ServerInteract(PlayerController player)
        {
            Interact(player);
        }

        public override string GetPrompt() => _phase switch
        {
            Phase.Idle => "Place ingredient to cook",
            Phase.Cooking => "Cooking...",
            Phase.Ready => "Collect!",
            Phase.Burnt => "Clear burnt food",
            _ => string.Empty,
        };

        private static bool HasCookableIngredient(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresCooking && !item.IsCooked && item.IsChopped)
                {
                    return true;
                }
            }
            return false;
        }

        private static IngredientItem TakeFirstCookable(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresCooking && !item.IsCooked && item.IsChopped)
                {
                    player.Carry.Remove(item);
                    return item;
                }
            }
            return null;
        }
    }
}
