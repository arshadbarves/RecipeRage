namespace App.State.States
{
    /// <summary>
    /// Generic State for loading operations.
    /// Can be used for initial boot, scene transitions, or other long-running tasks.
    /// </summary>
    public class LoadingState : BaseState
    {
        public override void Enter()
        {
            base.Enter();
            LogMessage("Entered Loading State");
            // The actual loading logic should be driven by a Service or Controller 
            // that pushes this state and updates the UI.
        }

        public override void Exit()
        {
            base.Exit();
            LogMessage("Exited Loading State");
        }
    }
}
