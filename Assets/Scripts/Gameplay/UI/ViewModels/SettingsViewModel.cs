using Core.Auth.Core;
using Core.Persistence;
using Core.Shared;
using Core.UI.Core;
using VContainer;

namespace Gameplay.UI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISaveService _saveService;
        private readonly IAuthService _authService;

        public BindableProperty<float> MusicVolume { get; } = new BindableProperty<float>(1f);
        public BindableProperty<float> SFXVolume { get; } = new BindableProperty<float>(1f);

        [Inject]
        public SettingsViewModel(ISaveService saveService, IAuthService authService)
        {
            _saveService = saveService;
            _authService = authService;
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _saveService.GetSettings();
            if (settings != null)
            {
                MusicVolume.Value = settings.MusicVolume;
                SFXVolume.Value = settings.SFXVolume;
            }
        }

        public void SaveVolumeSettings()
        {
            _saveService.UpdateSettings(s => 
            {
                s.MusicVolume = MusicVolume.Value;
                s.SFXVolume = SFXVolume.Value;
            });
        }

        public void Logout()
        {
            _authService.LogoutAsync();
        }
    }
}