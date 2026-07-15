using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IConfigProvider
    {
        string ProviderName { get; }
        bool IsAvailable();
        Task<bool> Initialize();
        Task<T> FetchConfig<T>(string key) where T : IConfigModel;
        Task<Dictionary<string, IConfigModel>> FetchAllConfigs();
    }
}
