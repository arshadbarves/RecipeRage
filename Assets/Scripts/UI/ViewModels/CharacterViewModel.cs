using Core.Bootstrap; // Added
using Core.Characters;
using UI.Core;
using VContainer;

namespace UI.ViewModels
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