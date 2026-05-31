using KitchenClash.Application.Models;
using KitchenClash.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using UnityEngine;
using VContainer;

namespace KitchenClash.Infrastructure.Gameplay
{
    public class SkinsService : ISkinsService, IDisposable
    {
        public event Action<int, string> OnSkinEquipped;

        private const string CHARACTERS_PATH = "ScriptableObjects/CharacterClasses";

        private readonly Dictionary<string, SkinItem> _skinsById = new Dictionary<string, SkinItem>();
        private readonly Dictionary<int, List<SkinItem>> _skinsByCharacter = new Dictionary<int, List<SkinItem>>();
        private readonly EconomyService _economyService;

        [Inject]
        public SkinsService(EconomyService economyService)
        {
            _economyService = economyService;
            LoadSkinsFromCharacterClasses();
        }

        private void LoadSkinsFromCharacterClasses()
        {
            CharacterClass[] characterClasses = Resources.LoadAll<CharacterClass>(CHARACTERS_PATH);
            if (characterClasses == null || characterClasses.Length == 0)
            {
                return;
            }

            foreach (CharacterClass character in characterClasses)
            {
                if (character.Skins == null || character.Skins.Count == 0)
                {
                    continue;
                }

                foreach (SkinItem skin in character.Skins)
                {
                    if (string.IsNullOrEmpty(skin.id))
                    {
                        continue;
                    }

                    _skinsById[skin.id] = skin;
                    if (!_skinsByCharacter.ContainsKey(character.Id))
                    {
                        _skinsByCharacter[character.Id] = new List<SkinItem>();
                    }

                    _skinsByCharacter[character.Id].Add(skin);
                }
            }

            GameLogger.Log($"Total skins loaded: {_skinsById.Count}");
        }

        public List<SkinItem> GetAllSkins() => _skinsById.Values.ToList();

        public List<SkinItem> GetSkinsForCharacter(int characterId)
        {
            return _skinsByCharacter.TryGetValue(characterId, out List<SkinItem> skins) ? skins : new List<SkinItem>();
        }

        public SkinItem GetDefaultSkinForCharacter(int characterId)
        {
            List<SkinItem> skins = GetSkinsForCharacter(characterId);
            return skins.FirstOrDefault(s => s.isDefault);
        }

        public SkinItem GetSkin(string skinId) => _skinsById.TryGetValue(skinId, out SkinItem skin) ? skin : null;
        public bool IsSkinUnlocked(string skinId) => _economyService.HasItem(skinId);

        public bool UnlockSkin(string skinId)
        {
            if (!_skinsById.ContainsKey(skinId))
            {
                return false;
            }

            SkinItem skin = _skinsById[skinId];
            return _economyService.Purchase(skinId, skin.Price, EconomyKeys.CurrencyCoins);
        }

        public SkinItem GetEquippedSkin(int characterId) => GetDefaultSkinForCharacter(characterId);

        public bool EquipSkin(int characterId, string skinId)
        {
            if (!IsSkinUnlocked(skinId))
            {
                return false;
            }

            OnSkinEquipped?.Invoke(characterId, skinId);
            return true;
        }

        public void Dispose()
        {
            _skinsById.Clear();
            _skinsByCharacter.Clear();
        }
    }

    // ISkinsService is defined in Domain/Interfaces/ISkinsService.cs
}
