using System.Collections.Generic;
using Core.Bootstrap;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Core.Networking.Bot
{
    /// <summary>
    /// Spawns bot players as NetworkObjects on the server
    /// </summary>
    public class BotSpawner
    {
        private readonly List<NetworkObject> _spawnedBots = new List<NetworkObject>();
        private readonly GameObject _botPrefab;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="botPrefab">The bot prefab to spawn (should have NetworkObject component)</param>
        public BotSpawner(GameObject botPrefab)
        {
            _botPrefab = botPrefab;
        }

        /// <summary>
        /// Spawn bots for the given bot players
        /// </summary>
        public void SpawnBots(List<BotPlayer> botPlayers)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[BotSpawner] Only server can spawn bots");
                return;
            }

            if (_botPrefab == null)
            {
                GameLogger.LogError("[BotSpawner] Bot prefab is null - cannot spawn bots");
                return;
            }

            GameLogger.Log($"[BotSpawner] Spawning {botPlayers.Count} bots");

            foreach (var botPlayer in botPlayers)
            {
                SpawnBot(botPlayer);
            }
        }

        /// <summary>
        /// Spawn a single bot
        /// </summary>
        private void SpawnBot(BotPlayer botPlayer)
        {
            // Get spawn position
            Vector3 spawnPosition = GetBotSpawnPosition(_spawnedBots.Count);

            // Instantiate bot
            GameObject botObject = Object.Instantiate(_botPrefab, spawnPosition, Quaternion.identity);
            botObject.name = $"Bot_{botPlayer.BotName}";

            // Get NetworkObject component
            NetworkObject networkObject = botObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                GameLogger.LogError($"[BotSpawner] Bot prefab missing NetworkObject component!");
                Object.Destroy(botObject);
                return;
            }

            // Get BotController component
            var botController = botObject.GetComponent<BotController>();
            if (botController != null)
            {
                botController.Initialize(botPlayer);
            }

            // Spawn as network object (not as player object)
            networkObject.Spawn();

            _spawnedBots.Add(networkObject);

            GameLogger.Log($"[BotSpawner] Spawned bot: {botPlayer.BotName} at {spawnPosition}");
        }

        /// <summary>
        /// Get spawn position for a bot
        /// </summary>
        private Vector3 GetBotSpawnPosition(int botIndex)
        {
            // Simple circular spawn pattern
            float angle = (botIndex * 45f) * Mathf.Deg2Rad;
            float radius = 8f;

            return new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
        }

        /// <summary>
        /// Despawn all bots
        /// </summary>
        public void DespawnAllBots()
        {
            // Check if NetworkManager exists and is server
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                _spawnedBots.Clear();
                return;
            }

            GameLogger.Log($"[BotSpawner] Despawning {_spawnedBots.Count} bots");

            foreach (var bot in _spawnedBots)
            {
                if (bot != null && bot.IsSpawned)
                {
                    bot.Despawn();
                    Object.Destroy(bot.gameObject);
                }
            }

            _spawnedBots.Clear();
        }

        /// <summary>
        /// Get spawned bot count
        /// </summary>
        public int GetSpawnedBotCount()
        {
            return _spawnedBots.Count;
        }
    }
}
