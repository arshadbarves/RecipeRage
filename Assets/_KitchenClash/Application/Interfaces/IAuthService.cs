using KitchenClash.Domain;
using System.Threading.Tasks;

namespace KitchenClash.Application
{
    /// <summary>
    /// GDD v3 Auth interface. EOS Connect primary, social logins link to EOS.
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
