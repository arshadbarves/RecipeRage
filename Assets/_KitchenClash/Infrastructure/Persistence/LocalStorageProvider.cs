using KitchenClash.Application;
using System.IO;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KitchenClash.Infrastructure.Persistence
{
    public sealed class LocalStorageProvider : IStorageProvider
    {
        private readonly string _basePath;

        public LocalStorageProvider()
        {
            _basePath = UnityEngine.Application.persistentDataPath;
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

        public async UniTask<string> ReadAsync(string key)
        {
            var path = GetPath(key);
            if (!File.Exists(path)) return null;
            return await File.ReadAllTextAsync(path);
        }

        public async UniTask WriteAsync(string key, string content)
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
