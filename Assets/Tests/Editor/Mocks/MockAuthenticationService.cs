using Core.Authentication;
using Cysharp.Threading.Tasks;

namespace Tests.Editor.Mocks
{
    public class MockAuthenticationService : IAuthenticationService
    {
        public bool IsLoggedIn => true;
        public string LastLoginMethod => "DeviceID";

        public void Initialize() { }
        public UniTask<bool> InitializeAsync() => UniTask.FromResult(true);
        public UniTask<bool> AttemptAutoLoginAsync() => UniTask.FromResult(true);
        public UniTask<bool> LoginAsGuestAsync() => UniTask.FromResult(true);
        public UniTask<bool> LoginWithFacebookAsync() => UniTask.FromResult(false);
        public UniTask LogoutAsync() => UniTask.CompletedTask;
    }
}
