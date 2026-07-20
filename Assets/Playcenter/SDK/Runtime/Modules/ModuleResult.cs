namespace Playcenter.SDK
{
    public sealed class ModuleResult
    {
        public bool Success { get; private set; }

        // Populated when Success is false; FailedModuleId is filled in by ModuleHost.
        public BootFailure Failure { get; private set; }

        public static ModuleResult Ok() => new ModuleResult { Success = true };

        public static ModuleResult Fail(BootFailureCode code, string message) =>
            new ModuleResult
            {
                Success = false,
                Failure = new BootFailure(code, message, null)
            };
    }
}
