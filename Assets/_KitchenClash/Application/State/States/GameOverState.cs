namespace KitchenClash.Application.State.States
{
    public class GameOverState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly IMatchContext _matchContext;

        public GameOverState(IUIService uiService, IMatchContext matchContext)
        {
            _uiService = uiService;
            _matchContext = matchContext;
        }

        public override void Enter()
        {
            base.Enter();
            _matchContext.Refresh();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update() { }
        public override void FixedUpdate() { }
    }
}
