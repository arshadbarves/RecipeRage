using System.Collections.Generic;
using Core.Networking.Common;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Handles lobby search operations
    /// </summary>
    public class LobbySearchService
    {
        private readonly EOSLobbyManager _eosLobbyManager;

        public LobbySearchService(EOSLobbyManager eosLobbyManager)
        {
            _eosLobbyManager = eosLobbyManager;
        }

        public void SearchByGameMode(GameMode gameMode, System.Action<Result> callback)
        {
            _eosLobbyManager.SearchByAttribute("GameMode", gameMode.ToString(), (result) =>
            {
                if (result == Result.Success)
                {
                    var searchResults = _eosLobbyManager.GetSearchResults();
                    Debug.Log($"[LobbySearchService] Found {searchResults.Count} lobbies");
                }
                callback?.Invoke(result);
            });
        }

        public Dictionary<Lobby, LobbyDetails> GetSearchResults()
        {
            return _eosLobbyManager.GetSearchResults();
        }
    }
}
