namespace Playcenter.GameFlow
{
    /// <summary>
    /// Optional queue hint when the player taps PLAY.
    /// Null / empty fields mean "use remembered last queue".
    /// </summary>
    public sealed class PlayRequest
    {
        public string ModeId { get; set; }
        public int TeamSize { get; set; }
        public string ChefId { get; set; }

        public static PlayRequest Empty { get; } = new PlayRequest();
    }
}
