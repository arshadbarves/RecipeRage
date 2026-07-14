namespace KitchenClash.Application
{
    /// <summary>
    /// Vendor-neutral result for lobby create/join operations.
    /// Infrastructure maps Epic.OnlineServices.Result into this at the boundary.
    /// </summary>
    public readonly struct LobbyOpResult
    {
        public bool Success { get; }
        public string ErrorCode { get; }
        public string Message { get; }

        public LobbyOpResult(bool success, string errorCode = null, string message = null)
        {
            Success = success;
            ErrorCode = errorCode;
            Message = message;
        }

        public static LobbyOpResult Ok() => new(true);

        public static LobbyOpResult Fail(string errorCode, string message = null) =>
            new(false, errorCode, message);

        public override string ToString() =>
            Success ? "Success" : $"Failed({ErrorCode}): {Message}";
    }
}
