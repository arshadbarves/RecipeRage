using System;

namespace KitchenClash.Domain
{
    public sealed class FriendRequest
    {
        public string Id { get; set; }
        public string FromUserId { get; set; }
        public string FromDisplayName { get; set; }
        public string FromUserName { get; set; }
        public string FromFriendCode { get; set; }
        public string ToUserId { get; set; }
        public string Message { get; set; }
        public string SentAt { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
