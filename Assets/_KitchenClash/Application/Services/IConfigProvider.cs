using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public interface IConfigProvider
    {
        string ProviderName { get; }
        bool IsAvailable();
        UniTask<bool> Initialize();
        UniTask<T> FetchConfig<T>(string key) where T : Domain.Interfaces.IConfigModel;
        UniTask<Dictionary<string, Domain.Interfaces.IConfigModel>> FetchAllConfigs();
    }
}
