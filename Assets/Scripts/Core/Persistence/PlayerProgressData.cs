using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Core.Persistence
{
    /// <summary>
    /// Data class for player progress.
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        // Unlocked content
        [Header("Unlocked Content")]
        public List<string> UnlockedCharacters = new List<string>();
        public List<string> UnlockedMaps = new List<string>();
        public List<string> UnlockedRecipes = new List<string>();
        public List<string> UnlockedCosmetics = new List<string>();

        // Game progress
        [Header("Game Progress")]
        public int HighestLevel = 0;
        public Dictionary<string, int> GameModeHighScores = new Dictionary<string, int>();
        public Dictionary<string, float> GameModeBestTimes = new Dictionary<string, float>();

        // Achievements
        [Header("Achievements")]
        public List<string> CompletedAchievements = new List<string>();
        public Dictionary<string, int> AchievementProgress = new Dictionary<string, int>();

        // Tutorial progress
        [Header("Tutorial Progress")]
        public bool TutorialCompleted = false;
        public List<string> CompletedTutorialSteps = new List<string>();

        /// <summary>
        /// Check if a character is unlocked.
        /// </summary>
        /// <param name="characterId">The character ID to check</param>
        /// <returns>True if the character is unlocked</returns>
        public bool IsCharacterUnlocked(string characterId)
        {
            return UnlockedCharacters.Contains(characterId);
        }

        /// <summary>
        /// Unlock a character.
        /// </summary>
        /// <param name="characterId">The character ID to unlock</param>
        public void UnlockCharacter(string characterId)
        {
            if (!UnlockedCharacters.Contains(characterId))
            {
                UnlockedCharacters.Add(characterId);
            }
        }

        /// <summary>
        /// Check if a map is unlocked.
        /// </summary>
        /// <param name="mapId">The map ID to check</param>
        /// <returns>True if the map is unlocked</returns>
        public bool IsMapUnlocked(string mapId)
        {
            return UnlockedMaps.Contains(mapId);
        }

        /// <summary>
        /// Unlock a map.
        /// </summary>
        /// <param name="mapId">The map ID to unlock</param>
        public void UnlockMap(string mapId)
        {
            if (!UnlockedMaps.Contains(mapId))
            {
                UnlockedMaps.Add(mapId);
            }
        }

        /// <summary>
        /// Check if a recipe is unlocked.
        /// </summary>
        /// <param name="recipeId">The recipe ID to check</param>
        /// <returns>True if the recipe is unlocked</returns>
        public bool IsRecipeUnlocked(string recipeId)
        {
            return UnlockedRecipes.Contains(recipeId);
        }

        /// <summary>
        /// Unlock a recipe.
        /// </summary>
        /// <param name="recipeId">The recipe ID to unlock</param>
        public void UnlockRecipe(string recipeId)
        {
            if (!UnlockedRecipes.Contains(recipeId))
            {
                UnlockedRecipes.Add(recipeId);
            }
        }

        /// <summary>
        /// Check if a cosmetic is unlocked.
        /// </summary>
        /// <param name="cosmeticId">The cosmetic ID to check</param>
        /// <returns>True if the cosmetic is unlocked</returns>
        public bool IsCosmeticUnlocked(string cosmeticId)
        {
            return UnlockedCosmetics.Contains(cosmeticId);
        }

        /// <summary>
        /// Unlock a cosmetic.
        /// </summary>
        /// <param name="cosmeticId">The cosmetic ID to unlock</param>
        public void UnlockCosmetic(string cosmeticId)
        {
            if (!UnlockedCosmetics.Contains(cosmeticId))
            {
                UnlockedCosmetics.Add(cosmeticId);
            }
        }

        /// <summary>
        /// Update the high score for a game mode.
        /// </summary>
        /// <param name="gameModeId">The game mode ID</param>
        /// <param name="score">The new score</param>
        /// <returns>True if the score was a new high score</returns>
        public bool UpdateHighScore(string gameModeId, int score)
        {
            if (!GameModeHighScores.ContainsKey(gameModeId) || score > GameModeHighScores[gameModeId])
            {
                GameModeHighScores[gameModeId] = score;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update the best time for a game mode.
        /// </summary>
        /// <param name="gameModeId">The game mode ID</param>
        /// <param name="time">The new time</param>
        /// <returns>True if the time was a new best time</returns>
        public bool UpdateBestTime(string gameModeId, float time)
        {
            if (!GameModeBestTimes.ContainsKey(gameModeId) || time < GameModeBestTimes[gameModeId])
            {
                GameModeBestTimes[gameModeId] = time;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Complete an achievement.
        /// </summary>
        /// <param name="achievementId">The achievement ID to complete</param>
        public void CompleteAchievement(string achievementId)
        {
            if (!CompletedAchievements.Contains(achievementId))
            {
                CompletedAchievements.Add(achievementId);
            }
        }

        /// <summary>
        /// Update achievement progress.
        /// </summary>
        /// <param name="achievementId">The achievement ID</param>
        /// <param name="progress">The new progress value</param>
        public void UpdateAchievementProgress(string achievementId, int progress)
        {
            AchievementProgress[achievementId] = progress;
        }

        /// <summary>
        /// Complete a tutorial step.
        /// </summary>
        /// <param name="stepId">The tutorial step ID to complete</param>
        public void CompleteTutorialStep(string stepId)
        {
            if (!CompletedTutorialSteps.Contains(stepId))
            {
                CompletedTutorialSteps.Add(stepId);
            }
        }

        /// <summary>
        /// Check if a tutorial step is completed.
        /// </summary>
        /// <param name="stepId">The tutorial step ID to check</param>
        /// <returns>True if the tutorial step is completed</returns>
        public bool IsTutorialStepCompleted(string stepId)
        {
            return CompletedTutorialSteps.Contains(stepId);
        }
    }
}
