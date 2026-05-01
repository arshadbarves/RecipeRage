using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class HomeScreenViewModel : ScreenViewModel
    {
        private readonly IMatchService _matchService;
        private readonly IConfigService _cfg;

        public BindableProperty<string> PlayerName { get; } = new("Player");
        public BindableProperty<int> Trophies { get; } = new(0);
        public BindableProperty<string> CurrentMapName { get; } = new("");
        public BindableProperty<string> CurrentMode { get; } = new("");
        public BindableProperty<ChefId> SelectedChef { get; } = new(ChefId.Rosa);

        public HomeScreenViewModel(IMatchService matchService, IConfigService cfg)
        {
            _matchService = matchService;
            _cfg = cfg;
        }

        public override void OnEnter(object param)
        {
            var queues = _matchService.GetQueues();
            if (queues.Count > 0)
            {
                CurrentMapName.Value = queues[0].SceneName;
                CurrentMode.Value = queues[0].DisplayName;
            }
        }
    }
}
