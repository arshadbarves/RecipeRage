namespace Playcenter.Services
{
    public interface INetTransportConfigurator
    {
        void ConfigureForSession(NetRole role, string sessionToken);
    }
}
