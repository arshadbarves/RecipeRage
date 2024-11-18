using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;
using RecipeRage.Gameplay.Kitchen.Recipes;

namespace RecipeRage.Gameplay.Kitchen.Stations
{
    public class PlatingStation : BaseStation
    {
        [Header("Plating Settings")]
        [SerializeField] private float plateTime = 3f;
        [SerializeField] private float presentationBonus = 0.15f;
        [SerializeField] private int maxIngredients = 6;
        [SerializeField] private ParticleSystem plateEffect;
        [SerializeField] private AudioClip plateSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;

        // Network state
        private readonly NetworkVariable<float> _plateProgress = new();
        private readonly NetworkList<InventoryItem> _currentIngredients;
        private readonly NetworkVariable<bool> _isPlating = new();
        private readonly NetworkVariable<int> _recipeId = new(-1);

        public PlatingStation()
        {
            _currentIngredients = new NetworkList<InventoryItem>();
        }

        private void Update()
        {
            if (!IsServer) return;
            if (!_isPlating.Value) return;

            UpdatePlating();
        }

        private void UpdatePlating()
        {
            if (_plateProgress.Value < 1f)
            {
                _plateProgress.Value = Mathf.Min(1f, _plateProgress.Value + Time.deltaTime / plateTime);
                
                if (_plateProgress.Value >= 1f)
                {
                    CompletePlating();
                }
            }
        }

        protected override bool OnStationUsageStart(BaseNetworkCharacter character)
        {
            var inventory = character.GetComponent<PlayerInventory>();
            var recipes = character.GetComponent<PlayerRecipes>();
            if (inventory == null || recipes == null) return false;

            // Allow adding ingredients if not plating
            if (!_isPlating.Value)
            {
                return true;
            }

            return false;
        }

        protected override bool OnStationUsageComplete(BaseNetworkCharacter character)
        {
            return true; // Always allow completion to check recipe
        }

        protected override bool OnStationUsageCancel(BaseNetworkCharacter character)
        {
            if (_isPlating.Value)
            {
                CancelPlatingServerRpc();
            }
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddIngredientServerRpc(ulong characterId, InventoryItem ingredient)
        {
            if (_isPlating.Value) return;
            if (_currentIngredients.Count >= maxIngredients) return;

            var character = FindCharacter(characterId);
            if (character == null) return;

            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            if (inventory.TryRemoveItem(ingredient))
            {
                _currentIngredients.Add(ingredient);
                PlayPlateEffectClientRpc(0); // Add sound
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartPlatingServerRpc(ulong characterId, int recipeId)
        {
            if (_isPlating.Value) return;
            if (_currentIngredients.Count == 0) return;

            var character = FindCharacter(characterId);
            if (character == null) return;

            var recipes = character.GetComponent<PlayerRecipes>();
            if (recipes == null) return;

            // Validate recipe
            var recipe = recipes.GetRecipe(recipeId);
            if (recipe != null && recipe.ValidateIngredients(_currentIngredients.ToArray()))
            {
                _isPlating.Value = true;
                _plateProgress.Value = 0f;
                _recipeId.Value = recipeId;
                PlayPlateEffectClientRpc(1); // Start sound
            }
            else
            {
                PlayPlateEffectClientRpc(3); // Fail sound
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CancelPlatingServerRpc()
        {
            if (!_isPlating.Value) return;

            _isPlating.Value = false;
            _plateProgress.Value = 0f;
            _recipeId.Value = -1;
            PlayPlateEffectClientRpc(2); // Cancel sound
        }

        private void CompletePlating()
        {
            if (_currentIngredients.Count == 0 || _recipeId.Value == -1) return;

            var character = FindCharacter(_currentUserId.Value);
            if (character == null) return;

            var recipes = character.GetComponent<PlayerRecipes>();
            if (recipes == null) return;

            var recipe = recipes.GetRecipe(_recipeId.Value);
            if (recipe == null) return;

            // Calculate final quality with presentation bonus
            float quality = recipe.CalculateQuality(_currentIngredients.ToArray(), _plateProgress.Value);
            quality = Mathf.Min(1f, quality + presentationBonus);

            // Create completed dish
            var dish = new InventoryItem
            {
                ItemId = recipe.Id,
                Type = ItemType.Dish,
                Quality = quality
            };

            // Add to character's inventory
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.TryAddItem(dish))
            {
                // Award points
                var stats = character.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    int points = recipe.CalculatePoints(quality);
                    stats.AddScore(points);
                }

                // Clear station
                _currentIngredients.Clear();
                _isPlating.Value = false;
                _recipeId.Value = -1;
                
                PlayPlateEffectClientRpc(4); // Success sound
            }
        }

        [ClientRpc]
        private void PlayPlateEffectClientRpc(int soundIndex)
        {
            if (plateEffect != null)
            {
                if (soundIndex == 1) // Start plating
                {
                    plateEffect.Play();
                }
                else if (soundIndex == 2) // Cancel
                {
                    plateEffect.Stop();
                }
            }

            if (audioSource != null)
            {
                AudioClip clipToPlay = null;
                switch (soundIndex)
                {
                    case 0: // Add ingredient
                        clipToPlay = plateSound;
                        break;
                    case 3: // Fail
                        clipToPlay = failSound;
                        break;
                    case 4: // Success
                        clipToPlay = successSound;
                        break;
                }

                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);
                }
            }
        }
    }
}
