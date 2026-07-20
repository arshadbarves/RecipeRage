using System;

namespace Playcenter.Services
{
    public sealed class FriendInfo
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public string FriendCode { get; set; }
        /// <summary>Backend product user id as string.</summary>
        public string ProductUserId { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsRecent { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
