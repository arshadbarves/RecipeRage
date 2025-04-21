using System;
using System.Collections.Generic;
using Core.Networking.Common;
using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Handles game-specific network messages.
    /// </summary>
    public class NetworkMessageHandler : MonoBehaviour
    {
        // Singleton instance
        public static NetworkMessageHandler Instance { get; private set; }
        
        // Reference to the network manager
        private RecipeRageNetworkManager _networkManager;
        
        // Events for game systems
        public event Action<PlayerInfo, PlayerAction> OnPlayerActionReceived;
        public event Action<PlayerInfo, string> OnChatMessageReceived;
        public event Action<PlayerInfo, int> OnEmoteReceived;
        public event Action<TeamId, int> OnTeamScoreUpdated;
        public event Action<Dictionary<string, object>> OnGameStateUpdated;
        public event Action<string, Vector3, Quaternion> OnObjectStateUpdated;
        public event Action<string, Vector3> OnIngredientSpawned;
        public event Action<string, string> OnIngredientPickedUp;
        public event Action<string, Vector3> OnIngredientDropped;
        public event Action<string, string> OnIngredientProcessed;
        public event Action<string, int> OnRecipeCompleted;
        public event Action<string, string> OnRecipeFailed;
        
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        private void Start()
        {
            // Get the network manager
            _networkManager = RecipeRageNetworkManager.Instance;
            
            if (_networkManager == null)
            {
                Debug.LogError("[NetworkMessageHandler] RecipeRageNetworkManager instance not found");
                return;
            }
            
            // Subscribe to P2P events
            _networkManager.P2PManager.OnPlayerActionReceived += HandlePlayerAction;
            _networkManager.P2PManager.OnChatMessageReceived += HandleChatMessage;
            _networkManager.P2PManager.OnEmoteReceived += HandleEmote;
            
            Debug.Log("[NetworkMessageHandler] Initialized");
        }
        
        /// <summary>
        /// Send a player action.
        /// </summary>
        /// <param name="action">The player action</param>
        public void SendPlayerAction(PlayerAction action)
        {
            _networkManager.P2PManager.SendPlayerAction(action);
        }
        
        /// <summary>
        /// Send a chat message.
        /// </summary>
        /// <param name="message">The chat message</param>
        public void SendChatMessage(string message)
        {
            _networkManager.P2PManager.SendChatMessage(message);
        }
        
        /// <summary>
        /// Send an emote.
        /// </summary>
        /// <param name="emoteId">The emote ID</param>
        public void SendEmote(int emoteId)
        {
            _networkManager.P2PManager.SendEmote(emoteId);
        }
        
        /// <summary>
        /// Update a team's score.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="score">The new score</param>
        public void UpdateTeamScore(TeamId teamId, int score)
        {
            // Create the score data
            byte[] scoreData = new byte[8];
            BitConverter.GetBytes((int)teamId).CopyTo(scoreData, 0);
            BitConverter.GetBytes(score).CopyTo(scoreData, 4);
            
            // Send to all players
            _networkManager.P2PManager.SendToAll(NetworkMessageType.TeamScore, scoreData);
            
            // Update local state
            OnTeamScoreUpdated?.Invoke(teamId, score);
        }
        
        /// <summary>
        /// Spawn an ingredient.
        /// </summary>
        /// <param name="ingredientId">The ingredient ID</param>
        /// <param name="position">The position</param>
        public void SpawnIngredient(string ingredientId, Vector3 position)
        {
            // Create the spawn data
            byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(ingredientId);
            byte[] posBytes = new byte[12];
            BitConverter.GetBytes(position.x).CopyTo(posBytes, 0);
            BitConverter.GetBytes(position.y).CopyTo(posBytes, 4);
            BitConverter.GetBytes(position.z).CopyTo(posBytes, 8);
            
            byte[] spawnData = new byte[idBytes.Length + posBytes.Length + 4];
            BitConverter.GetBytes(idBytes.Length).CopyTo(spawnData, 0);
            idBytes.CopyTo(spawnData, 4);
            posBytes.CopyTo(spawnData, 4 + idBytes.Length);
            
            // Send to all players
            _networkManager.P2PManager.SendToAll(NetworkMessageType.IngredientSpawned, spawnData);
            
            // Update local state
            OnIngredientSpawned?.Invoke(ingredientId, position);
        }
        
        /// <summary>
        /// Pick up an ingredient.
        /// </summary>
        /// <param name="ingredientId">The ingredient ID</param>
        /// <param name="playerId">The player ID</param>
        public void PickUpIngredient(string ingredientId, string playerId)
        {
            // Create the pickup data
            byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(ingredientId);
            byte[] playerBytes = System.Text.Encoding.UTF8.GetBytes(playerId);
            
            byte[] pickupData = new byte[idBytes.Length + playerBytes.Length + 8];
            BitConverter.GetBytes(idBytes.Length).CopyTo(pickupData, 0);
            BitConverter.GetBytes(playerBytes.Length).CopyTo(pickupData, 4);
            idBytes.CopyTo(pickupData, 8);
            playerBytes.CopyTo(pickupData, 8 + idBytes.Length);
            
            // Send to all players
            _networkManager.P2PManager.SendToAll(NetworkMessageType.IngredientPickedUp, pickupData);
            
            // Update local state
            OnIngredientPickedUp?.Invoke(ingredientId, playerId);
        }
        
        /// <summary>
        /// Drop an ingredient.
        /// </summary>
        /// <param name="ingredientId">The ingredient ID</param>
        /// <param name="position">The position</param>
        public void DropIngredient(string ingredientId, Vector3 position)
        {
            // Create the drop data
            byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(ingredientId);
            byte[] posBytes = new byte[12];
            BitConverter.GetBytes(position.x).CopyTo(posBytes, 0);
            BitConverter.GetBytes(position.y).CopyTo(posBytes, 4);
            BitConverter.GetBytes(position.z).CopyTo(posBytes, 8);
            
            byte[] dropData = new byte[idBytes.Length + posBytes.Length + 4];
            BitConverter.GetBytes(idBytes.Length).CopyTo(dropData, 0);
            idBytes.CopyTo(dropData, 4);
            posBytes.CopyTo(dropData, 4 + idBytes.Length);
            
            // Send to all players
            _networkManager.P2PManager.SendToAll(NetworkMessageType.IngredientDropped, dropData);
            
            // Update local state
            OnIngredientDropped?.Invoke(ingredientId, position);
        }
        
        /// <summary>
        /// Complete a recipe.
        /// </summary>
        /// <param name="recipeId">The recipe ID</param>
        /// <param name="score">The score</param>
        public void CompleteRecipe(string recipeId, int score)
        {
            // Create the complete data
            byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(recipeId);
            
            byte[] completeData = new byte[idBytes.Length + 8];
            BitConverter.GetBytes(idBytes.Length).CopyTo(completeData, 0);
            BitConverter.GetBytes(score).CopyTo(completeData, 4);
            idBytes.CopyTo(completeData, 8);
            
            // Send to all players
            _networkManager.P2PManager.SendToAll(NetworkMessageType.RecipeCompleted, completeData);
            
            // Update local state
            OnRecipeCompleted?.Invoke(recipeId, score);
        }
        
        /// <summary>
        /// Handle a player action.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="action">The action</param>
        private void HandlePlayerAction(PlayerInfo sender, PlayerAction action)
        {
            // Forward to game systems
            OnPlayerActionReceived?.Invoke(sender, action);
        }
        
        /// <summary>
        /// Handle a chat message.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="message">The message</param>
        private void HandleChatMessage(PlayerInfo sender, string message)
        {
            // Forward to game systems
            OnChatMessageReceived?.Invoke(sender, message);
        }
        
        /// <summary>
        /// Handle an emote.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="emoteId">The emote ID</param>
        private void HandleEmote(PlayerInfo sender, int emoteId)
        {
            // Forward to game systems
            OnEmoteReceived?.Invoke(sender, emoteId);
        }
        
        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            // Unsubscribe from events
            if (_networkManager != null && _networkManager.P2PManager != null)
            {
                _networkManager.P2PManager.OnPlayerActionReceived -= HandlePlayerAction;
                _networkManager.P2PManager.OnChatMessageReceived -= HandleChatMessage;
                _networkManager.P2PManager.OnEmoteReceived -= HandleEmote;
            }
        }
    }
}
