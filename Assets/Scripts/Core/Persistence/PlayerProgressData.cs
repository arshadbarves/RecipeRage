using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Persistence
{
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
        public List<string> CompletedTutorialSteps = new List<string>(); // TODO: Need to be removed not neeeded

        public bool IsCharacterUnlocked(string characterId)
        {
            return UnlockedCharacters.Contains(characterId);
        }

        public void UnlockCharacter(string characterId)
        {
            if (!UnlockedCharacters.Contains(characterId))
            {
                UnlockedCharacters.Add(characterId);
            }
        }

        public bool IsMapUnlocked(string mapId)
        {
            return UnlockedMaps.Contains(mapId);
        }

        public void UnlockMap(string mapId)
        {
            if (!UnlockedMaps.Contains(mapId))
            {
                UnlockedMaps.Add(mapId);
            }
        }

        // TODO: Need to be removed
        public bool IsRecipeUnlocked(string recipeId)
        {
            return UnlockedRecipes.Contains(recipeId);
        }

        // TODO: Need to be removed
        public void UnlockRecipe(string recipeId)
        {
            if (!UnlockedRecipes.Contains(recipeId))
            {
                UnlockedRecipes.Add(recipeId);
            }
        }

        public bool IsCosmeticUnlocked(string cosmeticId)
        {
            return UnlockedCosmetics.Contains(cosmeticId);
        }

        public void UnlockCosmetic(string cosmeticId)
        {
            if (!UnlockedCosmetics.Contains(cosmeticId))
            {
                UnlockedCosmetics.Add(cosmeticId);
            }
        }

        public bool UpdateHighScore(string gameModeId, int score)
        {
            if (!GameModeHighScores.ContainsKey(gameModeId) || score > GameModeHighScores[gameModeId])
            {
                GameModeHighScores[gameModeId] = score;
                return true;
            }

            return false;
        }

        public bool UpdateBestTime(string gameModeId, float time)
        {
            if (!GameModeBestTimes.ContainsKey(gameModeId) || time < GameModeBestTimes[gameModeId])
            {
                GameModeBestTimes[gameModeId] = time;
                return true;
            }

            return false;
        }

        public void CompleteAchievement(string achievementId)
        {
            if (!CompletedAchievements.Contains(achievementId))
            {
                CompletedAchievements.Add(achievementId);
            }
        }

        public void UpdateAchievementProgress(string achievementId, int progress)
        {
            AchievementProgress[achievementId] = progress;
        }

        // TODO: Need to be removed
        public void CompleteTutorialStep(string stepId)
        {
            if (!CompletedTutorialSteps.Contains(stepId))
            {
                CompletedTutorialSteps.Add(stepId);
            }
        }

        public bool IsTutorialStepCompleted(string stepId)
        {
            return CompletedTutorialSteps.Contains(stepId);
        }
    }
}
