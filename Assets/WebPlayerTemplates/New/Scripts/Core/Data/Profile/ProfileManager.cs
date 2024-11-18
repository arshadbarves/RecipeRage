using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data.Save;
using UnityEngine;
using VContainer;

namespace Core.Data.Profile
{
    public class ProfileManager
    {
        private readonly Dictionary<string, PlayerProfile> _loadedProfiles;
        private readonly SaveManager _saveManager;

        [Inject]
        public ProfileManager(SaveManager saveManager)
        {
            _saveManager = saveManager;
            _loadedProfiles = new Dictionary<string, PlayerProfile>();
        }

        public PlayerProfile CurrentProfile { get; private set; }

        public async Task Initialize()
        {
            // Load last used profile ID
            string lastProfileId = PlayerPrefs.GetString("LastProfileId", null);
            if (!string.IsNullOrEmpty(lastProfileId))
            {
                await LoadProfile(lastProfileId);
            }
        }

        public async Task<PlayerProfile> LoadProfile(string profileId)
        {
            if (_loadedProfiles.TryGetValue(profileId, out PlayerProfile profile))
            {
                CurrentProfile = profile;
                return profile;
            }

            profile = new PlayerProfile(profileId, _saveManager);
            await profile.Initialize();

            _loadedProfiles[profileId] = profile;
            CurrentProfile = profile;

            PlayerPrefs.SetString("LastProfileId", profileId);
            PlayerPrefs.Save();

            return profile;
        }

        public async Task<PlayerProfile> CreateProfile(string profileId)
        {
            if (_loadedProfiles.ContainsKey(profileId))
            {
                throw new Exception($"Profile {profileId} already exists");
            }

            PlayerProfile profile = new PlayerProfile(profileId, _saveManager);
            await profile.Initialize();

            _loadedProfiles[profileId] = profile;
            CurrentProfile = profile;

            PlayerPrefs.SetString("LastProfileId", profileId);
            PlayerPrefs.Save();

            return profile;
        }

        public async Task SaveAllProfiles()
        {
            foreach (PlayerProfile profile in _loadedProfiles.Values)
            {
                await profile.SaveProfile();
            }
        }

        public void UnloadProfile(string profileId)
        {
            if (_loadedProfiles.ContainsKey(profileId))
            {
                _loadedProfiles.Remove(profileId);
                if (CurrentProfile?.Data.playerId == profileId)
                {
                    CurrentProfile = null;
                }
            }
        }
    }
}