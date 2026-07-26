namespace Playcenter.Services
{
    /// <summary>
    /// Engine-free snapshot of player-facing game settings.
    /// </summary>
    public sealed class GameSettings
    {
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public bool ReduceMotion { get; set; }
        public string LanguageCode { get; set; } = "en";

        public GameSettings Clone()
        {
            return new GameSettings
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                ReduceMotion = ReduceMotion,
                LanguageCode = LanguageCode ?? "en"
            };
        }

        public void CopyFrom(GameSettings source)
        {
            if (source == null)
            {
                return;
            }

            MasterVolume = source.MasterVolume;
            MusicVolume = source.MusicVolume;
            SfxVolume = source.SfxVolume;
            ReduceMotion = source.ReduceMotion;
            LanguageCode = source.LanguageCode ?? "en";
        }
    }
}
