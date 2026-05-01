using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public interface IConfigProvider
    {
        string ProviderName { get; }
        bool IsAvailable();
        UniTask<bool> Initialize();
        UniTask<T> FetchConfig<T>(string key) where T : KitchenClash.Domain.IConfigModel;
        UniTask<Dictionary<string, KitchenClash.Domain.IConfigModel>> FetchAllConfigs();
    }
}
