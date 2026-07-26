using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Portable auth contract. Guest + social link; product user id is backend-agnostic.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResult> LoginAsGuestAsync();
        Task<AuthResult> LoginWithGoogleAsync();
        Task<AuthResult> LoginWithFacebookAsync();
        Task<AuthResult> LoginWithAppleAsync();
        Task LinkToGoogleAsync();
        Task LogoutAsync();
        string ProductUserId { get; }
        bool IsGuest { get; }
    }
}
