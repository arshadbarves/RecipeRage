using KitchenClash.Domain;
using System;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application
{
    public interface IAuthService
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        bool IsUgsSignedIn { get; }
        string PlayerId { get; }
        string EosProductUserId { get; }
        UniTask<bool> InitializeAsync();
        UniTask<bool> LoginAsync(AuthType type);
        UniTask LogoutAsync();
    }
}
