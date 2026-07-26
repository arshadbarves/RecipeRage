using System.Collections;

namespace Playcenter.Services
{
    /// <summary>
    /// Remote config with local fallback defaults. Firebase Remote Config in production.
    /// </summary>
    public interface IConfigService
    {
        bool IsReady { get; }
        IEnumerator Initialize();
        T Get<T>(string key, T fallback);
    }
}
