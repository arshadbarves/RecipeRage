using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Player
{
    public class PlayerRecipes : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxActiveRecipes = 2;

        private readonly NetworkList<RecipeProgress> _activeRecipes;
        private readonly Dictionary<Recipe, float> _recipeProgress = new();

        public PlayerRecipes()
        {
            _activeRecipes = new NetworkList<RecipeProgress>();
        }

        public bool TryStartRecipe(Recipe recipe)
        {
            if (!IsServer) return false;
            if (_activeRecipes.Count >= maxActiveRecipes) return false;

            var progress = new RecipeProgress
            {
                RecipeId = recipe.Id,
                Progress = 0f,
                Quality = 1f
            };

            _activeRecipes.Add(progress);
            _recipeProgress[recipe] = 0f;

            OnRecipesUpdatedClientRpc();
            return true;
        }

        public bool TryCompleteRecipe(Recipe recipe, List<InventoryItem> ingredients)
        {
            if (!IsServer) return false;
            if (!_recipeProgress.ContainsKey(recipe)) return false;

            // Validate ingredients
            if (!recipe.ValidateIngredients(ingredients))
                return false;

            // Calculate quality based on ingredients
            float quality = recipe.CalculateQuality(ingredients);

            // Remove recipe from active recipes
            for (int i = 0; i < _activeRecipes.Count; i++)
            {
                if (_activeRecipes[i].RecipeId == recipe.Id)
                {
                    _activeRecipes.RemoveAt(i);
                    break;
                }
            }

            _recipeProgress.Remove(recipe);

            // Award points based on quality
            AwardPointsServerRpc(quality);
            OnRecipesUpdatedClientRpc();
            return true;
        }

        [ServerRpc]
        private void AwardPointsServerRpc(float quality)
        {
            // TODO: Implement scoring system
        }

        [ClientRpc]
        private void OnRecipesUpdatedClientRpc()
        {
            // Notify UI or other systems of recipe changes
        }
    }

    public struct RecipeProgress : INetworkSerializable, IEquatable<RecipeProgress>
    {
        public int RecipeId;
        public float Progress;
        public float Quality;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref RecipeId);
            serializer.SerializeValue(ref Progress);
            serializer.SerializeValue(ref Quality);
        }
        public bool Equals(RecipeProgress other)
        {
            return RecipeId == other.RecipeId && Progress.Equals(other.Progress) && Quality.Equals(other.Quality);
        }
    }

    public class Recipe : ScriptableObject
    {
        public int Id;
        public string Name;
        public float BaseCookTime;
        public List<IngredientRequirement> RequiredIngredients;

        public bool ValidateIngredients(List<InventoryItem> ingredients)
        {
            // TODO: Implement ingredient validation
            return true;
        }

        public float CalculateQuality(List<InventoryItem> ingredients)
        {
            // TODO: Implement quality calculation
            return 1f;
        }
    }

    [System.Serializable]
    public struct IngredientRequirement
    {
        public int IngredientId;
        public int Count;
        public float MinQuality;
    }
}
