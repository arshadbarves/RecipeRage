using Core.Characters;
using UI.Core;
using VContainer;

namespace UI.ViewModels
{
    public class CharacterViewModel : BaseViewModel
    {
        private readonly ICharacterService _characterService;

        [Inject]
        public CharacterViewModel(ICharacterService characterService)
        {
            _characterService = characterService;
        }
    }
}