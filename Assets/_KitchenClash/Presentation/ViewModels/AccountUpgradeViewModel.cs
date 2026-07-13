using System;
using Cysharp.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using VContainer;

namespace KitchenClash.Presentation.ViewModels
{
    /// <summary>
    /// Drives the AccountUpgradeScreen shown once after tutorial completion.
    ///
    /// Responsibilities:
    ///   • Provide trophy count + guest display name as social proof
    ///   • Attempt Google / Facebook / Apple linking via IAuthService
    ///   • Handle "Continue as Guest" (persists HasSeenAccountUpgradePrompt)
    ///   • Publish AccountUpgradeResultEvent so AccountUpgradeState can transition
    ///
    /// On any path (link or guest) the flag is saved so the screen never
    /// surfaces as a popup again (available in Settings only).
    /// </summary>
    public class AccountUpgradeViewModel : BaseViewModel
    {
        private readonly IAuthService       _authService;
        private readonly IEventBus          _eventBus;
        private readonly IPlayerDataService _playerDataService;
        private readonly ITrophyService     _trophyService;
        private readonly ILocalizationManager _localization;

        // ── Bindable properties ───────────────────────────────────────────
        public BindableProperty<bool>   IsLinking      { get; } = new(false);
        public BindableProperty<string> StatusText     { get; } = new("");
        public BindableProperty<int>    TrophyCount    { get; } = new(0);
        public BindableProperty<bool>   GoogleEnabled  { get; } = new(true);
        public BindableProperty<bool>   FacebookEnabled{ get; } = new(true);
        /// <summary>Apple sign-in only surfaces on iOS.</summary>
        public BindableProperty<bool>   AppleVisible   { get; } = new(false);

        [Inject]
        public AccountUpgradeViewModel(
            IAuthService        authService,
            IEventBus           eventBus,
            IPlayerDataService  playerDataService,
            ITrophyService      trophyService,
            ILocalizationManager localization)
        {
            _authService       = authService;
            _eventBus          = eventBus;
            _playerDataService = playerDataService;
            _trophyService     = trophyService;
            _localization      = localization;
        }

        public override void Initialize()
        {
            base.Initialize();

            TrophyCount.Value   = _trophyService?.CurrentTrophies ?? 0;
            StatusText.Value    = Loc("upgrade_status_ready") ?? "Save your progress";

#if UNITY_IOS
            AppleVisible.Value = true;
#else
            AppleVisible.Value = false;
#endif
        }

        // ── Link commands (called by screen buttons) ──────────────────────

        public void LinkWithGoogle()   => LinkAsync("Google",   async () => await _authService.LinkToGoogleAsync().AsUniTask()).Forget();
        public void LinkWithFacebook() => LinkAsync("Facebook", async () => await _authService.LoginWithFacebookAsync().AsUniTask()).Forget();
        public void LinkWithApple()    => LinkAsync("Apple",    async () => await _authService.LoginWithAppleAsync().AsUniTask()).Forget();

        /// <summary>
        /// Player explicitly chose not to link now.
        /// Marks the prompt as seen and continues to the main lobby as guest.
        /// The option remains available in Settings → "Link Account".
        /// </summary>
        public void ContinueAsGuest()
        {
            MarkPromptSeen();
            _eventBus.Publish(new AccountUpgradeResultEvent(
                linked: false,
                provider: "Guest",
                productUserId: _authService.ProductUserId));
        }

        // ── Internal ──────────────────────────────────────────────────────

        private async UniTaskVoid LinkAsync(string provider, Func<UniTask> linkAction)
        {
            if (IsLinking.Value) return;

            IsLinking.Value     = true;
            StatusText.Value    = (Loc("upgrade_status_linking") ?? "Linking account...").ToUpper();
            DisableAllButtons();

            try
            {
                await linkAction();

                StatusText.Value = (Loc("upgrade_status_linked") ?? "Account linked!").ToUpper();
                MarkPromptSeen();

                _eventBus.Publish(new AccountUpgradeResultEvent(
                    linked: true,
                    provider: provider,
                    productUserId: _authService.ProductUserId));
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[AccountUpgradeViewModel] {provider} link failed: {ex.Message}");
                StatusText.Value = (Loc("upgrade_status_failed") ?? "Linking failed — try again").ToUpper();
                IsLinking.Value  = false;
                EnableAllButtons();
            }
        }

        private void MarkPromptSeen()
        {
            // Persist the flag so we never show the popup again
            var progress = _playerDataService.GetProgress();
            if (progress != null)
            {
                progress.HasSeenAccountUpgradePrompt = true;
                // The progress is saved by PlayerDataService on next save cycle
            }
        }

        private void DisableAllButtons()
        {
            GoogleEnabled.Value   = false;
            FacebookEnabled.Value = false;
        }

        private void EnableAllButtons()
        {
            GoogleEnabled.Value   = true;
            FacebookEnabled.Value = true;
        }

        private string Loc(string key) => _localization?.GetText(key);
    }
}
