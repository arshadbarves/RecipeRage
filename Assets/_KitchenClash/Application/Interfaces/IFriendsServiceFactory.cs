using Playcenter.Services;

namespace KitchenClash.Application
{
    /// <summary>
    /// Creates an <see cref="IFriendsService"/> after platform auth is ready.
    /// Keeps Network free of concrete EOS friends types.
    /// </summary>
    public interface IFriendsServiceFactory
    {
        IFriendsService Create(ILobbyManager lobbyManager, IAuthService authService);
    }
}
