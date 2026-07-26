using System;
using UnityEngine;

namespace KitchenClash.Application.Models
{
    [Serializable]
    public class GameSettingsData
    {
        [Range(0f, 1f)] public float MasterVolume = 0.8f;
        [Range(0f, 1f)] public float MusicVolume = 0.7f;
        [Range(0f, 1f)] public float SFXVolume = 0.9f;
        public bool IsMuted = false;
        public int ResolutionIndex = 0;
        public int GraphicsQuality = 2;
        public bool VibrationEnabled = true;
        public bool NotificationsEnabled = true;
        public string LanguageCode = "English";
        public string LastLoginMethod = "";
        [Range(0.1f, 3f)] public float ControlsSensitivity = 1f;
    }
}
