using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IConfigService
    {
        T Get<T>(string key, T fallback);
        Task FetchAsync();
    }
}
