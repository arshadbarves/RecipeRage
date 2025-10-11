using System;
using UnityEngine;

namespace Core.State.States
{
    /// <summary>
    /// State for loading game assets and initializing systems.
    /// </summary>
    public class LoadingState : BaseState
    {
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
        public override void Enter()
        {
            base.Enter();

            // Reset loading state
            _isLoadingComplete = false;
            _loadingProgress = 0f;

            // Start loading process
            LogMessage("Starting loading process");

            // In a real implementation, you would start loading assets and initializing systems here
            // For now, we'll simulate loading with a delay

            // TODO: Implement actual asset loading and system initialization
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Clean up any loading resources if needed
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
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
                LogMessage($"Loading progress: {_loadingProgress * 100:F0}%");
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
            LogMessage("Loading complete");

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
        public override void FixedUpdate()
        {
            // Loading state doesn't need physics updates
        }
    }
}
