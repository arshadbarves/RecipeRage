using Core.Networking.Common;

namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Strategy interface for different matchmaking approaches
    /// </summary>
    public interface IMatchmakingStrategy
    {
        void Execute(GameMode gameMode, int minPlayers, int maxPlayers);
        void SearchForLobbies(GameMode gameMode);
    }
}
