using KitchenClash.Application.Services;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Extends IGameStarter with the ability to receive an IMatchContext at
    /// match-start time. Defined in Infrastructure (not Application) because
    /// IMatchContext carries Unity/NGO types that have no place in the
    /// Application layer.
    ///
    /// MatchContextBridge resolves IMatchContextReceiver from the session scope
    /// and calls SetMatchContext() / ClearMatchContext() as the match scope
    /// starts and disposes.
    /// </summary>
    public interface IMatchContextReceiver : IGameStarter
    {
        void SetMatchContext(IMatchContext matchContext);
        void ClearMatchContext();
    }
}
