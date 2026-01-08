using System.Collections.Generic;
using System.Linq;
using Core.Bootstrap;
using Modules.Shared.Enums;
using Modules.Logging;
using Gameplay.Networking.Bot;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Manages player and bot spawning using spawn points in the scene.
    /// Supports team-based spawning and spawn point management.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private bool _randomizeSpawnPosition = true;
        [SerializeField] private bool _reuseSpawnPoints = true;
        [SerializeField] private float _spawnCooldown = 2f;

        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _botPrefab;

        private List<SpawnPoint> _allSpawnPoints = new List<SpawnPoint>();
        private Dictionary<TeamCategory, List<SpawnPoint>> _spawnPointsByTeam = new Dictionary<TeamCategory, List<SpawnPoint>>();
        private Dictionary<ulong, SpawnPoint> _playerSpawnPoints = new Dictionary<ulong, SpawnPoint>();
        private float _lastSpawnTime;

        private void Awake()
        {
            InitializeSpawnPoints();
        }

        /// <summary>
        /// Initialize and categorize all spawn points in the scene
        /// </summary>
        private void InitializeSpawnPoints()
        {
            _allSpawnPoints = FindObjectsOfType<SpawnPoint>().ToList();

            if (_allSpawnPoints.Count == 0)
            {
                GameLogger.LogWarning("[SpawnManager] No spawn points found in scene!");
                return;
            }

            // Categorize by team
            _spawnPointsByTeam.Clear();
            _spawnPointsByTeam[TeamCategory.Neutral] = new List<SpawnPoint>();
            _spawnPointsByTeam[TeamCategory.TeamA] = new List<SpawnPoint>();
            _spawnPointsByTeam[TeamCategory.TeamB] = new List<SpawnPoint>();

            foreach (var spawnPoint in _allSpawnPoints)
            {
                _spawnPointsByTeam[spawnPoint.TeamCategory].Add(spawnPoint);
            }

            GameLogger.Log($"[SpawnManager] Initialized {_allSpawnPoints.Count} spawn points:");
            GameLogger.Log($"  - Neutral: {_spawnPointsByTeam[TeamCategory.Neutral].Count}");
            GameLogger.Log($"  - Team A: {_spawnPointsByTeam[TeamCategory.TeamA].Count}");
            GameLogger.Log($"  - Team B: {_spawnPointsByTeam[TeamCategory.TeamB].Count}");
        }

        /// <summary>
        /// Spawn a player at an appropriate spawn point
        /// </summary>
        public bool SpawnPlayer(ulong clientId, TeamCategory team = TeamCategory.Neutral)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[SpawnManager] Only server can spawn players");
                return false;
            }

            if (_playerPrefab == null)
            {
                GameLogger.LogError("[SpawnManager] Player prefab is not assigned!");
                return false;
            }

            if (Time.time - _lastSpawnTime < _spawnCooldown)
            {
                GameLogger.LogWarning("[SpawnManager] Spawn cooldown active");
                return false;
            }

            SpawnPoint spawnPoint = GetAvailableSpawnPoint(team);
            if (spawnPoint == null)
            {
                GameLogger.LogError($"[SpawnManager] No available spawn point for team {team}");
                return false;
            }

            Vector3 spawnPosition = spawnPoint.GetSpawnPosition(_randomizeSpawnPosition);
            Quaternion spawnRotation = spawnPoint.GetSpawnRotation();

            GameObject playerObject = Instantiate(_playerPrefab, spawnPosition, spawnRotation);
            playerObject.name = $"Player_{clientId}";

            NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                GameLogger.LogError("[SpawnManager] Player prefab missing NetworkObject component!");
                Destroy(playerObject);
                return false;
            }

            // Spawn as player object
            networkObject.SpawnAsPlayerObject(clientId);

            // Track spawn point usage
            _playerSpawnPoints[clientId] = spawnPoint;
            if (!_reuseSpawnPoints)
            {
                spawnPoint.IsAvailable = false;
            }

            _lastSpawnTime = Time.time;

            GameLogger.Log($"[SpawnManager] Spawned player {clientId} at {spawnPosition} (Team: {team})");
            return true;
        }

        /// <summary>
        /// Spawn a bot at an appropriate spawn point
        /// </summary>
        public bool SpawnBot(BotPlayer botPlayer, TeamCategory team = TeamCategory.Neutral)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                GameLogger.LogWarning("[SpawnManager] Only server can spawn bots");
                return false;
            }

            if (_botPrefab == null)
            {
                GameLogger.LogError("[SpawnManager] Bot prefab is not assigned!");
                return false;
            }

            SpawnPoint spawnPoint = GetAvailableSpawnPoint(team);
            if (spawnPoint == null)
            {
                GameLogger.LogError($"[SpawnManager] No available spawn point for team {team}");
                return false;
            }

            Vector3 spawnPosition = spawnPoint.GetSpawnPosition(_randomizeSpawnPosition);
            Quaternion spawnRotation = spawnPoint.GetSpawnRotation();

            GameObject botObject = Instantiate(_botPrefab, spawnPosition, spawnRotation);
            botObject.name = $"Bot_{botPlayer.BotName}";

            NetworkObject networkObject = botObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                GameLogger.LogError("[SpawnManager] Bot prefab missing NetworkObject component!");
                Destroy(botObject);
                return false;
            }

            // Initialize bot controller
            var botController = botObject.GetComponent<BotController>();
            if (botController != null)
            {
                botController.Initialize(botPlayer);
            }

            // Spawn as network object (not player object)
            networkObject.Spawn();

            if (!_reuseSpawnPoints)
            {
                spawnPoint.IsAvailable = false;
            }

            GameLogger.Log($"[SpawnManager] Spawned bot {botPlayer.BotName} at {spawnPosition} (Team: {team})");
            return true;
        }

        /// <summary>
        /// Spawn multiple bots
        /// </summary>
        public void SpawnBots(List<BotPlayer> botPlayers, TeamCategory team = TeamCategory.Neutral)
        {
            foreach (var botPlayer in botPlayers)
            {
                SpawnBot(botPlayer, team);
            }
        }

        /// <summary>
        /// Get an available spawn point for the specified team
        /// </summary>
        private SpawnPoint GetAvailableSpawnPoint(TeamCategory team)
        {
            // Try team-specific spawn points first
            List<SpawnPoint> teamSpawnPoints = _spawnPointsByTeam[team];
            SpawnPoint spawnPoint = GetRandomAvailableSpawnPoint(teamSpawnPoints);

            // Fallback to neutral spawn points if no team-specific points available
            if (spawnPoint == null && team != TeamCategory.Neutral)
            {
                List<SpawnPoint> neutralSpawnPoints = _spawnPointsByTeam[TeamCategory.Neutral];
                spawnPoint = GetRandomAvailableSpawnPoint(neutralSpawnPoints);
            }

            return spawnPoint;
        }

        /// <summary>
        /// Get a random available spawn point from a list
        /// </summary>
        private SpawnPoint GetRandomAvailableSpawnPoint(List<SpawnPoint> spawnPoints)
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
                return null;

            // Get available spawn points
            List<SpawnPoint> availablePoints = spawnPoints.Where(sp => sp.IsAvailable).ToList();

            if (availablePoints.Count == 0)
            {
                // If reuse is enabled, all points are available
                if (_reuseSpawnPoints)
                {
                    availablePoints = spawnPoints;
                }
                else
                {
                    return null;
                }
            }

            // Return random spawn point
            return availablePoints[Random.Range(0, availablePoints.Count)];
        }

        /// <summary>
        /// Release a spawn point when a player despawns
        /// </summary>
        public void ReleaseSpawnPoint(ulong clientId)
        {
            if (_playerSpawnPoints.TryGetValue(clientId, out SpawnPoint spawnPoint))
            {
                spawnPoint.IsAvailable = true;
                _playerSpawnPoints.Remove(clientId);
                GameLogger.Log($"[SpawnManager] Released spawn point for player {clientId}");
            }
        }

        /// <summary>
        /// Reset all spawn points to available
        /// </summary>
        public void ResetAllSpawnPoints()
        {
            foreach (var spawnPoint in _allSpawnPoints)
            {
                spawnPoint.IsAvailable = true;
            }

            _playerSpawnPoints.Clear();
            GameLogger.Log("[SpawnManager] Reset all spawn points");
        }

        /// <summary>
        /// Get spawn point count by team
        /// </summary>
        public int GetSpawnPointCount(TeamCategory team)
        {
            return _spawnPointsByTeam.TryGetValue(team, out var points) ? points.Count : 0;
        }

        /// <summary>
        /// Get available spawn point count by team
        /// </summary>
        public int GetAvailableSpawnPointCount(TeamCategory team)
        {
            if (!_spawnPointsByTeam.TryGetValue(team, out var points))
                return 0;

            return points.Count(sp => sp.IsAvailable);
        }

        /// <summary>
        /// Check if spawn points are available for a team
        /// </summary>
        public bool HasAvailableSpawnPoints(TeamCategory team)
        {
            return GetAvailableSpawnPointCount(team) > 0 ||
                   (_reuseSpawnPoints && GetSpawnPointCount(team) > 0);
        }
    }
}
