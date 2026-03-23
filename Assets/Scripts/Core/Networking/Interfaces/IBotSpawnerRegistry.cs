namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Internal registry for the runtime bot spawner instance created during match start.
    /// </summary>
    public interface IBotSpawnerRegistry
    {
        IBotSpawner BotSpawner { get; set; }
    }
}
