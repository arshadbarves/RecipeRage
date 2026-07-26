namespace KitchenClash.Domain
{
    public sealed class MusicEvent
    {
        public MusicTrack Track { get; set; }
        public float FadeTime { get; set; }

        public MusicEvent() { }

        public MusicEvent(MusicTrack track, float fadeTime = 1f)
        {
            Track = track;
            FadeTime = fadeTime;
        }
    }
}
