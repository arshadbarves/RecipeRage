using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Auth orchestrator. Facebook/Google SDK sign-in is wired in Slice 2; Guest
    /// sign-in is fully functional now (persistent anonymous ID). Until provider
    /// SDKs land, Facebook/Google fall back to guest with a logged warning so the
    /// whole game is playable end-to-end.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private const string GuestIdKey = "auth_guest_id";
        private const string ProviderKey = "auth_provider";

        private readonly ISaveService _save;
        private readonly ILoggingService _log;
        private readonly IAnalyticsService _analytics;

        public bool IsReady { get; private set; }
        public bool IsSignedIn { get; private set; }
        public string UserId { get; private set; }
        public string DisplayName { get; private set; }

        public AuthService(ISaveService save, ILoggingService log, IAnalyticsService analytics)
        {
            _save = save;
            _log = log;
            _analytics = analytics;
        }

        public IEnumerator Initialize()
        {
            // Restore previous session
            var savedId = _save.Load(GuestIdKey, string.Empty);
            if (!string.IsNullOrEmpty(savedId))
            {
                UserId = savedId;
                DisplayName = "Guest";
                IsSignedIn = true;
                _log.Log($"[Auth] Restored guest session: {savedId}");
            }

            IsReady = true;
            yield break;
        }

        public async Task<AuthResult> SignInWithFacebook()
        {
            // Facebook SDK sign-in wired in Slice 2. Fallback: guest.
            _log.LogWarning("[Auth] Facebook SDK not wired yet — signing in as guest");
            return await SignInAsGuest();
        }

        public async Task<AuthResult> SignInWithGoogle()
        {
            // Google SDK sign-in wired in Slice 2. Fallback: guest.
            _log.LogWarning("[Auth] Google SDK not wired yet — signing in as guest");
            return await SignInAsGuest();
        }

        public Task<AuthResult> SignInAsGuest()
        {
            var guestId = _save.Load(GuestIdKey, string.Empty);
            if (string.IsNullOrEmpty(guestId))
            {
                guestId = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 12);
                _save.Save(GuestIdKey, guestId);
            }

            UserId = guestId;
            DisplayName = "Guest";
            IsSignedIn = true;
            _save.Save(ProviderKey, "guest");

            _analytics.TrackEvent("auth_sign_in", new Dictionary<string, object> { { "provider", "guest" } });
            return Task.FromResult(new AuthResult(true, guestId, DisplayName));
        }

        public void SignOut()
        {
            IsSignedIn = false;
            UserId = null;
            DisplayName = null;
            _analytics.TrackEvent("auth_sign_out");
        }
    }
}
