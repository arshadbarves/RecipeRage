using System.Threading.Tasks;

namespace Core.Auth
{
    public interface IAuthenticationManager
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        string PlayerId { get; }
        string EosProductUserId { get; }

        Task<bool> InitializeAsync();
        Task<bool> SignInWithEOSAsync();
        void SignOut();
        void Dispose();
    }
}
