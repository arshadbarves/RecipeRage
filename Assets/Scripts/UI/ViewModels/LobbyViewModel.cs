using Core.Networking.Interfaces;
using Core.Reactive;
using Core.State;
using Core.State.States;
using UI.Core;
using VContainer;

namespace UI.ViewModels
{
    public class LobbyViewModel : BaseViewModel
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameStateManager _stateManager;

        public BindableProperty<bool> IsMatchmaking { get; } = new BindableProperty<bool>(false);
        public BindableProperty<int> PlayerCount { get; } = new BindableProperty<int>(1);

        [Inject]
        public LobbyViewModel(IMatchmakingService matchmakingService, IGameStateManager stateManager)
        {
            _matchmakingService = matchmakingService;
            _stateManager = stateManager;
        }

        public void Play()
        {
            // Simple pass-through for now
            _stateManager.ChangeState<MatchmakingState>();
        }

        public void StartMatchmaking()
        {
            IsMatchmaking.Value = true;
        }
    }
}