using System.Collections.Generic;
using Core.Logging;
using Gameplay;
using Gameplay.Spawning;
using Unity.Netcode;
using UnityEngine;

namespace Core.Networking.Bot
{
    /// <summary>
    /// Spawns bot players as NetworkObjects on the server.
    /// Now integrates with SpawnManager for scene-based spawn points.
    /// </summary>
    public class BotSpawner
    {
        private readonly List<NetworkObject> _spawnedBots = new List<NetworkObject>();
        private readonly GameObject _botPrefab;
        private SpawnManager _spawnManager;

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
        /// <param name="botPlayers">List of bot players to spawn</param>
        /// <param name="team">Team category for spawn point selection</param>
        public void SpawnBots(List<BotPlayer> botPlayers, TeamCategory team = TeamCategory.Neutral)
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

            // Try to get SpawnManager
            _spawnManager = SpawnManagerIntegration.GetSpawnManager();

            GameLogger.Log($"[BotSpawner] Spawning {botPlayers.Count} bots (Team: {team})");

            foreach (var botPlayer in botPlayers)
            {
                SpawnBot(botPlayer, team);
            }
        }

        /// <summary>
        /// Spawn a single bot
        /// </summary>
        /// <param name="botPlayer">Bot player data</param>
        /// <param name="team">Team category for spawn point selection</param>
        private void SpawnBot(BotPlayer botPlayer, TeamCategory team)
        {
            Vector3 spawnPosition;
            Quaternion spawnRotation;

            // Use SpawnManager if available, otherwise fallback to circular pattern
            if (_spawnManager != null)
            {
                // Let SpawnManager handle the spawning
                bool spawned = _spawnManager.SpawnBot(botPlayer, team);
                if (spawned)
                {
                    // Track the spawned bot (SpawnManager already spawned it)
                    // Find the bot that was just spawned
                    var botObjects = Object.FindObjectsOfType<BotController>();
                    foreach (var spawnedBotController in botObjects)
                    {
                        if (spawnedBotController.GetBotData() == botPlayer)
                        {
                            var spawnedNetworkObject = spawnedBotController.GetComponent<NetworkObject>();
                            if (spawnedNetworkObject != null && !_spawnedBots.Contains(spawnedNetworkObject))
                            {
                                _spawnedBots.Add(spawnedNetworkObject);
                                break;
                            }
                        }
                    }
                }
                return;
            }

            // Fallback: Use circular spawn pattern if no SpawnManager
            GameLogger.LogWarning("[BotSpawner] No SpawnManager found, using fallback spawn pattern");
            spawnPosition = GetBotSpawnPosition(_spawnedBots.Count);
            spawnRotation = Quaternion.identity;

            // Instantiate bot
            GameObject botObject = Object.Instantiate(_botPrefab, spawnPosition, spawnRotation);
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
        /// Get spawn position for a bot (fallback when no SpawnManager)
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
