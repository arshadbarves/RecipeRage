using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IConfigService
    {
        T Get<T>(string key, T fallback);
        Task FetchAsync();
    }
}
