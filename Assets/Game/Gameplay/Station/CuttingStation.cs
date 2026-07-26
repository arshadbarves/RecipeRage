using Playcenter;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Tap-burst chopping. Player places an unchopped ingredient, then taps the
    /// Chop button ChopTaps times. Faster tapping = done sooner. Pure skill —
    /// no chef ability affects tap count.
    /// </summary>
    public sealed class CuttingStation : StationBase
    {
        private IngredientItem _current;
        private int _tapsRemaining;
        private PlayerController _placingPlayer;
        private IEventBus _eventBus;

        public bool HasIngredient => _current != null;
        public float Progress01 => _current == null ? 0f : 1f - (float)_tapsRemaining / _current.Definition.ChopTaps;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _stationName = "Cutting Board";
        }

        private void Update()
        {
            // Chop input is global (ChopPressed) while this station has an item
            // and the placing player is still nearby — Slice 2 scopes this per-player.
            if (_current != null && _placingPlayer != null && ServiceLocator.Get<IInputService>().ChopPressed)
            {
                OnChopTap();
            }
        }

        public override bool CanInteract(PlayerController player)
        {
            return _current == null && HasUnchoppedIngredient(player);
        }

        public override void Interact(PlayerController player)
        {
            var item = TakeFirstUnchopped(player);
            if (item == null)
            {
                return;
            }

            _current = item;
            _tapsRemaining = item.Definition.ChopTaps;
            _placingPlayer = player;
        }

        public override string GetPrompt() => _current == null ? "Place ingredient to chop" : $"Chop! ({_tapsRemaining} taps)";

        private void OnChopTap()
        {
            _tapsRemaining--;
            if (_tapsRemaining <= 0)
            {
                _current.Chop();
                _eventBus.Publish(new IngredientChoppedEvent(_current.Definition.Type));
                _placingPlayer.Carry.TryAdd(_current);
                _current = null;
                _placingPlayer = null;
            }
        }

        /// <summary>Server-side chop tap entry (from NetworkCuttingStation RPC).</summary>
        public void ChopTapFromNetwork()
        {
            if (_current != null)
            {
                OnChopTap();
            }
        }

        private static bool HasUnchoppedIngredient(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresChopping && !item.IsChopped)
                {
                    return true;
                }
            }
            return false;
        }

        private static IngredientItem TakeFirstUnchopped(PlayerController player)
        {
            foreach (var item in player.Carry.Items)
            {
                if (item.Definition.RequiresChopping && !item.IsChopped)
                {
                    player.Carry.Remove(item);
                    return item;
                }
            }
            return null;
        }
    }
}
