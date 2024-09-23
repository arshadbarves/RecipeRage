using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.GameMode
{
    public enum GameModeState
    {
        PreGame,
        InGame,
        GameOver
    }

    public abstract class BaseGameMode : NetworkBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private Text countdownText; // Reference to the UI Text element

        public LevelData LevelData => levelData;

        private Dictionary<string, IngredientData> _ingredientDataCache;
        protected GameModeState CurrentState { get; set; }

        private void Awake()
        {
            _ingredientDataCache = levelData.AvailableRecipes.SelectMany(recipe => recipe.Ingredients)
                .Select(ingredientRequirement => ingredientRequirement.Ingredient)
                .ToDictionary(ingredientData => ingredientData.IngredientId);
        }

        public IngredientData GetIngredientData(string ingredientId)
        {
            return _ingredientDataCache[ingredientId];
        }

        public RecipeData GetRecipeData(string recipeId)
        {
            return levelData.AvailableRecipes.Find(recipe => recipe.RecipeId == recipeId);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                StartCoroutine(StartGameWithDelay());
            }
        }

        // TODO: move this to gameplay system and use a timer system
        private IEnumerator StartGameWithDelay()
        {
            CurrentState = GameModeState.PreGame;
            int countdown = 3;
            while (countdown > 0)
            {
                countdownText.text = countdown.ToString(); // Update the UI text
                yield return new WaitForSeconds(1);
                countdown--;
            }
            countdownText.text = "Go!"; // Optional: Display "Go!" when the countdown ends
            yield return new WaitForSeconds(1);
            countdownText.text = ""; // Clear the text
            StartGame();
        }

        public abstract void StartGame();
        public abstract void GameOver();

        [ServerRpc]
        protected void EndGameServerRpc()
        {
            EndGameClientRpc();
        }

        [ClientRpc]
        private void EndGameClientRpc()
        {
            GameOver();
        }
    }
}