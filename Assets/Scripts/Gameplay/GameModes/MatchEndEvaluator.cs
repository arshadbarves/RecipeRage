using System.Collections.Generic;

namespace Gameplay.GameModes
{
    public readonly struct MatchEndEvaluation
    {
        public static MatchEndEvaluation NoEnd => new(false, -1, false, 0);

        public MatchEndEvaluation(bool shouldEnd, int winningTeamId, bool isDraw, int winningScore)
        {
            ShouldEnd = shouldEnd;
            WinningTeamId = winningTeamId;
            IsDraw = isDraw;
            WinningScore = winningScore;
        }

        public bool ShouldEnd { get; }
        public int WinningTeamId { get; }
        public bool IsDraw { get; }
        public int WinningScore { get; }
    }

    public static class MatchEndEvaluator
    {
        public static MatchEndEvaluation EvaluateScoreLimit(IReadOnlyList<int> teamScores, bool hasScoreLimit, int targetScore)
        {
            if (!hasScoreLimit || targetScore <= 0 || teamScores == null || teamScores.Count == 0)
            {
                return MatchEndEvaluation.NoEnd;
            }

            for (int i = 0; i < teamScores.Count; i++)
            {
                if (teamScores[i] >= targetScore)
                {
                    return EvaluateFinalScores(teamScores);
                }
            }

            return MatchEndEvaluation.NoEnd;
        }

        public static MatchEndEvaluation EvaluateFinalScores(IReadOnlyList<int> teamScores)
        {
            if (teamScores == null || teamScores.Count == 0)
            {
                return new MatchEndEvaluation(true, -1, true, 0);
            }

            int winningTeamId = -1;
            int winningScore = int.MinValue;
            bool isDraw = false;

            for (int i = 0; i < teamScores.Count; i++)
            {
                int currentScore = teamScores[i];

                if (currentScore > winningScore)
                {
                    winningScore = currentScore;
                    winningTeamId = i;
                    isDraw = false;
                }
                else if (currentScore == winningScore)
                {
                    isDraw = true;
                }
            }

            if (isDraw)
            {
                winningTeamId = -1;
            }

            return new MatchEndEvaluation(true, winningTeamId, isDraw, winningScore);
        }
    }
}
