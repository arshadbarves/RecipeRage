using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking.EOS;
using RecipeRage.Core.Patterns;
using UnityEngine;

namespace RecipeRage.Core.Networking.Common
{
    /// <summary>
    /// This file ensures that all necessary namespaces are properly imported.
    /// It doesn't contain any actual code.
    /// </summary>
    public static class NetworkNamespaces
    {
        // This method is never called, it's just here to ensure the namespaces are imported
        private static void EnsureNamespacesAreImported()
        {
            // Epic Online Services
            var result = Result.Success;
            var productUserId = new ProductUserId();
            
            // PlayEveryWare EOS Samples
            var eosManager = EOSManager.Instance;
            var session = new Session();
            var lobby = new Lobby();
            
            // RecipeRage
            var gameMode = new GameMode();
            var sessionManager = new RecipeRageSessionManager();
            var lobbyManager = new RecipeRageLobbyManager();
            var p2pManager = new RecipeRageP2PManager();
            var networkManager = RecipeRageNetworkManager.Instance;
            
            // Common
            var sessionInfo = new NetworkSessionInfo();
            var player = new NetworkPlayer();
            
            // Suppress unused variable warnings
            Debug.Log($"{result} {productUserId} {eosManager} {session} {lobby} {gameMode} {sessionManager} {lobbyManager} {p2pManager} {networkManager} {sessionInfo} {player}");
        }
    }
}
