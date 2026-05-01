using System;

namespace KitchenClash.Domain
{
    public sealed class FriendInfo
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public string FriendCode { get; set; }
        /// <summary>EOS ProductUserId as string. Use ToString()/parse in Infrastructure.</summary>
        public string ProductUserId { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsRecent { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
