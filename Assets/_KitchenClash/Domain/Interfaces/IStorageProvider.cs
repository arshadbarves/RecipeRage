using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IStorageProvider
    {
        bool IsAvailable { get; }
        string Read(string key);
        void Write(string key, string content);
        Task<string> ReadAsync(string key);
        Task WriteAsync(string key, string content);
        bool Exists(string key);
        void Delete(string key);
    }
}
