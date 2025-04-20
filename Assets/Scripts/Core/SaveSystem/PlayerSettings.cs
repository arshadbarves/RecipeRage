using System;
using
namespace Core.SaveSystem
{
    /// <summary>
    /// Stores player settings that persist between game sessions.
    /// </summary>
    [Serializable]
    public class PlayerSettings
    {
        #region Audio Settings

        /// <summary>
        /// Master volume (0-1).
        /// </summary>
        [Range(0f, 1f)]
        public float MasterVolume = 0.8f;

        /// <summary>
        /// Music volume (0-1).
        /// </summary>
        [Range(0f, 1f)]
        public float MusicVolume = 0.7f;

        /// <summary>
        /// Sound effects volume (0-1).
        /// </summary>
        [Range(0f, 1f)]
        public float SfxVolume = 0.9f;

        /// <summary>
        /// Whether audio is muted.
        /// 
        /// <summary>
        /// Whether the game is in fullscreen mode.
        /// </summary>
        public bool Fullscreen = true;

        /// <summary>
        /// Quality level index.
        /// </summary>
        public int QualityLevel = 2;

        /// <summary>
        /// Resolution index.
        /// </summary>
        public int ResolutionIndex = 1;

        #endregion

        #region Gameplay Settings

        /// <summary>
        /// Whether camera shake is 
        axis is inverted.

        /// <summary>
        /// Whether vibration is enabled.
        /// </summary>
        public bool Vibration = true;

        /// <summary>
        /// Mouse/touch sensitivity (0-100).
        /// </summary>
        [Range(0f, 100f)]
        public float Sensitivity = 65f;

        /// <summary>
        /// Whether to show tutorials.
        /// </summary>
        public bool ShowTutorial = true;

        #endregion

        #region Player Info

        /// <summary>
        /// Player name.
        /// </summary>
        public string PlayerName = "Player";

        #endregion

        /// <summary>
        /// Apply settings to the game.
        /// </summary>
        public void Apply()
        {
            // Apply graphics settings
            Screen.fullScreen = Fullscreen;

            Audio