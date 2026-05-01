using System;
using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsGuestAsync();
        Task<AuthResult> LoginWithGoogleAsync();
        Task<AuthResult> LoginWithFacebookAsync();
        Task<AuthResult> LoginWithAppleAsync();
        Task LinkToGoogleAsync();
        string ProductUserId { get; }
        bool IsGuest { get; }
        event Action<AuthResult> OnAuthChanged;
    }
}
