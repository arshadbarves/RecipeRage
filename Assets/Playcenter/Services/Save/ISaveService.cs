using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Typed key-value save layer on top of IStorageService. JSON-serialized.
    /// </summary>
    public interface ISaveService
    {
        void Save<T>(string key, T value);
        T Load<T>(string key, T fallback);
        bool Has(string key);
        void Delete(string key);
        Task Flush();
    }
}
