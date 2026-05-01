using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class MatchHUDViewModel : ScreenViewModel
    {
        private readonly IScoreService _scoreService;

        public BindableProperty<int> TeamAScore { get; } = new(0);
        public BindableProperty<int> TeamBScore { get; } = new(0);
        public BindableProperty<float> TimeRemaining { get; } = new(0f);
        public BindableProperty<int> ComboCount { get; } = new(0);

        public MatchHUDViewModel(IScoreService scoreService)
        {
            _scoreService = scoreService;
            _scoreService.OnScoreChanged += HandleScoreChanged;
        }

        private void HandleScoreChanged(ScoreChangedEvent e)
        {
            TeamAScore.Value = e.TeamAScore;
            TeamBScore.Value = e.TeamBScore;
        }

        public override void OnExit()
        {
            _scoreService.OnScoreChanged -= HandleScoreChanged;
        }
    }
}
