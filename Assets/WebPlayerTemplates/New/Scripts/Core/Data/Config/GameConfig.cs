using System;
using UnityEngine;

namespace Core.Data.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "RecipeRage/Config/GameConfig")]
    public class GameConfig : ScriptableObject
    {

        public enum LogLevel
        {
            None,
            Error,
            Warning,
            Info,
            Debug
        }

        private static GameConfig _instance;

        [Header("Game Settings")]
        public GameplaySettings gameplay;
        public PlayerSettings player;
        public KitchenSettings kitchen;

        [Header("Development")]
        public bool enableDebugMode;
        public bool enableCheatCommands;
        public LogLevel logLevel = LogLevel.Info;
        public static GameConfig Instance {
            get {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("Configs/GameConfig");
                }
                return _instance;
            }
        }

        private void OnValidate()
        {
            ValidateGameplaySettings();
            ValidatePlayerSettings();
            ValidateKitchenSettings();
        }

        private void ValidateGameplaySettings()
        {
            gameplay.roundDuration = Mathf.Max(60f, gameplay.roundDuration);
            gameplay.preparationTime = Mathf.Max(0f, gameplay.preparationTime);
            gameplay.maxPlayers = Mathf.Clamp(gameplay.maxPlayers, 2, 8);
            gameplay.minPlayersToStart = Mathf.Clamp(gameplay.minPlayersToStart, 1, gameplay.maxPlayers);
        }

        private void ValidatePlayerSettings()
        {
            player.moveSpeed = Mathf.Max(1f, player.moveSpeed);
            player.dashSpeed = Mathf.Max(player.moveSpeed, player.dashSpeed);
            player.dashDuration = Mathf.Max(0.1f, player.dashDuration);
            player.dashCooldown = Mathf.Max(player.dashDuration, player.dashCooldown);
        }

        private void ValidateKitchenSettings()
        {
            kitchen.basicCookingTime = Mathf.Max(1f, kitchen.basicCookingTime);
            kitchen.burningTimeMultiplier = Mathf.Max(1f, kitchen.burningTimeMultiplier);
            kitchen.maxIngredients = Mathf.Clamp(kitchen.maxIngredients, 1, 8);
            kitchen.orderSpawnInterval = Mathf.Max(1f, kitchen.orderSpawnInterval);
        }
        [Serializable]
        public class GameplaySettings
        {
            [Header("Round Settings")]
            public float roundDuration = 300f;
            public float preparationTime = 10f;
            public int maxPlayers = 4;
            public int minPlayersToStart = 2;

            [Header("Scoring")]
            public int baseOrderScore = 100;
            public float timeMultiplier = 1.5f;
            public float comboMultiplier = 1.2f;
            public int perfectOrderBonus = 50;
        }

        [Serializable]
        public class PlayerSettings
        {
            [Header("Movement")]
            public float moveSpeed = 5f;
            public float dashSpeed = 10f;
            public float dashDuration = 0.3f;
            public float dashCooldown = 2f;

            [Header("Interaction")]
            public float pickupRadius = 1f;
            public float throwForce = 8f;
            public float interactCooldown = 0.5f;
        }

        [Serializable]
        public class KitchenSettings
        {
            [Header("Cooking")]
            public float basicCookingTime = 5f;
            public float burningTimeMultiplier = 1.5f;
            public int maxIngredients = 4;

            [Header("Orders")]
            public float orderSpawnInterval = 15f;
            public float orderTimeLimit = 60f;
            public int maxActiveOrders = 5;
            public float urgentOrderTimeMultiplier = 0.7f;
        }
    }
}