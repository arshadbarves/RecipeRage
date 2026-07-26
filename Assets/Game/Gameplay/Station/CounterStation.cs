using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Temporary storage for up to 2 items. Place with one interact, take back with another.
    /// </summary>
    public sealed class CounterStation : StationBase
    {
        private readonly List<IngredientItem> _stored = new List<IngredientItem>(2);
        private const int MaxStored = 2;

        private void Start()
        {
            _stationName = "Counter";
        }

        public override bool CanInteract(PlayerController player)
        {
            return (_stored.Count < MaxStored && player.Carry.Items.Count > 0)
                || (_stored.Count > 0 && player.Carry.Items.Count < 4);
        }

        public override void Interact(PlayerController player)
        {
            // Take back first (if carrying space), else place
            if (_stored.Count > 0 && player.Carry.Items.Count == 0)
            {
                var item = _stored[_stored.Count - 1];
                _stored.RemoveAt(_stored.Count - 1);
                player.Carry.TryAdd(item);
                return;
            }

            if (_stored.Count < MaxStored && player.Carry.Items.Count > 0)
            {
                var item = player.Carry.Items[player.Carry.Items.Count - 1];
                player.Carry.Remove(item);
                _stored.Add(item);
            }
        }

        public override string GetPrompt() => $"Counter ({_stored.Count}/{MaxStored})";
    }
}
