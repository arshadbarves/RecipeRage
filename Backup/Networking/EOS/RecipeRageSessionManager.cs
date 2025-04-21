using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.GameModes;
using UnityEngine;

namespace RecipeRage.Core.Networking.EOS
{
    /// <summary>
    /// Manages game sessions using the EOS Sessions interface.
    /// Extends the functionality of the EOSSessionsManager from the EOS samples.
    /// </summary>
    public class RecipeRageSessionManager : MonoBehaviour
    {
        // Reference to the EOS Sessions Manager
        private EOSSessionsManager _eosSessionsManager;
        
        // Game-specific session properties
        private GameMode _currentGameMode;
        private string _currentMapName;
        private bool _isHost;
        
        // Events
        public event Action<Result> OnSessionCreated;
        public event Action<Result> OnSessionJoined;
        public event Action<Result> OnSessionLeft;
        public event Action<NetworkPlayer> OnPlayerJoined;
        public event Action<NetworkPlayer> OnPlayerLeft;
        
        /// <summary>
        /// Initialize the session manager.
        /// </summary>
        public void Initialize()
        {
            // Get the EOS Sessions Manager
            _eosSessionsManager = EOSManager.Instance.GetComponent<EOSSessionsManager>();
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not found!");
                return;
            }
            
            Debug.Log("[RecipeRageSessionManager] Initialized");
        }
        
        /// <summary>
        /// Create a game session.
        /// </summary>
        /// <param name="sessionName">The session name</param>
        /// <param name="gameMode">The game mode</param>
        /// <param name="mapName">The map name</param>
        public void CreateGameSession(string sessionName, GameMode gameMode, string mapName)
        {
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not initialized!");
                return;
            }
            
            _currentGameMode = gameMode;
            _currentMapName = mapName;
            _isHost = true;
            
            // Create a session using the EOS sample code
            Session session = new Session();
            session.Name = sessionName;
            session.MaxPlayers = (uint)gameMode.MaxPlayers;
            session.AllowJoinInProgress = gameMode.AllowJoinInProgress;
            
            // Add game-specific attributes
            AddGameModeAttributes(session, gameMode);
            AddMapAttribute(session, mapName);
            
            // Use the EOS sample code to create the session
            _eosSessionsManager.CreateSession(session, OnSessionCreationComplete);
            
            Debug.Log($"[RecipeRageSessionManager] Creating session: {sessionName}, Game Mode: {gameMode.DisplayName}, Map: {mapName}");
        }
        
        /// <summary>
        /// Join a session by ID.
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        public void JoinSession(string sessionId)
        {
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not initialized!");
                return;
            }
            
            _isHost = false;
            
            // Use the EOS sample code to join the session
            _eosSessionsManager.JoinSession(sessionId, OnSessionJoinComplete);
            
            Debug.Log($"[RecipeRageSessionManager] Joining session: {sessionId}");
        }
        
        /// <summary>
        /// Leave the current session.
        /// </summary>
        public void LeaveSession()
        {
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not initialized!");
                return;
            }
            
            // Use the EOS sample code to leave the session
            _eosSessionsManager.LeaveSession(OnSessionLeftComplete);
            
            Debug.Log("[RecipeRageSessionManager] Leaving session");
        }
        
        /// <summary>
        /// Search for sessions.
        /// </summary>
        /// <param name="callback">Callback to invoke with the results</param>
        public void FindSessions(Action<List<NetworkSessionInfo>> callback)
        {
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not initialized!");
                callback?.Invoke(new List<NetworkSessionInfo>());
                return;
            }
            
            // Use the EOS sample code to search for sessions
            _eosSessionsManager.FindSessions(results => 
            {
                // Convert EOS sessions to our NetworkSessionInfo format
                List<NetworkSessionInfo> sessionInfos = new List<NetworkSessionInfo>();
                foreach (var kvp in results.GetResults())
                {
                    Session session = kvp.Key;
                    
                    NetworkSessionInfo sessionInfo = new NetworkSessionInfo
                    {
                        SessionId = session.Id,
                        SessionName = session.Name,
                        PlayerCount = (int)session.NumConnections,
                        MaxPlayers = (int)session.MaxPlayers,
                        IsPrivate = session.PermissionLevel != OnlineSessionPermissionLevel.PublicAdvertised,
                        HostName = GetHostName(session),
                        GameMode = GetGameModeId(session),
                        MapName = GetMapName(session)
                    };
                    
                    sessionInfos.Add(sessionInfo);
                }
                
                callback?.Invoke(sessionInfos);
            });
            
            Debug.Log("[RecipeRageSessionManager] Searching for sessions");
        }
        
        /// <summary>
        /// Get the current session info.
        /// </summary>
        /// <returns>The current session info, or null if not in a session</returns>
        public NetworkSessionInfo GetCurrentSessionInfo()
        {
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not initialized!");
                return null;
            }
            
            // Get the current session from the EOS sample code
            Session currentSession = _eosSessionsManager.GetCurrentSession();
            if (currentSession == null || !currentSession.IsValid())
            {
                return null;
            }
            
            NetworkSessionInfo sessionInfo = new NetworkSessionInfo
            {
                SessionId = currentSession.Id,
                SessionName = currentSession.Name,
                PlayerCount = (int)currentSession.NumConnections,
                MaxPlayers = (int)currentSession.MaxPlayers,
                IsPrivate = currentSession.PermissionLevel != OnlineSessionPermissionLevel.PublicAdvertised,
                HostName = GetHostName(currentSession),
                GameMode = GetGameModeId(currentSession),
                MapName = GetMapName(currentSession)
            };
            
            return sessionInfo;
        }
        
        /// <summary>
        /// Get the list of players in the current session.
        /// </summary>
        /// <returns>The list of players, or an empty list if not in a session</returns>
        public List<NetworkPlayer> GetPlayers()
        {
            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not initialized!");
                return new List<NetworkPlayer>();
            }
            
            // Get the current session from the EOS sample code
            Session currentSession = _eosSessionsManager.GetCurrentSession();
            if (currentSession == null || !currentSession.IsValid())
            {
                return new List<NetworkPlayer>();
            }
            
            // Get the players from the session
            List<NetworkPlayer> players = new List<NetworkPlayer>();
            
            // TODO: Implement this when we have the player list from EOS
            // This will require extending the EOS sample code to expose the player list
            
            return players;
        }
        
        /// <summary>
        /// Check if the local player is the host.
        /// </summary>
        /// <returns>True if the local player is the host, false otherwise</returns>
        public bool IsHost()
        {
            return _isHost;
        }
        
        #region Private Methods
        
        /// <summary>
        /// Add game mode attributes to a session.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="gameMode">The game mode</param>
        private void AddGameModeAttributes(Session session, GameMode gameMode)
        {
            // Add game mode ID
            SessionAttribute gameModeIdAttr = new SessionAttribute();
            gameModeIdAttr.Key = "GameModeId";
            gameModeIdAttr.ValueType = AttributeType.String;
            gameModeIdAttr.AsString = gameMode.Id;
            gameModeIdAttr.Advertisement = SessionAttributeAdvertisementType.Advertise;
            session.Attributes.Add(gameModeIdAttr);
            
            // Add game mode name
            SessionAttribute gameModeNameAttr = new SessionAttribute();
            gameModeNameAttr.Key = "GameModeName";
            gameModeNameAttr.ValueType = AttributeType.String;
            gameModeNameAttr.AsString = gameMode.DisplayName;
            gameModeNameAttr.Advertisement = SessionAttributeAdvertisementType.Advertise;
            session.Attributes.Add(gameModeNameAttr);
            
            // Add team count
            SessionAttribute teamCountAttr = new SessionAttribute();
            teamCountAttr.Key = "TeamCount";
            teamCountAttr.ValueType = AttributeType.Int64;
            teamCountAttr.AsInt64 = gameMode.TeamCount;
            teamCountAttr.Advertisement = SessionAttributeAdvertisementType.Advertise;
            session.Attributes.Add(teamCountAttr);
            
            // Add players per team
            SessionAttribute playersPerTeamAttr = new SessionAttribute();
            playersPerTeamAttr.Key = "PlayersPerTeam";
            playersPerTeamAttr.ValueType = AttributeType.Int64;
            playersPerTeamAttr.AsInt64 = gameMode.PlayersPerTeam;
            playersPerTeamAttr.Advertisement = SessionAttributeAdvertisementType.Advertise;
            session.Attributes.Add(playersPerTeamAttr);
        }
        
        /// <summary>
        /// Add map attribute to a session.
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="mapName">The map name</param>
        private void AddMapAttribute(Session session, string mapName)
        {
            SessionAttribute mapNameAttr = new SessionAttribute();
            mapNameAttr.Key = "MapName";
            mapNameAttr.ValueType = AttributeType.String;
            mapNameAttr.AsString = mapName;
            mapNameAttr.Advertisement = SessionAttributeAdvertisementType.Advertise;
            session.Attributes.Add(mapNameAttr);
        }
        
        /// <summary>
        /// Get the host name from a session.
        /// </summary>
        /// <param name="session">The session</param>
        /// <returns>The host name, or an empty string if not found</returns>
        private string GetHostName(Session session)
        {
            foreach (SessionAttribute attr in session.Attributes)
            {
                if (attr.Key == "HostName" && attr.ValueType == AttributeType.String)
                {
                    return attr.AsString;
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Get the game mode ID from a session.
        /// </summary>
        /// <param name="session">The session</param>
        /// <returns>The game mode ID, or an empty string if not found</returns>
        private string GetGameModeId(Session session)
        {
            foreach (SessionAttribute attr in session.Attributes)
            {
                if (attr.Key == "GameModeId" && attr.ValueType == AttributeType.String)
                {
                    return attr.AsString;
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Get the map name from a session.
        /// </summary>
        /// <param name="session">The session</param>
        /// <returns>The map name, or an empty string if not found</returns>
        private string GetMapName(Session session)
        {
            foreach (SessionAttribute attr in session.Attributes)
            {
                if (attr.Key == "MapName" && attr.ValueType == AttributeType.String)
                {
                    return attr.AsString;
                }
            }
            
            return string.Empty;
        }
        
        #endregion
        
        #region Callbacks
        
        /// <summary>
        /// Callback for session creation completion.
        /// </summary>
        /// <param name="info">Callback info</param>
        private void OnSessionCreationComplete(SessionsManagerCreateSessionCallbackInfo info)
        {
            if (info.ResultCode == Result.Success)
            {
                Debug.Log($"[RecipeRageSessionManager] Session created successfully: {info.SessionToCreateName}");
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to create session: {info.ResultCode}");
                _isHost = false;
            }
            
            OnSessionCreated?.Invoke(info.ResultCode);
        }
        
        /// <summary>
        /// Callback for session join completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnSessionJoinComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageSessionManager] Session joined successfully");
                
                // Get the current session info to update our local state
                Session currentSession = _eosSessionsManager.GetCurrentSession();
                if (currentSession != null && currentSession.IsValid())
                {
                    _currentGameMode = GameModeManager.Instance.GetGameMode(GetGameModeId(currentSession));
                    _currentMapName = GetMapName(currentSession);
                }
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to join session: {result}");
            }
            
            OnSessionJoined?.Invoke(result);
        }
        
        /// <summary>
        /// Callback for session leave completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnSessionLeftComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageSessionManager] Session left successfully");
                _isHost = false;
                _currentGameMode = null;
                _currentMapName = null;
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to leave session: {result}");
            }
            
            OnSessionLeft?.Invoke(result);
        }
        
        #endregion
    }
}
