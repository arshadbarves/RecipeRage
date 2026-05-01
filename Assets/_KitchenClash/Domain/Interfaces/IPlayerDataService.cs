using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IPlayerDataService
    {
        Task SaveAsync(string key, string data);
        Task<string> LoadAsync(string key);
        Task DeleteAsync(string key);
    }
}
