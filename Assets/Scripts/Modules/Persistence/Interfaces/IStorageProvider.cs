using Cysharp.Threading.Tasks;

namespace Modules.Persistence
{
    /// <summary>
    /// Base interface for all storage providers.
    /// Supports both synchronous and asynchronous operations.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Indicates if the provider is currently available for use.
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        /// Read data synchronously (blocking).
        /// </summary>
        string Read(string key);
        
        /// <summary>
        /// Write data synchronously (blocking).
        /// </summary>
        void Write(string key, string content);
        
        /// <summary>
        /// Read data asynchronously (non-blocking).
        /// </summary>
        UniTask<string> ReadAsync(string key);
        
        /// <summary>
        /// Write data asynchronously (non-blocking).
        /// </summary>
        UniTask WriteAsync(string key, string content);
        
        /// <summary>
        /// Check if data exists for the given key.
        /// </summary>
        bool Exists(string key);
        
        /// <summary>
        /// Delete data for the given key.
        /// </summary>
        void Delete(string key);
        
        /// <summary>
        /// Delete data asynchronously.
        /// </summary>
        UniTask DeleteAsync(string key);
    }
}
