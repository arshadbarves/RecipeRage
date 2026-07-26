using Playcenter;
using Playcenter.Services;
using RecipeRage.Net;

namespace RecipeRage
{
    /// <summary>
    /// Pre-matchmaking lobby: player picks their chef (Brawl Stars-style),
    /// then taps Play → chef locks → matchmaking starts. There is NO separate
    /// pre-match chef select screen — the choice rides into the roster.
    /// </summary>
    public sealed class LobbyState : IGameState
    {
        private readonly int _teamSize;

        public LobbyState(int teamSize)
        {
            _teamSize = teamSize;
        }

        public void Enter()
        {
            ServiceLocator.Get<ILoggingService>().Log($"[Flow] Lobby entered (team {_teamSize})");
            // Slice 5 shows the chef grid UI here.
        }

        public void Exit() { }
        public void Update(float deltaTime) { }

        /// <summary>Called by the Play button (Slice 5 UI wires this).</summary>
        public void OnPlayPressed()
        {
            var progression = ServiceLocator.Get<IChefProgressionService>();
            var selected = progression.GetSelectedChef();
            ServiceLocator.Get<ILoggingService>().Log($"[Flow] Play pressed, chef locked: {selected}");

            var matchmaking = ServiceLocator.Get<MatchmakingController>();
            matchmaking.OnMatchFound += OnMatchFound;
            matchmaking.QuickMatch(_teamSize);
        }

        private void OnMatchFound()
        {
            var matchmaking = ServiceLocator.Get<MatchmakingController>();
            matchmaking.OnMatchFound -= OnMatchFound;
            ServiceLocator.Get<IGameStateMachine>().ChangeState(new TeamCompositionState());
        }
    }
}
