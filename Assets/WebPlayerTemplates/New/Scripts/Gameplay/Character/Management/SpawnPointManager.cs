// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Unity.Netcode;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace Gameplay.Character.Management
// {
//     public class SpawnPointManager : NetworkBehaviour
//     {
//
//         [SerializeField] private List<TeamSpawnPoints> teamSpawnPoints;
//         [SerializeField] private float minSpawnDistance = 5f;
//         private readonly HashSet<Vector3> _recentlyUsedSpawns = new HashSet<Vector3>();
//
//         private readonly Dictionary<ushort, List<Transform>> _spawnPointsByTeam = new Dictionary<ushort, List<Transform>>();
//
//         // Editor validation
//         private void OnValidate()
//         {
//             foreach (TeamSpawnPoints teamSpawns in teamSpawnPoints)
//             {
//                 if (teamSpawns.spawnPoints == null || teamSpawns.spawnPoints.Count == 0)
//                 {
//                     Debug.LogError($"Team {teamSpawns.teamId} has no spawn points assigned!");
//                 }
//             }
//         }
//
//         public override void OnNetworkSpawn()
//         {
//             if (IsServer)
//             {
//                 InitializeSpawnPoints();
//             }
//         }
//
//         private void InitializeSpawnPoints()
//         {
//             foreach (TeamSpawnPoints teamSpawns in teamSpawnPoints)
//             {
//                 _spawnPointsByTeam[teamSpawns.teamId] = teamSpawns.spawnPoints;
//             }
//         }
//
//         public Vector3 GetTeamSpawnPoint(ushort teamId)
//         {
//             if (!_spawnPointsByTeam.TryGetValue(teamId, out List<Transform> spawns))
//             {
//                 Debug.LogError($"No spawn points found for team {teamId}");
//                 return Vector3.zero;
//             }
//
//             // Filter out recently used spawn points and those too close to players
//             List<Transform> validSpawns = spawns
//                 .Where(sp => !_recentlyUsedSpawns.Contains(sp.position))
//                 .Where(sp => IsSpawnPointSafe(sp.position))
//                 .ToList();
//
//             if (validSpawns.Count == 0)
//             {
//                 // Fallback to any spawn point if no valid ones are found
//                 validSpawns = spawns;
//             }
//
//             // Choose random spawn point from valid ones
//             Transform spawnPoint = validSpawns[Random.Range(0, validSpawns.Count)];
//
//             // Mark as recently used
//             _recentlyUsedSpawns.Add(spawnPoint.position);
//             CleanupRecentSpawns();
//
//             return spawnPoint.position;
//         }
//
//         private bool IsSpawnPointSafe(Vector3 spawnPosition)
//         {
//             // Check for nearby players
//             Collider[] colliders = Physics.OverlapSphere(spawnPosition, minSpawnDistance);
//             return !colliders.Any(c => c.GetComponent<ChefCharacter>() != null);
//         }
//
//         private void CleanupRecentSpawns()
//         {
//             // Keep only the last few spawn points in memory
//             const int maxRecentSpawns = 5;
//             if (_recentlyUsedSpawns.Count > maxRecentSpawns)
//             {
//                 _recentlyUsedSpawns.Clear();
//             }
//         }
//         [Serializable]
//         public class TeamSpawnPoints
//         {
//             public ushort teamId;
//             public List<Transform> spawnPoints;
//         }
//     }
// }
