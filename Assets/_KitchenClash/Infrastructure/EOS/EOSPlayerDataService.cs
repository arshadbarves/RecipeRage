using KitchenClash.Application;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// EOS Player Data Storage implementation.
    /// Stores trophies, streak, settings (5MB/player per GDD).
    /// </summary>
    public sealed class EOSPlayerDataService
    {
        // TODO: Implement with EOS Player Data Storage SDK calls
        private readonly IStorageProvider _fallbackStorage;

        public EOSPlayerDataService(IStorageProvider fallbackStorage)
        {
            _fallbackStorage = fallbackStorage;
        }

        public async UniTask SaveAsync(string key, string data)
        {
            // TODO: Save to EOS Player Data Storage
            // Fallback to local for now
            await _fallbackStorage.WriteAsync(key, data);
        }

        public async UniTask<string> LoadAsync(string key)
        {
            // TODO: Load from EOS Player Data Storage
            return await _fallbackStorage.ReadAsync(key);
        }

        public async UniTask DeleteAsync(string key)
        {
            // TODO: Delete from EOS Player Data Storage
            _fallbackStorage.Delete(key);
            await UniTask.CompletedTask;
        }
    }
}
