using Cysharp.Threading.Tasks;

namespace KitchenClash.Application
{
    public interface IStorageProvider
    {
        bool IsAvailable { get; }
        string Read(string key);
        void Write(string key, string content);
        UniTask<string> ReadAsync(string key);
        UniTask WriteAsync(string key, string content);
        bool Exists(string key);
        void Delete(string key);
    }
}
