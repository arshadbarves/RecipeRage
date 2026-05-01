using System;
using System.Threading.Tasks;
using KitchenClash.Domain;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// GDD Section 3: Auth via EOS Connect. No Firebase Auth.
    /// Google/Facebook/Apple tokens link directly to EOS Connect.
    /// Guest = EOS Device ID.
    /// </summary>
    public sealed class EOSAuthService : IAuthService
    {
        private readonly EOSManager _eos;
        private string _productUserId;
        private bool _isGuest;

        public EOSAuthService()
        {
            _eos = EOSManager.Instance;
        }

        public string ProductUserId => _productUserId;
        public bool IsGuest => _isGuest;
        public event Action<AuthResult> OnAuthChanged;

        public async Task<AuthResult> LoginAsGuestAsync()
        {
            try
            {
                var options = new CreateDeviceIdOptions { DeviceModel = "Unity" };
                // Create device ID then login
                var connectInterface = _eos.GetEOSConnectInterface();

                var loginOptions = new LoginOptions
                {
                    Credentials = new Credentials
                    {
                        Type = ExternalCredentialType.DeviceidAccessToken,
                        Token = null
                    }
                };

                var tcs = new TaskCompletionSource<AuthResult>();
                connectInterface.Login(ref loginOptions, null, (ref LoginCallbackInfo info) =>
                {
                    if (info.ResultCode == Result.Success)
                    {
                        _productUserId = info.LocalUserId.ToString();
                        _isGuest = true;
                        var result = new AuthResult(true, _productUserId, isGuest: true);
                        OnAuthChanged?.Invoke(result);
                        tcs.SetResult(result);
                    }
                    else
                    {
                        tcs.SetResult(AuthResult.Failed(info.ResultCode.ToString()));
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                return AuthResult.Failed(ex.Message);
            }
        }

        public async Task<AuthResult> LoginWithGoogleAsync()
        {
            // GDD: GoogleSignIn.DefaultInstance.SignIn() -> idToken
            // Then EOS Connect.Login(ExternalCredentialType.GoogleIdToken, token)
            // TODO: Integrate Google Sign-In SDK
            return await Task.FromResult(AuthResult.Failed("Google login not yet implemented"));
        }

        public async Task<AuthResult> LoginWithFacebookAsync()
        {
            // GDD: ExternalCredentialType.FacebookAccessToken
            // TODO: Integrate Facebook SDK
            return await Task.FromResult(AuthResult.Failed("Facebook login not yet implemented"));
        }

        public async Task<AuthResult> LoginWithAppleAsync()
        {
            // GDD: ExternalCredentialType.AppleIdToken
            // TODO: Integrate Apple Sign-In SDK
            return await Task.FromResult(AuthResult.Failed("Apple login not yet implemented"));
        }

        public async Task LinkToGoogleAsync()
        {
            // GDD: EOS Connect.LinkAccount(DeviceId PUID -> Google PUID)
            // TODO: Implement account linking
            await Task.CompletedTask;
        }
    }
}
