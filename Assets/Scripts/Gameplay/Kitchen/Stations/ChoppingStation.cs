using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class ChoppingStation : BaseStation
    {
        [Header("Chopping Settings")]
        [SerializeField] private float choppingTime = 3f;
        [SerializeField] private float qualityBonus = 0.2f;
        [SerializeField] private int chopsRequired = 5;
        [SerializeField] private AudioClip[] chopSounds;

        // Network state
        private readonly NetworkVariable<int> _currentChops = new();
        private readonly NetworkVariable<float> _choppingProgress = new();

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Check for choppable ingredients
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanChopIngredient(item))
                {
                    _currentChops.Value = 0;
                    _choppingProgress.Value = 0f;
                    return true;
                }
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            if (_currentChops.Value < chopsRequired) return false;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return false;

            // Find and improve choppable ingredient
            var items = inventory.GetItems();
            foreach (var item in items)
            {
                if (CanChopIngredient(item))
                {
                    var choppedItem = new InventoryItem
                    {
                        ItemId = item.ItemId,
                        Type = item.Type,
                        Quality = Mathf.Min(1f, item.Quality + qualityBonus)
                    };

                    if (inventory.TryRemoveItem(item) && inventory.TryAddItem(choppedItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            _currentChops.Value = 0;
            _choppingProgress.Value = 0f;
            return true;
        }

        private bool CanChopIngredient(InventoryItem item)
        {
            // Define which ingredients can be chopped
            return item.Type == ItemType.Ingredient && 
                   (item.ItemId == (int)IngredientType.Tomato ||
                    item.ItemId == (int)IngredientType.Onion ||
                    item.ItemId == (int)IngredientType.Carrot ||
                    item.ItemId == (int)IngredientType.Potato ||
                    item.ItemId == (int)IngredientType.Meat);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChopServerRpc(ulong characterId)
        {
            if (CurrentState.Value != StateType.InUse) return;
            if (_currentChops.Value >= chopsRequired) return;

            _currentChops.Value++;
            _choppingProgress.Value = (float)_currentChops.Value / chopsRequired;

            // Play chop effect
            PlayChopEffectClientRpc();

            if (_currentChops.Value >= chopsRequired)
            {
                var character = FindCharacter(characterId);
                if (character != null)
                {
                    CompleteInteraction(character);
                }
            }
        }

        [ClientRpc]
        private void PlayChopEffectClientRpc()
        {
            if (chopSounds != null && chopSounds.Length > 0 && audioSource != null)
            {
                var sound = chopSounds[UnityEngine.Random.Range(0, chopSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
        }
    }
}
