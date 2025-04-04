using System;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for matchmaking and lobby
    /// </summary>
    public class MatchmakingState : GameState
    {
        /// <summary>
        /// Event triggered when matchmaking is complete
        /// </summary>
        public event Action<bool> OnMatchmakingComplete;
        
        /// <summary>
        /// Whether matchmaking is in progress
        /// </summary>
        public bool IsMatchmakingInProgress { get; private set; }
        
        /// <summary>
        /// Time spent in matchmaking
        /// </summary>
        public float MatchmakingTime { get; private set; }
        
        /// <summary>
        /// Initialize the list of allowed state transitions
        /// </summary>
        protected override void InitializeAllowedTransitions()
        {
            AllowTransitionTo<MainMenuState>();
            AllowTransitionTo<LoadingState>();
            AllowTransitionTo<GameplayState>();
        }
        
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Reset matchmaking state
            IsMatchmakingInProgress = false;
            MatchmakingTime = 0f;
            
            // Show matchmaking UI
            Debug.Log("Showing matchmaking UI");
            
            // TODO: Implement matchmaking UI display
        }
        
        /// <summary>
        /// Called when the state is updated
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (IsMatchmakingInProgress)
            {
                // Update matchmaking time
                MatchmakingTime += Time.deltaTime;
                
                // TODO: Update matchmaking UI with time
            }
        }
        
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            
            // Cancel matchmaking if in progress
            if (IsMatchmakingInProgress)
            {
                CancelMatchmaking();
            }
            
            // Hide matchmaking UI
            Debug.Log("Hiding matchmaking UI");
            
            // TODO: Implement matchmaking UI hiding
        }
        
        /// <summary>
        /// Start the matchmaking process
        /// </summary>
        /// <param name="gameMode">The game mode to search for</param>
        /// <param name="region">The region to search in</param>
        public void StartMatchmaking(string gameMode, string region)
        {
            if (IsMatchmakingInProgress)
            {
                Debug.LogWarning("Matchmaking is already in progress.");
                return;
            }
            
            Debug.Log($"Starting matchmaking for game mode: {gameMode}, region: {region}");
            
            IsMatchmakingInProgress = true;
            MatchmakingTime = 0f;
            
            // TODO: Implement actual matchmaking logic with EOS
            // This would involve calling the EOS matchmaking service
            
            // For now, simulate matchmaking with a delay
            // In a real implementation, this would be replaced with actual EOS calls
            // and the OnMatchmakingComplete event would be triggered by the EOS callback
            
            // Simulate successful matchmaking after a delay
            // In a real implementation, this would be handled by EOS callbacks
            Debug.Log("Simulating matchmaking process...");
        }
        
        /// <summary>
        /// Cancel the current matchmaking process
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!IsMatchmakingInProgress)
            {
                return;
            }
            
            Debug.Log("Canceling matchmaking");
            
            IsMatchmakingInProgress = false;
            
            // TODO: Implement actual matchmaking cancellation with EOS
            
            // Notify listeners that matchmaking was canceled
            OnMatchmakingComplete?.Invoke(false);
        }
        
        /// <summary>
        /// Called when a match is found
        /// </summary>
        /// <param name="sessionId">The session ID of the match</param>
        public void OnMatchFound(string sessionId)
        {
            if (!IsMatchmakingInProgress)
            {
                Debug.LogWarning("Received match found event but matchmaking is not in progress.");
                return;
            }
            
            Debug.Log($"Match found! Session ID: {sessionId}");
            
            IsMatchmakingInProgress = false;
            
            // Notify listeners that matchmaking was successful
            OnMatchmakingComplete?.Invoke(true);
            
            // TODO: Store session information for the gameplay state
        }
    }
}
