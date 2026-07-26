using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Friends via Unity Gaming Services (NOT EOS — EOS requires Epic accounts).
    /// </summary>
    public interface IFriendsService
    {
        bool IsReady { get; }
        string MyFriendCode { get; }
        IEnumerator Initialize();
        Task<List<FriendInfo>> GetFriends();
        Task<bool> AddFriendByCode(string code);
        void InviteFriend(string friendId);
    }

    public sealed class FriendInfo
    {
        public string FriendId;
        public string DisplayName;
        public FriendPresence Presence;
    }

    public enum FriendPresence
    {
        Offline,
        InMainMenu,
        InLobby,
        InMatch
    }
}
