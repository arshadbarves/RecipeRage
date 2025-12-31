using Gameplay.Camera;

namespace Gameplay
{
    /// <summary>
    /// Static context for gameplay-scoped systems.
    /// These systems only exist during active gameplay and are disposed when exiting gameplay state.
    /// 
    /// This approach:
    /// - Avoids polluting VContainer with gameplay-only systems
    /// - Provides clear lifecycle management (created in GameplayState.Enter, disposed in Exit)
    /// - Makes gameplay systems easily accessible without service registration overhead
    /// - Follows the pattern of OrderManager and ScoreManager in GameplayState
    /// </summary>
    public static class GameplayContext
    {
        /// <summary>
        /// Camera controller for gameplay camera management
        /// </summary>
        public static ICameraController CameraController { get; set; }

        /// <summary>
        /// Reset all gameplay context (called on GameplayState.Exit)
        /// </summary>
        public static void Reset()
        {
            CameraController = null;
        }
    }
}
