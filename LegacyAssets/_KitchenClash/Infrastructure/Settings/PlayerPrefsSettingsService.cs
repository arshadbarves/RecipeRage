using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Settings
{
    /// <summary>
    /// <see cref="ISettingsService"/> adapter over an <see cref="ISettingsStore"/>.
    /// Keys use the <c>pc.settings.*</c> prefix.
    /// </summary>
    public sealed class PlayerPrefsSettingsService : ISettingsService
    {
        public const string MasterVolumeKey = "pc.settings.masterVolume";
        public const string MusicVolumeKey = "pc.settings.musicVolume";
        public const string SfxVolumeKey = "pc.settings.sfxVolume";
        public const string ReduceMotionKey = "pc.settings.reduceMotion";
        public const string LanguageCodeKey = "pc.settings.languageCode";

        private readonly ISettingsStore _store;
        private readonly GameSettings _current = new GameSettings();

        public PlayerPrefsSettingsService(ISettingsStore store)
        {
            _store = store ?? throw new System.ArgumentNullException(nameof(store));
            LoadFromStore();
        }

        public GameSettings Current => _current;

        public Task LoadAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            LoadFromStore();
            return Task.CompletedTask;
        }

        public Task SaveAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _store.SetFloat(MasterVolumeKey, _current.MasterVolume);
            _store.SetFloat(MusicVolumeKey, _current.MusicVolume);
            _store.SetFloat(SfxVolumeKey, _current.SfxVolume);
            _store.SetInt(ReduceMotionKey, _current.ReduceMotion ? 1 : 0);
            _store.SetString(LanguageCodeKey, _current.LanguageCode ?? "en");
            _store.Save();
            return Task.CompletedTask;
        }

        public void Apply(GameSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            _current.CopyFrom(settings);
            ClampCurrent();
        }

        private void LoadFromStore()
        {
            _current.MasterVolume = _store.GetFloat(MasterVolumeKey, 1f);
            _current.MusicVolume = _store.GetFloat(MusicVolumeKey, 1f);
            _current.SfxVolume = _store.GetFloat(SfxVolumeKey, 1f);
            _current.ReduceMotion = _store.GetInt(ReduceMotionKey, 0) != 0;
            _current.LanguageCode = _store.GetString(LanguageCodeKey, "en");
            ClampCurrent();
        }

        private void ClampCurrent()
        {
            _current.MasterVolume = Clamp01(_current.MasterVolume);
            _current.MusicVolume = Clamp01(_current.MusicVolume);
            _current.SfxVolume = Clamp01(_current.SfxVolume);
            if (string.IsNullOrEmpty(_current.LanguageCode))
            {
                _current.LanguageCode = "en";
            }
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}
