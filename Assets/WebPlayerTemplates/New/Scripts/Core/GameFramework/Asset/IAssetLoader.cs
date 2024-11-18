using System.Threading.Tasks;
using UnityEngine;

namespace Core.GameFramework.Asset
{
    public interface IAssetLoader
    {
        Task<T> LoadAssetAsync<T>(string key) where T : Object;
        Task<GameObject> InstantiateAsync(string key, Vector3 position, Quaternion rotation);
        void ReleaseAsset<T>(T asset) where T : Object;
        Task ReleaseAllAssets();
        float GetLoadingProgress();
    }
}