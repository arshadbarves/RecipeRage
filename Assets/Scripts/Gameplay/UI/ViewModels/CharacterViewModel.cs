using Modules.Shared.Interfaces; // Added
using Gameplay.Characters;
using Modules.UI.Core;
using VContainer;

namespace Gameplay.UI.ViewModels
{
    public class CharacterViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private ICharacterService CharacterService => _sessionManager.SessionContainer?.Resolve<ICharacterService>();

        [Inject]
        public CharacterViewModel(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }
    }
}