using System.Threading.Tasks;
using GameSystem.State.GameStates;

namespace Core.Interfaces
{
    public interface IGameStateService : IGameService
    {
        void RequestGameStateChange(GameState newState);
        GameState CurrentState { get; }
        event System.Action<GameState> OnGameStateChanged;
        Task InitializeAsync();
    }
}
