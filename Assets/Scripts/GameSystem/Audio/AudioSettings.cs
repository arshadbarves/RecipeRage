using System;

namespace GameSystem.Audio
{
    [Serializable]
    public class AudioSettings
    {
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public float AmbientVolume { get; set; } = 1f;
        public float VoiceVolume { get; set; } = 1f;
    }
}