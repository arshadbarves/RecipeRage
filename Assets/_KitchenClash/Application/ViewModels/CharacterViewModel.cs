using Gameplay.Characters;
using KitchenClash.Presentation.Common;
using Core.Session;
using VContainer;

namespace KitchenClash.Application.ViewModels
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
