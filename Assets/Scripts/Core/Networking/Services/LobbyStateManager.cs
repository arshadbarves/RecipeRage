using System;
using System.Collections.Generic;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Sessions;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Manages lobby state and operations (Single Responsibility)
    /// </summary>
    public class LobbyStateManager : ILobbyManager
    {
        private readonly EOSLobbyManager _eosLobbyManager;
        private readonly ITeamManager _teamManager;
        
        private Lobby _currentLobby;
        private GameMode _currentGameMode = GameMode.Classic;
        private string _currentMapName = "Kitchen";
        private bool _isPrivate;

        public event Action<Result> OnLobbyCreated;
        public event Action<Result> OnLobbyJoined;
        public event Action<Result> OnLobbyLeft;
        public event Action OnLobbyUpdated;

        public bool IsLobbyOwner => _currentLobby?.IsOwner(EOSManager.Instance.GetProductUserId()) ?? false;
        public int CurrentPlayerCount => _teamManager.TeamA.Count + _teamManager.TeamB.Count;
        public GameMode CurrentGameMode => _currentGameMode;
        public string CurrentMapName => _currentMapName;
        public bool IsPrivate => _isPrivate;
        public Lobby CurrentLobby => _currentLobby;

        public LobbyStateManager(EOSLobbyManager eosLobbyManager, ITeamManager teamManager)
        {
            _eosLobbyManager = eosLobbyManager ?? throw new ArgumentNullException(nameof(eosLobbyManager));
            _teamManager = teamManager ?? throw new ArgumentNullException(nameof(teamManager));
        }

        public void Initialize()
        {
            _eosLobbyManager.LobbyChanged += OnLobbyChanged;
            Debug.Log("[LobbyStateManager] Initialized");
        }

        public void CreateLobby(string lobbyName, int maxPlayers = 4, bool isPrivate = false)
        {
            Lobby lobby = new Lobby
            {
                MaxNumLobbyMembers = (uint)maxPlayers,
                LobbyPermissionLevel = isPrivate ? 
                    LobbyPermissionLevel.Inviteonly : 
                    LobbyPermissionLevel.Publicadvertised,
                AllowInvites = true,
                PresenceEnabled = true,
                RTCRoomEnabled = true,
                BucketId = $"RecipeRage_{_currentGameMode}"
            };

            AddLobbyAttributes(lobby, lobbyName);
            _eosLobbyManager.CreateLobby(lobby, OnLobbyCreationComplete);
            _isPrivate = isPrivate;

            Debug.Log($"[LobbyStateManager] Creating lobby: {lobbyName}, MaxPlayers: {maxPlayers}, IsPrivate: {isPrivate}");
        }

        public void JoinLobby(string lobbyId, bool presenceEnabled = true)
        {
            _eosLobbyManager.SearchByLobbyId(lobbyId, (Result searchResult) =>
            {
                if (searchResult != Result.Success)
                {
                    Debug.LogError($"[LobbyStateManager] Failed to find lobby {lobbyId}: {searchResult}");
                    OnLobbyJoined?.Invoke(searchResult);
                    return;
                }

                Dictionary<Lobby, LobbyDetails> searchResults = _eosLobbyManager.GetSearchResults();
                foreach (var kvp in searchResults)
                {
                    if (kvp.Key.Id == lobbyId)
                    {
                        _eosLobbyManager.JoinLobby(lobbyId, kvp.Value, presenceEnabled, OnLobbyJoinComplete);
                        Debug.Log($"[LobbyStateManager] Joining lobby: {lobbyId}");
                        return;
                    }
                }

                Debug.LogError($"[LobbyStateManager] Lobby {lobbyId} found in search but not in results");
                OnLobbyJoined?.Invoke(Result.NotFound);
            });
        }

        public void LeaveLobby()
        {
            if (_currentLobby == null)
            {
                Debug.LogError("[LobbyStateManager] No current lobby to leave");
                OnLobbyLeft?.Invoke(Result.NotFound);
                return;
            }

            _eosLobbyManager.LeaveLobby(OnLobbyLeftComplete);
            Debug.Log($"[LobbyStateManager] Leaving lobby: {_currentLobby.Id}");
        }

        public void SetGameMode(GameMode gameMode)
        {
            if (_currentLobby == null || !IsLobbyOwner)
            {
                Debug.LogError("[LobbyStateManager] No current lobby or not lobby owner");
                return;
            }

            AddLobbyAttribute("GameMode", gameMode.ToString());
            _currentGameMode = gameMode;
            Debug.Log($"[LobbyStateManager] Setting game mode: {gameMode}");
        }

        public void SetMapName(string mapName)
        {
            if (_currentLobby == null || !IsLobbyOwner)
            {
                Debug.LogError("[LobbyStateManager] No current lobby or not lobby owner");
                return;
            }

            AddLobbyAttribute("MapName", mapName);
            _currentMapName = mapName;
            Debug.Log($"[LobbyStateManager] Setting map name: {mapName}");
        }

        public bool AreAllPlayersReady()
        {
            foreach (PlayerInfo player in _teamManager.TeamA)
            {
                if (!player.IsReady) return false;
            }

            foreach (PlayerInfo player in _teamManager.TeamB)
            {
                if (!player.IsReady) return false;
            }

            return CurrentPlayerCount > 0;
        }

        private void AddLobbyAttributes(Lobby lobby, string lobbyName)
        {
            lobby.Attributes.Add(new LobbyAttribute
            {
                Key = "LobbyName",
                ValueType = AttributeType.String,
                AsString = lobbyName,
                Visibility = LobbyAttributeVisibility.Public
            });

            lobby.Attributes.Add(new LobbyAttribute
            {
                Key = "GameMode",
                ValueType = AttributeType.String,
                AsString = _currentGameMode.ToString(),
                Visibility = LobbyAttributeVisibility.Public
            });

            lobby.Attributes.Add(new LobbyAttribute
            {
                Key = "MapName",
                ValueType = AttributeType.String,
                AsString = _currentMapName,
                Visibility = LobbyAttributeVisibility.Public
            });
        }

        private void AddLobbyAttribute(string key, string value)
        {
            if (_eosLobbyManager.GetCurrentLobby() == null) return;

            Lobby updatedLobby = new Lobby();
            updatedLobby.Attributes.Add(new LobbyAttribute
            {
                Key = key,
                ValueType = AttributeType.String,
                AsString = value,
                Visibility = LobbyAttributeVisibility.Public
            });

            _eosLobbyManager.ModifyLobby(updatedLobby, null);
        }

        private void OnLobbyChanged(object sender, EOSLobbyManager.LobbyChangeEventArgs e)
        {
            _currentLobby = _eosLobbyManager.GetCurrentLobby();
            
            if (_teamManager is TeamManager teamManager)
            {
                teamManager.UpdateTeamsFromLobby(_currentLobby);
            }
            
            ExtractLobbyAttributes();
            OnLobbyUpdated?.Invoke();
            
            Debug.Log($"[LobbyStateManager] Lobby changed: {e.LobbyId}, Type: {e.LobbyChangeType}");
        }

        private void ExtractLobbyAttributes()
        {
            if (_currentLobby == null) return;

            foreach (LobbyAttribute attribute in _currentLobby.Attributes)
            {
                switch (attribute.Key)
                {
                    case "GameMode":
                        if (Enum.TryParse<GameMode>(attribute.AsString, out GameMode gameMode))
                        {
                            _currentGameMode = gameMode;
                        }
                        break;
                    case "MapName":
                        _currentMapName = attribute.AsString;
                        break;
                }
            }

            _isPrivate = _currentLobby.LobbyPermissionLevel == LobbyPermissionLevel.Inviteonly;
        }

        private void OnLobbyCreationComplete(Result result)
        {
            if (result == Result.Success)
            {
                _currentLobby = _eosLobbyManager.GetCurrentLobby();
                if (_teamManager is TeamManager teamManager)
                {
                    teamManager.UpdateTeamsFromLobby(_currentLobby);
                }
                Debug.Log("[LobbyStateManager] Lobby created");
            }
            else
            {
                Debug.LogError($"[LobbyStateManager] Failed to create lobby: {result}");
            }

            OnLobbyCreated?.Invoke(result);
        }

        public void UpdatePlayerManagerLobby(PlayerManager playerManager)
        {
            if (playerManager != null)
            {
                playerManager.SetCurrentLobby(_currentLobby);
            }
        }

        private void OnLobbyJoinComplete(Result result)
        {
            if (result == Result.Success)
            {
                _currentLobby = _eosLobbyManager.GetCurrentLobby();
                if (_teamManager is TeamManager teamManager)
                {
                    teamManager.UpdateTeamsFromLobby(_currentLobby);
                }
                Debug.Log("[LobbyStateManager] Lobby joined");
            }
            else
            {
                Debug.LogError($"[LobbyStateManager] Failed to join lobby: {result}");
            }

            OnLobbyJoined?.Invoke(result);
        }

        private void OnLobbyLeftComplete(Result result)
        {
            if (result == Result.Success)
            {
                _currentLobby = null;
                if (_teamManager is TeamManager teamManager)
                {
                    teamManager.Clear();
                }
                Debug.Log("[LobbyStateManager] Lobby left");
            }
            else
            {
                Debug.LogError($"[LobbyStateManager] Failed to leave lobby: {result}");
            }

            OnLobbyLeft?.Invoke(result);
        }
    }
}
