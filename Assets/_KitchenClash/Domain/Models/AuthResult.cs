namespace KitchenClash.Domain
{
    public sealed class AuthResult
    {
        public bool Success { get; }
        public string ProductUserId { get; }
        public bool IsGuest { get; }
        public string Error { get; }

        public AuthResult(bool success, string productUserId = null, bool isGuest = false, string error = null)
        {
            Success = success;
            ProductUserId = productUserId;
            IsGuest = isGuest;
            Error = error;
        }

        public static AuthResult Failed(string error) => new(false, error: error);
    }
}
