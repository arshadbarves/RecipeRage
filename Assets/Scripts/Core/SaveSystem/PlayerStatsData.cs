using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    /// <summary>
    /// Data class for player statistics.
    /// </summary>
    [Serializable]
    public class PlayerStatsData
    {
        // Player info
        [Header("Player Info")]
        public string PlayerName = "";
        public int UsernameChangeCount = 0; // Track how many times username has been changed
        public string PlayerId = Guid.NewGuid().ToString();
        public int Level = 1;
        public int Experience = 0;
        public int ExperienceToNextLevel = 100;
        
        // Currency
        [Header("Currency")]
        public int Coins = 0;
        public int Gems = 0;
        
        // Game stats
        [Header("Game Stats")]
        public int GamesPlayed = 0;
        public int GamesWon = 0;
        public int GamesLost = 0;
        public float TotalPlayTime = 0f;
        public int TotalScore = 0;
        public int TotalOrdersCompleted = 0;
        public int TotalOrdersFailed = 0;
        public int TotalIngredientsCut = 0;
        public int TotalIngredientsBurned = 0;
        public int TotalDishesServed = 0;
        public int HighestCombo = 0;
        
        // Character stats
        [Header("Character Stats")]
        public Dictionary<string, int> CharacterUsage = new Dictionary<string, int>();
        public string FavoriteCharacter = "";
        
        // Game mode stats
        [Header("Game Mode Stats")]
        public Dictionary<string, int> GameModeUsage = new Dictionary<string, int>();
        public string FavoriteGameMode = "";
        
        /// <summary>
        /// Add experience to the player.
        /// </summary>
        /// <param name="amount">The amount of experience to add</param>
        /// <returns>True if the player leveled up</returns>
        public bool AddExperience(int amount)
        {
            Experience += amount;
            
            bool leveledUp = false;
            
            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel = CalculateExperienceForLevel(Level + 1);
                leveledUp = true;
            }
            
            return leveledUp;
        }
        
        /// <summary>
        /// Add coins to the player.
        /// </summary>
        /// <param name="amount">The amount of coins to add</param>
        public void AddCoins(int amount)
        {
            Coins += amount;
        }
        
        /// <summary>
        /// Add gems to the player.
        /// </summary>
        /// <param name="amount">The amount of gems to add</param>
        public void AddGems(int amount)
        {
            Gems += amount;
        }
        
        /// <summary>
        /// Record a game played.
        /// </summary>
        /// <param name="won">Whether the game was won</param>
        /// <param name="gameModeId">The game mode ID</param>
        /// <param name="characterId">The character ID</param>
        /// <param name="playTime">The play time in seconds</param>
        /// <param name="score">The score</param>
        public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score)
        {
            GamesPlayed++;
            
            if (won)
            {
                GamesWon++;
            }
            else
            {
                GamesLost++;
            }
            
            TotalPlayTime += playTime;
            TotalScore += score;
            
            // Update character usage
            if (!string.IsNullOrEmpty(characterId))
            {
                if (!CharacterUsage.ContainsKey(characterId))
                {
                    CharacterUsage[characterId] = 0;
                }
                
                CharacterUsage[characterId]++;
                
                // Update favorite character
                if (string.IsNullOrEmpty(FavoriteCharacter) || CharacterUsage[characterId] > CharacterUsage[FavoriteCharacter])
                {
                    FavoriteCharacter = characterId;
                }
            }
            
            // Update game mode usage
            if (!string.IsNullOrEmpty(gameModeId))
            {
                if (!GameModeUsage.ContainsKey(gameModeId))
                {
                    GameModeUsage[gameModeId] = 0;
                }
                
                GameModeUsage[gameModeId]++;
                
                // Update favorite game mode
                if (string.IsNullOrEmpty(FavoriteGameMode) || GameModeUsage[gameModeId] > GameModeUsage[FavoriteGameMode])
                {
                    FavoriteGameMode = gameModeId;
                }
            }
        }
        
        /// <summary>
        /// Record an order completed.
        /// </summary>
        public void RecordOrderCompleted()
        {
            TotalOrdersCompleted++;
        }
        
        /// <summary>
        /// Record an order failed.
        /// </summary>
        public void RecordOrderFailed()
        {
            TotalOrdersFailed++;
        }
        
        /// <summary>
        /// Record an ingredient cut.
        /// </summary>
        public void RecordIngredientCut()
        {
            TotalIngredientsCut++;
        }
        
        /// <summary>
        /// Record an ingredient burned.
        /// </summary>
        public void RecordIngredientBurned()
        {
            TotalIngredientsBurned++;
        }
        
        /// <summary>
        /// Record a dish served.
        /// </summary>
        public void RecordDishServed()
        {
            TotalDishesServed++;
        }
        
        /// <summary>
        /// Record a combo.
        /// </summary>
        /// <param name="combo">The combo count</param>
        public void RecordCombo(int combo)
        {
            if (combo > HighestCombo)
            {
                HighestCombo = combo;
            }
        }
        
        /// <summary>
        /// Calculate the experience required for a level.
        /// </summary>
        /// <param name="level">The level</param>
        /// <returns>The experience required</returns>
        private int CalculateExperienceForLevel(int level)
        {
            // Simple formula: 100 * level
            return 100 * level;
        }
    }
}
