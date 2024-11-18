using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using RecipeRage.Gameplay.Kitchen.Recipes;

namespace RecipeRage.Gameplay.Core
{
    public class CharacterRecipes : NetworkBehaviour
    {
        [SerializeField] private Recipe[] availableRecipes;
        
        private readonly NetworkVariable<int> _activeRecipeId = new(-1);
        private readonly NetworkVariable<float> _recipeProgress = new();

        public Recipe GetRecipe(int recipeId)
        {
            foreach (var recipe in availableRecipes)
            {
                if (recipe.Id == recipeId)
                    return recipe;
            }
            return null;
        }

        public Recipe[] GetAvailableRecipes()
        {
            return availableRecipes;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetActiveRecipeServerRpc(int recipeId)
        {
            _activeRecipeId.Value = recipeId;
            _recipeProgress.Value = 0f;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateRecipeProgressServerRpc(float progress)
        {
            _recipeProgress.Value = Mathf.Clamp01(progress);
        }

        public int GetActiveRecipeId()
        {
            return _activeRecipeId.Value;
        }

        public float GetRecipeProgress()
        {
            return _recipeProgress.Value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _activeRecipeId.Value = -1;
                _recipeProgress.Value = 0f;
            }
        }
    }
}
