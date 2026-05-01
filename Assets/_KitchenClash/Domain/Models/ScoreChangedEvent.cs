namespace KitchenClash.Domain
{
    public sealed class ScoreChangedEvent
    {
        public TeamId Team { get; }
        public int Delta { get; }
        public int TeamAScore { get; }
        public int TeamBScore { get; }

        public ScoreChangedEvent(TeamId team, int delta, int teamAScore, int teamBScore)
        {
            Team = team;
            Delta = delta;
            TeamAScore = teamAScore;
            TeamBScore = teamBScore;
        }
    }
}
