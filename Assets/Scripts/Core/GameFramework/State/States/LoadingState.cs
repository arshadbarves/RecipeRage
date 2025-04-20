using System;
using UnityEngine;

namespace Core.GameFramework.State.States
{
    /// <summary>
    /// State for loading game assets and initializing systems.
    /// </summary>
    public class LoadingState : IState
    {
        /// <summary>
        /// Name of the state for debugging.
        /// </summary>
        public string StateName => GetType().Name;
        /// <summary>
        /// Event triggered when loading is complete.
        /// </summary>
        public event Action OnLoadingComplete;

        /// <summary>
        /// Flag to track if loading is complete.
        /// </summary>
        private bool _isLoadingComplete;

        /// <summary>
        /// Simulated loading progress (0-1).
        /// </summary>
        private float _loadingProgress;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log($"[{StateName}] Entered");

            // Reset loading state
            _isLoadingComplete = false;
            _loadingProgress = 0f;

            // Start loading process
            Debug.Log("[LoadingState] Starting loading process");

            // In a real implementation, you would start loading assets and initializing systems here
            // For now, we'll simulate loading with a delay

            // TODO: Implement actual asset loading and system initialization
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public void Exit()
        {
            Debug.Log($"[{StateName}] Exited");

            // Clean up any loading resources if needed
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public void Update()
        {
            // If loading is already complete, do nothing
            if (_isLoadingComplete)
            {
                return;
            }

            // Simulate loading progress
            _loadingProgress += Time.deltaTime * 0.5f; // Adjust speed as needed

            // Clamp progress to 0-1 range
            _loadingProgress = Mathf.Clamp01(_loadingProgress);

            // Log progress occasionally
            if (Mathf.Approximately(_loadingProgress * 10, Mathf.Floor(_loadingProgress * 10)))
            {
                Debug.Log($"[LoadingState] Loading progress: {_loadingProgress * 100:F0}%");
            }

            // Check if loading is complete
            if (_loadingProgress >= 1.0f)
            {
                CompleteLoading();
            }
        }

        /// <summary>
        /// Called when loading is complete.
        /// </summary>
        private void CompleteLoading()
        {
            if (_isLoadingComplete)
            {
                return;
            }

            _isLoadingComplete = true;
            Debug.Log("[LoadingState] Loading complete");

            // Trigger the loading complete event
            OnLoadingComplete?.Invoke();
        }

        /// <summary>
        /// Gets the current loading progress (0-1).
        /// </summary>
        /// <returns>Loading progress between 0 and 1</returns>
        public float GetLoadingProgress()
        {
            return _loadingProgress;
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public void FixedUpdate()
        {
            // Loading state doesn't need physics updates
        }
    }
}
