// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Core.Data.Config;
// using Core.GameFramework.Event.Core;
// using Gameplay.Character.AI;
// using Gameplay.Character.Management.Events;
// using Unity.Netcode;
// using UnityEngine;
// using VContainer;
//
// namespace Gameplay.Character.Management
// {
//     public class CharacterManager : NetworkBehaviour
//     {
//         [Header("Character Settings"), SerializeField]
//          private float respawnDelay = 3f;
//         [SerializeField] private int maxPlayersPerTeam = 4;
//         private readonly Dictionary<ulong, BotController> _bots = new Dictionary<ulong, BotController>();
//         private readonly NetworkVariable<GameMode> _currentGameMode = new NetworkVariable<GameMode>();
//
//         private readonly NetworkVariable<int> _maxTeams = new NetworkVariable<int>();
//
//         private readonly Dictionary<ulong, ChefCharacter> _players = new Dictionary<ulong, ChefCharacter>();
//         private readonly Dictionary<ushort, TeamInfo> _teams = new Dictionary<ushort, TeamInfo>();
//
//         private EventManager _eventManager;
//         private GameConfig _gameConfig;
//         private SpawnPointManager _spawnPointManager;
//
//         public IReadOnlyDictionary<ulong, ChefCharacter> Players => _players;
//         public IReadOnlyDictionary<ushort, TeamInfo> Teams => _teams;
//
//         [Inject]
//         public void Construct(
//             EventManager eventManager,
//             GameConfig gameConfig,
//             SpawnPointManager spawnPointManager)
//         {
//             _eventManager = eventManager;
//             _gameConfig = gameConfig;
//             _spawnPointManager = spawnPointManager;
//         }
//
//         public override void OnNetworkSpawn()
//         {
//             if (IsServer)
//             {
//                 NetworkManager.OnClientConnectedCallback += OnClientConnected;
//                 NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
//                 InitializeTeams();
//             }
//         }
//
//         private void InitializeTeams()
//         {
//             if (!IsServer) return;
//
//             _maxTeams.Value = _currentGameMode.Value switch {
//                 GameMode.Solo => _gameConfig.gameplay.maxPlayers,
//                 GameMode.Duos => _gameConfig.gameplay.maxPlayers / 2,
//                 GameMode.Teams => 2,
//                 _ => throw new ArgumentOutOfRangeException()
//             };
//
//             for (ushort i = 0; i < _maxTeams.Value; i++)
//             {
//                 _teams[i] = new TeamInfo {
//                     TeamId = i, Score = 0, Players = new HashSet<ulong>(), MaxPlayers = maxPlayersPerTeam
//                 };
//             }
//         }
//
//         public async void SpawnPlayerCharacter(ulong clientId, CharacterData characterData)
//         {
//             if (!IsServer) return;
//
//             // Find appropriate team
//             ushort teamId = AssignTeam(clientId);
//
//             // Get spawn point
//             Vector3 spawnPoint = _spawnPointManager.GetTeamSpawnPoint(teamId);
//
//             // Instantiate character
//             GameObject characterObj = (GameObject)Instantiate(
//                 characterData.characterPrefab,
//                 spawnPoint,
//                 Quaternion.identity
//             );
//
//             NetworkObject netObj = characterObj.GetComponent<NetworkObject>();
//             netObj.SpawnWithOwnership(clientId);
//
//             ChefCharacter character = characterObj.GetComponent<ChefCharacter>();
//             character.AssignToTeam(teamId);
//
//             _players[clientId] = character;
//             _teams[teamId].Players.Add(clientId);
//
//             // Notify systems of new player
//             _eventManager.Publish(new PlayerSpawnedEvent(clientId, character));
//         }
//
//         private ushort AssignTeam(ulong clientId)
//         {
//             switch (_currentGameMode.Value)
//             {
//                 case GameMode.Solo:
//                     return (ushort)clientId; // Each player is their own team
//
//                 case GameMode.Duos:
//                 case GameMode.Teams:
//                     // Find team with fewest players
//                     return FindOptimalTeam();
//
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//         }
//
//         private ushort FindOptimalTeam()
//         {
//             ushort selectedTeam = 0;
//             int minPlayers = int.MaxValue;
//             float lowestScore = float.MaxValue;
//
//             foreach (TeamInfo team in _teams.Values)
//             {
//                 if (team.Players.Count >= team.MaxPlayers) continue;
//
//                 // First priority: team with fewer players
//                 if (team.Players.Count < minPlayers)
//                 {
//                     minPlayers = team.Players.Count;
//                     selectedTeam = team.TeamId;
//                     lowestScore = team.Score;
//                 }
//                 // Second priority: team with lower score (for balance)
//                 else if (team.Players.Count == minPlayers && team.Score < lowestScore)
//                 {
//                     selectedTeam = team.TeamId;
//                     lowestScore = team.Score;
//                 }
//             }
//
//             return selectedTeam;
//         }
//
//         private async void OnClientDisconnected(ulong clientId)
//         {
//             if (!IsServer) return;
//
//             if (_players.TryGetValue(clientId, out ChefCharacter character))
//             {
//                 // Create bot to take over
//                 await ReplaceWithBot(clientId, character);
//
//                 // Remove from active players
//                 _players.Remove(clientId);
//
//                 // Notify systems of disconnection
//                 _eventManager.Publish(new PlayerDisconnectedEvent(clientId, character));
//             }
//         }
//
//         private async void OnClientConnected(ulong clientId)
//         {
//             if (!IsServer) return;
//
//             // Check if there's a bot playing for this player
//             if (_bots.TryGetValue(clientId, out BotController bot))
//             {
//                 await RestorePlayerControl(clientId, bot);
//             }
//         }
//
//         private async Task ReplaceWithBot(ulong clientId, ChefCharacter character)
//         {
//             if (!IsServer) return;
//
//             // Create bot controller
//             BotController botController = character.gameObject.AddComponent<BotController>();
//             _bots[clientId] = botController;
//
//             // Set bot difficulty based on player's skill level
//             await SetBotDifficulty(botController, clientId);
//
//             // Enable bot control
//             character.SetAsBot(true);
//             botController.enabled = true;
//
//             _eventManager.Publish(new BotTakeoverEvent(clientId, character));
//         }
//
//         private async Task SetBotDifficulty(BotController bot, ulong formerPlayerId)
//         {
//             // Get player's skill rating from profile/stats
//             float playerSkill = await GetPlayerSkillRating(formerPlayerId);
//
//             // Map skill rating to bot difficulty
//             CharacterData.BotDifficulty difficulty = playerSkill switch {
//                 float s when s >= 2000 => CharacterData.BotDifficulty.Expert,
//                 float s when s >= 1500 => CharacterData.BotDifficulty.Hard,
//                 float s when s >= 1000 => CharacterData.BotDifficulty.Medium,
//                 _ => CharacterData.BotDifficulty.Easy
//             };
//
//             bot.SetDifficulty(difficulty);
//         }
//
//         private async Task RestorePlayerControl(ulong clientId, BotController bot)
//         {
//             if (!IsServer) return;
//
//             ChefCharacter character = bot.GetComponent<ChefCharacter>();
//
//             // Disable bot control
//             character.SetAsBot(false);
//             bot.TakeOverBot();
//
//             // Remove bot component
//             Destroy(bot);
//             _bots.Remove(clientId);
//
//             // Add back to active players
//             _players[clientId] = character;
//
//             // Transfer ownership to player
//             character.GetComponent<NetworkObject>().ChangeOwnership(clientId);
//
//             _eventManager.Publish(new PlayerReconnectedEvent(clientId, character));
//         }
//
//         public void UpdateTeamScore(ushort teamId, int scoreChange)
//         {
//             if (!IsServer) return;
//
//             if (_teams.TryGetValue(teamId, out TeamInfo team))
//             {
//                 team.Score += scoreChange;
//                 NotifyTeamScoreUpdatedClientRpc(teamId, team.Score);
//             }
//         }
//
//         [ClientRpc]
//         private void NotifyTeamScoreUpdatedClientRpc(ushort teamId, int newScore)
//         {
//             if (_teams.TryGetValue(teamId, out TeamInfo team))
//             {
//                 team.Score = newScore;
//                 _eventManager.Publish(new TeamScoreUpdatedEvent(teamId, newScore));
//             }
//         }
//
//         public void SetGameMode(GameMode mode)
//         {
//             if (!IsServer) return;
//
//             _currentGameMode.Value = mode;
//             InitializeTeams();
//         }
//
//         public override void OnNetworkDespawn()
//         {
//             if (IsServer)
//             {
//                 NetworkManager.OnClientConnectedCallback -= OnClientConnected;
//                 NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
//             }
//
//             _players.Clear();
//             _bots.Clear();
//             _teams.Clear();
//         }
//
//         private async Task<float> GetPlayerSkillRating(ulong playerId)
//         {
//             // TODO: Implementation to get player's skill rating from profile/stats system
//             return 1000f; // Default rating
//         }
//     }
//
//     public class TeamInfo
//     {
//         public ushort TeamId { get; set; }
//         public int Score { get; set; }
//         public HashSet<ulong> Players { get; set; }
//         public int MaxPlayers { get; set; }
//     }
//
//     public enum GameMode
//     {
//         Solo,
//         Duos,
//         Teams
//     }
// }
