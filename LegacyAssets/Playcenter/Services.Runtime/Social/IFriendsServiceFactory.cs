namespace Playcenter.Services
{
    /// <summary>
    /// Creates an <see cref="IFriendsService"/> after platform auth is ready.
    /// </summary>
    public interface IFriendsServiceFactory
    {
        IFriendsService Create(ILobbyManager lobbyManager, IAuthService authService);
    }
}
