namespace Playcenter.SDK
{
    public interface IPlaycenterServices
    {
        T Get<T>() where T : class;
        bool TryGet<T>(out T service) where T : class;
        bool IsRegistered<T>() where T : class;
    }
}
