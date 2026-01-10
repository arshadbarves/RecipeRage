using Gameplay.Characters;
using Core.UI.Core;
using Core.Session;
using VContainer;

namespace Gameplay.UI.Features.Character
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