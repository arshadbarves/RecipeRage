using KitchenClash.Domain;

namespace KitchenClash.Presentation.ViewModels
{
    public sealed class MatchHUDViewModel : ScreenViewModel
    {
        private readonly IEventBus _eventBus;

        public BindableProperty<int> TeamAScore { get; } = new(0);
        public BindableProperty<int> TeamBScore { get; } = new(0);
        public BindableProperty<float> TimeRemaining { get; } = new(0f);
        public BindableProperty<int> ComboCount { get; } = new(0);

        public MatchHUDViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
        }

        private void HandleScoreChanged(ScoreChangedEvent e)
        {
            TeamAScore.Value = e.TeamAScore;
            TeamBScore.Value = e.TeamBScore;
        }

        public override void OnExit()
        {
            _eventBus.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
        }
    }
}
