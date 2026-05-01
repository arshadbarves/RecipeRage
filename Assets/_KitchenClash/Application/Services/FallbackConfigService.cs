using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    /// <summary>
    /// Returns hardcoded defaults. Used in Editor and tests per GDD.
    /// </summary>
    public sealed class FallbackConfigService : IConfigService
    {
        public T Get<T>(string key, T fallback) => fallback;
        public Task FetchAsync() => Task.CompletedTask;
    }
}
