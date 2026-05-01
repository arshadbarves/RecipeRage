using System.IO;
using System.Threading.Tasks;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Persistence
{
    public sealed class LocalStorageProvider : IStorageProvider
    {
        private readonly string _basePath;

        public LocalStorageProvider()
        {
            _basePath = Application.persistentDataPath;
        }

        public bool IsAvailable => true;

        public string Read(string key)
        {
            var path = GetPath(key);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void Write(string key, string content)
        {
            var path = GetPath(key);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }

        public async Task<string> ReadAsync(string key)
        {
            var path = GetPath(key);
            if (!File.Exists(path)) return null;
            return await File.ReadAllTextAsync(path);
        }

        public async Task WriteAsync(string key, string content)
        {
            var path = GetPath(key);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(path, content);
        }

        public bool Exists(string key) => File.Exists(GetPath(key));

        public void Delete(string key)
        {
            var path = GetPath(key);
            if (File.Exists(path)) File.Delete(path);
        }

        private string GetPath(string key) => Path.Combine(_basePath, key);
    }
}
