using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IRemoteConfigService
    {
        Task<bool> Initialize();
        T GetConfig<T>() where T : class, IConfigModel;
        bool TryGetConfig<T>(out T config) where T : class, IConfigModel;
        Task<bool> RefreshConfig();
        Task<bool> RefreshConfig<T>() where T : class, IConfigModel;
        ConfigHealthStatus HealthStatus { get; }
        DateTime LastUpdateTime { get; }
    }
}
