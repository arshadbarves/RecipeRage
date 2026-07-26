using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Load/save/apply player settings. Engine-free port.
    /// </summary>
    public interface ISettingsService
    {
        GameSettings Current { get; }

        Task LoadAsync(CancellationToken ct = default);

        Task SaveAsync(CancellationToken ct = default);

        void Apply(GameSettings settings);
    }
}
