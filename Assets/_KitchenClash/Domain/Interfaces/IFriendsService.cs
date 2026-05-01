using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IFriendsService
    {
        Task<IReadOnlyList<PlayerInfo>> GetFriendsAsync();
        Task SendInviteAsync(string playerId);
        Task AcceptInviteAsync(string playerId);
        Task BlockPlayerAsync(string playerId);
        event Action<PlayerInfo> OnFriendStatusChanged;
    }
}
