using Cysharp.Threading.Tasks;
using RecipeRage.Modules.Auth.Core;

namespace RecipeRage.Modules.Auth.Tests
{
    public class MockAuthService : IAuthService
    {
        public bool IsLoggedInResult { get; set; } = false;
        public string CurrentUserIdResult { get; set; } = "mock_user_id";

        public UniTask<bool> LoginAsync(AuthType type)
        {
            IsLoggedInResult = true;
            return UniTask.FromResult(true);
        }

        public UniTask LogoutAsync()
        {
            IsLoggedInResult = false;
            return UniTask.CompletedTask;
        }

        public bool IsLoggedIn()
        {
            return IsLoggedInResult;
        }

        public string GetCurrentUserId()
        {
            return CurrentUserIdResult;
        }
    }
}
