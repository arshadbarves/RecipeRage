using Cysharp.Threading.Tasks;
using RecipeRage.Modules.Auth.Core;
using PlayEveryWare.EpicOnlineServices;
using Core.Logging;
using Epic.OnlineServices;

namespace RecipeRage.Modules.Auth.Core
{
    public class EOSAuthService : IAuthService
    {
        public bool IsLoggedIn()
        {
            if (EOSManager.Instance == null) return false;
            var productUserId = EOSManager.Instance.GetProductUserId();
            return productUserId != null && productUserId.IsValid();
        }

        public string GetCurrentUserId()
        {
            return EOSManager.Instance?.GetProductUserId()?.ToString() ?? string.Empty;
        }

        public UniTask<bool> LoginAsync(AuthType type)
        {
            // To be implemented in next task
            return UniTask.FromResult(false);
        }

        public UniTask LogoutAsync()
        {
            // To be implemented in next task
            return UniTask.CompletedTask;
        }
        
        public void Initialize()
        {
            GameLogger.Log("EOSAuthService Skeleton Initialized");
        }
    }
}
