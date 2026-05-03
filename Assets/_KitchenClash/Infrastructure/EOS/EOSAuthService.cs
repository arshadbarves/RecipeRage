using System;
using System.Threading.Tasks;
using KitchenClash.Domain;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// DEPRECATED: Use AuthenticationService (IAuthService) instead.
    /// Kept temporarily for reference on social login stubs.
    /// GDD Section 3: Auth via EOS Connect. No Firebase Auth.
    /// Google/Facebook/Apple tokens link directly to EOS Connect.
    /// Guest = EOS Device ID.
    /// </summary>
    [System.Obsolete("Use AuthenticationService which implements IAuthService. Social login stubs from this class should be migrated as AuthType cases.")]
    public sealed class EOSAuthService
    {
        private string _productUserId;
        private bool _isGuest;

        public EOSAuthService()
        {
        }

        public string ProductUserId => _productUserId;
        public bool IsGuest => _isGuest;
        public event Action<AuthResult> OnAuthChanged;

        public async Task<AuthResult> LoginAsGuestAsync()
        {
            try
            {
                // NOTE: ConnectInterface access pattern may differ across EOS SDK versions.
                //       Verify EOSManager.Instance.GetEOSConnectInterface() is available in
                //       the version of PlayEveryWare.EpicOnlineServices in use.
                ConnectInterface connectInterface = EOSManager.Instance.GetEOSConnectInterface();
                if (connectInterface == null)
                {
                    return AuthResult.Failed("EOS Connect interface not available");
                }

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
            // REQUIRES: Google Sign-In Unity SDK (com.google.signin) and a matching
            //           Google OAuth client ID in google-services.json.
            //           Flow: GoogleSignIn.DefaultInstance.SignIn() → idToken
            //           → EOS Connect.Login(ExternalCredentialType.GoogleIdToken, token)
            return await Task.FromResult(AuthResult.Failed("Google login not yet implemented — Google Sign-In SDK required"));
        }

        public async Task<AuthResult> LoginWithFacebookAsync()
        {
            // REQUIRES: Facebook SDK for Unity (com.facebook.sdk).
            //           Flow: FB.LogInWithReadPermissions() → accessToken
            //           → EOS Connect.Login(ExternalCredentialType.FacebookAccessToken, token)
            return await Task.FromResult(AuthResult.Failed("Facebook login not yet implemented — Facebook SDK required"));
        }

        public async Task<AuthResult> LoginWithAppleAsync()
        {
            // REQUIRES: Apple Sign-In Unity plugin (e.g. com.lupidan.apple-signin-unity).
            //           Flow: AppleAuthManager.LoginWithAppleId() → idToken
            //           → EOS Connect.Login(ExternalCredentialType.AppleIdToken, token)
            return await Task.FromResult(AuthResult.Failed("Apple login not yet implemented — Apple Sign-In plugin required"));
        }

        public async Task LinkToGoogleAsync()
        {
            // REQUIRES: Google Sign-In SDK (same as LoginWithGoogleAsync).
            //           Flow: obtain Google idToken → EOS Connect.LinkAccount(DeviceId PUID → Google PUID)
            await Task.CompletedTask;
        }
    }
}
