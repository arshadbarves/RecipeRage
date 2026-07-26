using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    [UIScreen]
    public sealed class LoginScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var facebook = Root.Q<Button>("facebook-button");
            var google = Root.Q<Button>("google-button");
            var guest = Root.Q<Button>("guest-button");

            facebook.clicked += () => SignIn(provider => provider.SignInWithFacebook());
            google.clicked += () => SignIn(provider => provider.SignInWithGoogle());
            guest.clicked += () => SignIn(provider => provider.SignInAsGuest());

            UIAnimation.StaggerChildren(Root.Q<VisualElement>("button-container"), 0.1f);
            UIAnimation.ScaleBounce(Root.Q<VisualElement>("logo"));
        }

        private async void SignIn(System.Func<IAuthService, System.Threading.Tasks.Task<AuthResult>> signIn)
        {
            var auth = ServiceLocator.Get<IAuthService>();
            var result = await signIn(auth);
            if (result.Success)
            {
                ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
            }
        }
    }
}
