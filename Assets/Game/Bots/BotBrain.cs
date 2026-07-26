namespace RecipeRage.Bots
{
    /// <summary>
    /// Per-bot think cycle: snapshot → plan → assign. Runs at tick rate (10Hz),
    /// budgeted — cheap insurance against frame spikes with 4+ bots.
    /// </summary>
    public sealed class BotBrain
    {
        private readonly BotController _controller;
        private readonly TaskPlanner _planner;
        private readonly KitchenSnapshotBuilder _snapshotBuilder;
        private readonly BotClaimRegistry _claims;
        private readonly IBotBudget _budget;
        private float _difficultyDwellScale = 1f;

        public BotBrain(
            BotController controller,
            TaskPlanner planner,
            KitchenSnapshotBuilder snapshotBuilder,
            BotClaimRegistry claims,
            IBotBudget budget)
        {
            _controller = controller;
            _planner = planner;
            _snapshotBuilder = snapshotBuilder;
            _claims = claims;
            _budget = budget;
        }

        public void SetDifficulty(float dwellScale)
        {
            _difficultyDwellScale = dwellScale;
        }

        public void Tick()
        {
            if (_controller.CurrentTask != null && !_controller.CurrentTask.IsComplete)
            {
                return; // still executing
            }

            if (_controller.CurrentTask != null && _controller.CurrentTask.TargetStation != null)
            {
                _claims.Release(_controller.CurrentTask.TargetStation, _controller.BotId);
            }

            var snapshot = _snapshotBuilder.Build(_controller.GetComponent<PlayerController>().Carry, _controller.transform.position);
            var task = _planner.Plan(snapshot);
            if (task == null)
            {
                return;
            }

            if (task.TargetStation != null && !_claims.TryClaim(task.TargetStation, _controller.BotId))
            {
                return; // lost the race — replan next tick
            }

            _controller.AssignTask(task, _difficultyDwellScale);
        }
    }
}
