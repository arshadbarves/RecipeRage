using System;
using UnityEngine;

namespace Core.SaveSystem
{
    /// <summary>
    /// Data class for game settings.
    /// Unified naming convention for all settings
    /// </summary>
    [Serializable]
    public class GameSettingsData
    {
        // Audio settings
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float MasterVolume = 0.8f;

        [Range(0f, 1f)]
        public float MusicVolume = 0.7f;

        [Range(0f, 1f)]
        public float SFXVolume = 0.9f;

        [Range(0f, 1f)]
        public float VoiceVolume = 0.85f;

        public bool IsMuted = false;

        // Graphics settings
        [Header("Graphics Settings")]
        public bool IsFullscreen = true;
        public int ResolutionIndex = 0;
        public int GraphicsQuality = 2; // 0=Low, 1=Medium, 2=High

        [Range(0f, 1f)]
        public float Brightness = 0.5f;

        public bool IsVSyncEnabled = true;
        public bool ShowFPS = false;

        // Gameplay settings
        [Header("Gameplay Settings")]
        [Range(0f, 1f)]
        public float Sensitivity = 0.65f;

        public bool InvertY = false;
        public bool CameraShake = true;
        public bool AutoPickup = false;
        public bool IsVibrationEnabled = true;
        public bool ShowTutorials = true;
        public bool NotificationsEnabled = true;
        public int LanguageIndex = 0; // 0=English, 1=Spanish, etc.

        // Authentication
        [Header("Authentication")]
        public string LastLoginMethod = "";
    }
}
