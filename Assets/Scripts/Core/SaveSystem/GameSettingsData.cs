using System;
using UnityEngine;

namespace Core.SaveSystem
{
    /// <summary>
    /// Data class for game settings.
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
        public bool Fullscreen = true;
        public string Resolution = "1920x1080";
        public string QualityLevel = "High";

        [Range(0f, 1f)]
        public float Brightness = 0.5f;

        public bool VSync = true;

        // Gameplay settings
        [Header("Gameplay Settings")]
        [Range(0f, 1f)]
        public float Sensitivity = 0.65f;

        public bool InvertY = false;
        public bool CameraShake = true;
        public bool AutoPickup = false;
        public bool Vibration = true;
        public bool ShowTutorial = true;
        public bool Notifications = true;

        // Authentication
        [Header("Authentication")]
        public string LastLoginMethod = "";
    }
}
