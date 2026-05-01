using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class ResultsViewModel : ScreenViewModel
    {
        public BindableProperty<int> FinalScoreA { get; } = new(0);
        public BindableProperty<int> FinalScoreB { get; } = new(0);
        public BindableProperty<bool> IsWinner { get; } = new(false);
        public BindableProperty<bool> IsDraw { get; } = new(false);
        public BindableProperty<int> TrophyChange { get; } = new(0);

        public override void OnEnter(object param)
        {
            if (param is MatchEndEvaluation eval)
            {
                IsWinner.Value = eval.WinningTeamId >= 0;
                IsDraw.Value = eval.IsDraw;
            }
        }
    }
}
