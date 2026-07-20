namespace Playcenter.SDK
{
    public sealed class ModuleContext
    {
        public IPlaycenterServices Services { get; }
        public IBootProgress Progress { get; }

        public ModuleContext(IPlaycenterServices services, IBootProgress progress)
        {
            Services = services;
            Progress = progress;
        }
    }
}
