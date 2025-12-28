using Core.Bootstrap;
using Core.SaveSystem;

namespace Core.Settings
{
    /// <summary>
    /// Interface for applying game settings to the Unity engine.
    /// Follows SOLID: Interface Segregation and Dependency Inversion.
    /// </summary>
    public interface ISettingsService : IInitializable
    {
        void ApplyAllSettings(GameSettingsData data);
        void ApplyGraphicsSettings(GameSettingsData data);
        void ApplyAudioSettings(GameSettingsData data);
        void ApplyGameplaySettings(GameSettingsData data);
    }
}
