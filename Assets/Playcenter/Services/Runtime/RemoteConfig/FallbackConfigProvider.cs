using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Default provider when no cloud backend is wired: reports available, returns defaults.</summary>
    public sealed class FallbackConfigProvider : IConfigProvider
    {
        public string ProviderName => "Fallback";
        public bool IsAvailable() => true;
        public Task<bool> Initialize() => Task.FromResult(true);
        public Task<T> FetchConfig<T>(string key) where T : IConfigModel => Task.FromResult(default(T));
        public Task<Dictionary<string, IConfigModel>> FetchAllConfigs() =>
            Task.FromResult(new Dictionary<string, IConfigModel>());
    }
}
