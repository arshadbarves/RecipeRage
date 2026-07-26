using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Settings: audio sliders (mixer), sign-out, tutorial replay, legal links.
    /// </summary>
    [UIScreen]
    public sealed class SettingsScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var audio = ServiceLocator.Get<IAudioService>();

            var master = Root.Q<Slider>("master-volume");
            var music = Root.Q<Slider>("music-volume");
            var sfx = Root.Q<Slider>("sfx-volume");
            master.value = 1f;
            music.value = 0.7f;
            sfx.value = 1f;

            master.RegisterValueChangedCallback(e => audio.SetMasterVolume(e.newValue));
            music.RegisterValueChangedCallback(e => audio.SetMusicVolume(e.newValue));
            sfx.RegisterValueChangedCallback(e => audio.SetSfxVolume(e.newValue));

            Root.Q<Button>("sign-out-button").clicked += () =>
            {
                ServiceLocator.Get<IAuthService>().SignOut();
                ServiceLocator.Get<IUIService>().Show<LoginScreen>();
            };

            Root.Q<Button>("replay-tutorial-button").clicked += () =>
            {
                ServiceLocator.Get<ISaveService>().Save("tutorial_completed", false);
                ServiceLocator.Get<IGameStateMachine>().ChangeState(new TutorialState());
            };

            Root.Q<Button>("back-button").clicked += () =>
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
        }
    }
}
