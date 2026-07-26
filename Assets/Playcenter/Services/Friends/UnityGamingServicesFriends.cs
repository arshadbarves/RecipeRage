using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Unity Gaming Services Friends provider. Until UGS Friends is wired (Slice 5),
    /// returns an empty friend list and a locally-generated friend code.
    /// </summary>
    public sealed class UnityGamingServicesFriends : IFriendsService
    {
        private readonly ISaveService _save;
        private readonly ILoggingService _log;

        public bool IsReady { get; private set; }
        public string MyFriendCode { get; private set; }

        public UnityGamingServicesFriends(ISaveService save, ILoggingService log)
        {
            _save = save;
            _log = log;
        }

        public IEnumerator Initialize()
        {
            MyFriendCode = _save.Load("friend_code", string.Empty);
            if (string.IsNullOrEmpty(MyFriendCode))
            {
                var rng = new System.Random();
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var code = new char[6];
                for (int i = 0; i < code.Length; i++)
                {
                    code[i] = chars[rng.Next(chars.Length)];
                }
                MyFriendCode = new string(code);
                _save.Save("friend_code", MyFriendCode);
            }

            IsReady = true;
            _log.Log($"[Friends] Initialized (stub mode, UGS pending). Code: {MyFriendCode}");
            yield break;
        }

        public Task<List<FriendInfo>> GetFriends()
        {
            return Task.FromResult(new List<FriendInfo>());
        }

        public Task<bool> AddFriendByCode(string code)
        {
            _log.Log($"[Friends] Add by code requested: {code} (stub — no-op)");
            return Task.FromResult(false);
        }

        public void InviteFriend(string friendId)
        {
            _log.Log($"[Friends] Invite requested: {friendId} (stub — no-op)");
        }
    }
}
