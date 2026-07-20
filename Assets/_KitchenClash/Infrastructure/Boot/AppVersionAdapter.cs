using Playcenter.SDK;

namespace KitchenClash.Infrastructure.Boot
{
    /// <summary>
    /// Provides the current application version from Unity's Application.version
    /// to the Playcenter SDK module pipeline.
    /// </summary>
    public sealed class AppVersionAdapter : IAppVersion
    {
        // Fully qualify — KitchenClash.Application assembly collides with UnityEngine.Application.
        public string Current => UnityEngine.Application.version;
    }
}
