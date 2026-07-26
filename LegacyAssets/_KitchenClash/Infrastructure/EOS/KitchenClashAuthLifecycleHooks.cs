using KitchenClash.Application;
using KitchenClash.Domain;
using Playcenter.EOS;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Game-side auth side effects: settings + domain events.
    /// </summary>
    public sealed class KitchenClashAuthLifecycleHooks : IAuthLifecycleHooks
    {
        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;

        public KitchenClashAuthLifecycleHooks(IEventBus eventBus, ISaveService saveService)
        {
            _eventBus = eventBus;
            _saveService = saveService;
        }

        public void OnLoginSucceeded(string productUserId, string displayName, bool isGuest, string loginMethod)
        {
            _saveService?.OnUserLoggedIn();
            _saveService?.UpdateSettings(s => s.LastLoginMethod = loginMethod);
            _eventBus?.Publish(new LoginSuccessEvent
            {
                UserId = productUserId,
                DisplayName = displayName
            });
        }

        public void OnLogout(string productUserId)
        {
            _saveService?.OnUserLoggedOut();
            _eventBus?.Publish(new LogoutEvent { UserId = productUserId ?? "unknown" });
        }
    }
}
