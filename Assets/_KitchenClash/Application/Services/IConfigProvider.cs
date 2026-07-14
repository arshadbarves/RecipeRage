using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Playcenter.Services;

namespace KitchenClash.Application.Services
{
    public interface IConfigProvider
    {
        string ProviderName { get; }
        bool IsAvailable();
        UniTask<bool> Initialize();
        UniTask<T> FetchConfig<T>(string key) where T : IConfigModel;
        UniTask<Dictionary<string, IConfigModel>> FetchAllConfigs();
    }
}
