using Cysharp.Threading.Tasks;

namespace Core.GameModes
{
    /// <summary>
    /// Service for loading and unloading map scenes additively.
    /// </summary>
    public interface IMapLoader
    {
        /// <summary>
        /// Currently loaded map data.
        /// </summary>
        MapData CurrentMap { get; }

        /// <summary>
        /// Whether a map is currently loaded.
        /// </summary>
        bool IsMapLoaded { get; }

        /// <summary>
        /// Load a map scene additively.
        /// </summary>
        /// <param name="mapData">Map data to load</param>
        UniTask<bool> LoadMapAsync(MapData mapData);

        /// <summary>
        /// Load a map scene by name additively.
        /// </summary>
        /// <param name="sceneName">Scene name to load</param>
        UniTask<bool> LoadMapAsync(string sceneName);

        /// <summary>
        /// Unload the currently loaded map.
        /// </summary>
        UniTask UnloadCurrentMapAsync();

        /// <summary>
        /// Get map data by scene name.
        /// </summary>
        MapData GetMapData(string sceneName);
    }
}
