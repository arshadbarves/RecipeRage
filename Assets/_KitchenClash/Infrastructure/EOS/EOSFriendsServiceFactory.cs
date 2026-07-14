using KitchenClash.Application;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// EOS/UGS-backed factory for <see cref="IFriendsService"/>.
    /// Registered in Composition so Network never constructs EOSFriendsService.
    /// </summary>
    public sealed class EOSFriendsServiceFactory : IFriendsServiceFactory
    {
        public IFriendsService Create(ILobbyManager lobbyManager, IAuthService authService)
        {
            return new EOSFriendsService(lobbyManager, authService);
        }
    }
}
