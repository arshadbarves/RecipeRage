using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.Networking.Common;
using UnityEngine;

namespace RecipeRage.Core.Networking.EOS
{
    /// <summary>
    /// Wrapper for EOSSessionsManager that provides game-specific session functionality.
    /// </summary>
    public class RecipeRageSessionManager : MonoBehaviour
    {

        // Cached list of available sessions
        private readonly List<GameSessionInfo> _availableSessions = new List<GameSessionInfo>();

        // Current session information
        private GameSessionInfo _currentSession;
        // Reference to the EOS Sessions Manager
        private EOSSessionsManager _eosSessionsManager;

        // Events
        public event Action<Result> OnSessionCreated;
        public event Action<Result> OnSessionJoined;
        public event Action<Result> OnSessionLeft;
        public event Action<List<GameSessionInfo>> OnSessionsFound;

        /// <summary>
        /// Initialize the session manager.
        /// </summary>
        public void Initialize()
        {
            // Get the EOS Sessions Manager from the EOSManager
            _eosSessionsManager = EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>();

            if (_eosSessionsManager == null)
            {
                Debug.LogError("[RecipeRageSessionManager] EOSSessionsManager not found on EOSManager");
                return;
            }

            Debug.Log("[RecipeRageSessionManager] Initialized");
        }

        /// <summary>
        /// Create a new game session.
        /// </summary>
        /// <param name="sessionName"> The name of the session </param>
        /// <param name="gameMode"> The game mode </param>
        /// <param name="mapName"> The map name </param>
        /// <param name="maxPlayers"> The maximum number of players </param>
        /// <param name="isPrivate"> Whether the session is private </param>
        public void CreateGameSession(string sessionName, GameMode gameMode, string mapName, int maxPlayers = 4, bool isPrivate = false)
        {
            // Create a new session
            var session = new Session();
            session.Name = sessionName;
            session.MaxPlayers = (uint)maxPlayers;
            session.PermissionLevel = isPrivate ?
                OnlineSessionPermissionLevel.InviteOnly :
                OnlineSessionPermissionLevel.PublicAdvertised;
            session.AllowJoinInProgress = true;
            session.InvitesAllowed = true;

            // Add game-specific attributes
            AddSessionAttribute(session, "GameMode", gameMode.ToString(), true);
            AddSessionAttribute(session, "MapName", mapName, true);
            AddSessionAttribute(session, "IsPrivate", isPrivate.ToString(), true);

            // Create the session
            _eosSessionsManager.CreateSession(session, OnSessionCreationComplete);

            Debug.Log($"[RecipeRageSessionManager] Creating session: {sessionName}, GameMode: {gameMode}, Map: {mapName}");
        }

        /// <summary>
        /// Join an existing game session.
        /// </summary>
        /// <param name="sessionId"> The ID of the session to join </param>
        public void JoinSession(string sessionId)
        {
            // Find the session in the available sessions
            if (_eosSessionsManager.GetCurrentSearch() != null)
            {
                Dictionary<Session, SessionDetails> results = _eosSessionsManager.GetCurrentSearch().GetResults();

                foreach (KeyValuePair<Session, SessionDetails> kvp in results)
                {
                    if (kvp.Key.Id == sessionId)
                    {
                        // Join the session
                        _eosSessionsManager.JoinSession(kvp.Value, true, OnSessionJoinComplete);

                        Debug.Log($"[RecipeRageSessionManager] Joining session: {sessionId}");
                        return;
                    }
                }
            }

            Debug.LogError($"[RecipeRageSessionManager] Session not found: {sessionId}");
            OnSessionJoined?.Invoke(Result.NotFound);
        }

        /// <summary>
        /// Leave the current session.
        /// </summary>
        public void LeaveSession()
        {
            // Get the current session
            if (_eosSessionsManager.TryGetSession(_currentSession?.SessionId, out var session))
            {
                // Leave the session
                _eosSessionsManager.LeaveSession(session.Name, OnSessionLeftComplete);

                Debug.Log($"[RecipeRageSessionManager] Leaving session: {_currentSession?.SessionId}");
            }
            else
            {
                Debug.LogError("[RecipeRageSessionManager] No current session to leave");
                OnSessionLeft?.Invoke(Result.NotFound);
            }
        }

        /// <summary>
        /// Find available sessions.
        /// </summary>
        public void FindSessions()
        {
            // Create a search for sessions
            _eosSessionsManager.FindSessions(new List<SessionAttribute>(), OnSessionsFoundComplete);

            Debug.Log("[RecipeRageSessionManager] Finding sessions");
        }

        /// <summary>
        /// Get the current session.
        /// </summary>
        /// <returns> The current session info </returns>
        public GameSessionInfo GetCurrentSession()
        {
            return _currentSession;
        }

        /// <summary>
        /// Get the available sessions.
        /// </summary>
        /// <returns> The list of available sessions </returns>
        public List<GameSessionInfo> GetAvailableSessions()
        {
            return _availableSessions;
        }

        /// <summary>
        /// Add a session attribute to a session.
        /// </summary>
        /// <param name="session"> The session </param>
        /// <param name="key"> The attribute key </param>
        /// <param name="value"> The attribute value </param>
        /// <param name="advertise"> Whether to advertise the attribute </param>
        private void AddSessionAttribute(Session session, string key, string value, bool advertise)
        {
            var attribute = new SessionAttribute();
            attribute.Key = key;
            attribute.ValueType = AttributeType.String;
            attribute.AsString = value;
            attribute.Advertisement = advertise ?
                SessionAttributeAdvertisementType.Advertise :
                SessionAttributeAdvertisementType.DontAdvertise;

            session.Attributes.Add(attribute);
        }

        /// <summary>
        /// Convert a Session to a GameSessionInfo.
        /// </summary>
        /// <param name="session"> The session </param>
        /// <returns> The game session info </returns>
        private GameSessionInfo ConvertToGameSessionInfo(Session session)
        {
            var info = new GameSessionInfo();
            info.SessionId = session.Id;
            info.SessionName = session.Name;
            info.PlayerCount = (int)session.NumConnections;
            info.MaxPlayers = (int)session.MaxPlayers;
            info.IsPrivate = session.PermissionLevel == OnlineSessionPermissionLevel.InviteOnly;

            // Extract attributes
            foreach (var attribute in session.Attributes)
            {
                switch (attribute.Key)
                {
                    case "GameMode":
                        if (Enum.TryParse(attribute.AsString, out GameMode gameMode))
                        {
                            info.GameMode = gameMode;
                        }
                        break;
                    case "MapName":
                        info.MapName = attribute.AsString;
                        break;
                    case "HostName":
                        info.HostName = attribute.AsString;
                        break;
                }
            }

            return info;
        }

        /// <summary>
        /// Callback for session creation.
        /// </summary>
        /// <param name="info"> The callback info </param>
        private void OnSessionCreationComplete(SessionsManagerCreateSessionCallbackInfo info)
        {
            if (info.ResultCode == Result.Success)
            {
                Debug.Log($"[RecipeRageSessionManager] Session created: {info.SessionToCreateName}");

                // Get the created session
                if (_eosSessionsManager.TryGetSession(info.SessionToCreateName, out var session))
                {
                    _currentSession = ConvertToGameSessionInfo(session);
                }
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to create session: {info.ResultCode}");
            }

            OnSessionCreated?.Invoke(info.ResultCode);
        }

        /// <summary>
        /// Callback for session join.
        /// </summary>
        /// <param name="result"> The result </param>
        private void OnSessionJoinComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageSessionManager] Session joined");

                // Get the joined session
                foreach (KeyValuePair<string, Session> kvp in _eosSessionsManager.GetCurrentSessions())
                {
                    var session = kvp.Value;
                    if (session.SessionState == OnlineSessionState.InProgress)
                    {
                        _currentSession = ConvertToGameSessionInfo(session);
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to join session: {result}");
            }

            OnSessionJoined?.Invoke(result);
        }

        /// <summary>
        /// Callback for session leave.
        /// </summary>
        /// <param name="result"> The result </param>
        private void OnSessionLeftComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageSessionManager] Session left");
                _currentSession = null;
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to leave session: {result}");
            }

            OnSessionLeft?.Invoke(result);
        }

        /// <summary>
        /// Callback for sessions found.
        /// </summary>
        /// <param name="result"> The result </param>
        private void OnSessionsFoundComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageSessionManager] Sessions found");

                // Clear the available sessions
                _availableSessions.Clear();

                // Get the search results
                if (_eosSessionsManager.GetCurrentSearch() != null)
                {
                    Dictionary<Session, SessionDetails> results = _eosSessionsManager.GetCurrentSearch().GetResults();

                    foreach (KeyValuePair<Session, SessionDetails> kvp in results)
                    {
                        _availableSessions.Add(ConvertToGameSessionInfo(kvp.Key));
                    }
                }

                Debug.Log($"[RecipeRageSessionManager] Found {_availableSessions.Count} sessions");
            }
            else
            {
                Debug.LogError($"[RecipeRageSessionManager] Failed to find sessions: {result}");
            }

            OnSessionsFound?.Invoke(_availableSessions);
        }
    }
}