namespace Playcenter.SDK
{
    public sealed class BootFailure
    {
        public BootFailureCode Code { get; }
        public string Message { get; }
        public string FailedModuleId { get; }

        public BootFailure(BootFailureCode code, string message, string failedModuleId)
        {
            Code = code;
            Message = message;
            FailedModuleId = failedModuleId;
        }
    }
}
