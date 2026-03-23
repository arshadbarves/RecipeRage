using Gameplay.Characters;
using Core.UI.Core;
using Core.Session;
using VContainer;

namespace Gameplay.UI.Features.Character
{
    public class CharacterViewModel : BaseViewModel
    {
        private readonly ISessionContext _sessionContext;
        private ICharacterService CharacterService => _sessionContext.CharacterService;

        [Inject]
        public CharacterViewModel(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }
    }
}
