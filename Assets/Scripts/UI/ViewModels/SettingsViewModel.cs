using Core.SaveSystem;
using Core.Reactive;
using RecipeRage.Modules.Auth.Core;
using UI.Core;
using VContainer;

namespace UI.ViewModels
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

        public void Logout()
        {
            _authService.LogoutAsync();
        }
    }
}