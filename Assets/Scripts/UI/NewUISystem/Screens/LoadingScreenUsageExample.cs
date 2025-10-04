using UnityEngine;
using UI.UISystem.Screens;

namespace UI.UISystem.Examples
{
    /// <summary>
    /// Example usage of the customizable LoadingScreen UpdateProgress methods
    /// </summary>
    public class LoadingScreenUsageExample : MonoBehaviour
    {
        private LoadingScreen _loadingScreen;

        private void Start()
        {
            // Get reference to loading screen
            _loadingScreen = UIManager.Instance.GetScreen<LoadingScreen>();
        }

        /// <summary>
        /// Example: Update both status and progress
        /// </summary>
        public void UpdateBoth()
        {
            _loadingScreen.UpdateProgress("Loading assets...", 0.5f);
        }

        /// <summary>
        /// Example: Update only the status text (progress bar stays the same)
        /// </summary>
        public void UpdateStatusOnly()
        {
            _loadingScreen.UpdateStatus("Connecting to server...");
            // or
            _loadingScreen.UpdateProgress(status: "Connecting to server...");
        }

        /// <summary>
        /// Example: Update only the progress value (status text stays the same)
        /// </summary>
        public void UpdateProgressOnly()
        {
            _loadingScreen.UpdateProgressValue(0.75f);
            // or
            _loadingScreen.UpdateProgress(progress: 0.75f);
        }

        /// <summary>
        /// Example: Configure minimum durations
        /// </summary>
        public void ConfigureLoadingScreen()
        {
            _loadingScreen
                .SetMinimumDuration(2f)        // Loading must take at least 2 seconds
                .SetMinimumShowDuration(1.5f)  // Screen must be visible for at least 1.5 seconds
                .SetHideDelay(0.5f);           // Wait 0.5s after completion before hiding
        }

        /// <summary>
        /// Example: Fast loading (will still show for MinShowDuration)
        /// </summary>
        public async void FastLoadingExample()
        {
            _loadingScreen.SetMinimumShowDuration(2f); // Ensure visible for 2 seconds minimum
            _loadingScreen.StartLoading();

            // Even if this completes in 0.5 seconds, screen will stay visible for 2 seconds
            _loadingScreen.UpdateProgress("Loading...", 0.5f);
            await System.Threading.Tasks.Task.Delay(500);
            _loadingScreen.UpdateProgress("Complete!", 1.0f);
        }

        /// <summary>
        /// Example: Typical loading sequence
        /// </summary>
        public async void LoadGameSequence()
        {
            _loadingScreen.StartLoading();

            // Phase 1: Load assets
            _loadingScreen.UpdateProgress("Loading assets...", 0.2f);
            await System.Threading.Tasks.Task.Delay(1000);

            // Phase 2: Initialize systems (only update status)
            _loadingScreen.UpdateStatus("Initializing systems...");
            await System.Threading.Tasks.Task.Delay(500);

            // Phase 3: Update progress without changing status
            _loadingScreen.UpdateProgressValue(0.6f);
            await System.Threading.Tasks.Task.Delay(500);

            // Phase 4: Connect to server
            _loadingScreen.UpdateProgress("Connecting to server...", 0.8f);
            await System.Threading.Tasks.Task.Delay(1000);

            // Phase 5: Complete
            _loadingScreen.UpdateProgress("Ready!", 1.0f);
        }
    }
}
