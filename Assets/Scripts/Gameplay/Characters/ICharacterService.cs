using System;

namespace Gameplay.Characters
{
    public interface ICharacterService
    {
        CharacterClass SelectedCharacter { get; }
        CharacterClass[] GetAvailableCharacters();
        CharacterClass[] GetUnlockedCharacters();
        CharacterClass GetCharacter(int id);
        bool IsUnlocked(int characterId);
        bool Unlock(int characterId);
        bool SelectCharacter(int characterId);
        event Action<CharacterClass> OnCharacterSelected;
        event Action<int> OnCharacterUnlocked;
    }
}
