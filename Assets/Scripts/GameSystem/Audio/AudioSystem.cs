using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace GameSystem.Audio
{
    public class AudioSystem : IGameSystem
    {
        [Inject] private IAudioService _audioService;
        public async Task InitializeAsync()
        {
            await _audioService.InitializeAsync();
            Debug.Log("Audio System Initialized");
        }

        public void Update()
        {
        }


        public Task CleanupAsync()
        {
            return _audioService.CleanupAsync();
        }

        public void PlaySfx(string sfxName)
        {
            _audioService.PlaySfx(sfxName);
        }
        public float GetMusicVolume()
        {
            return _audioService.GetVolume(AudioChannelType.Music);
        }
        public void MuteMusic()
        {
            _audioService.SetVolume(AudioChannelType.Music, 0);
        }
        public void UnmuteMusic()
        {
            _audioService.SetVolume(AudioChannelType.Music, 1);
        }

        public float GetSfxVolume()
        {
            return _audioService.GetVolume(AudioChannelType.SFX);
        }
        public void MuteSfx()
        {
            _audioService.SetVolume(AudioChannelType.SFX, 0);
        }
        public void UnmuteSfx()
        {
            _audioService.SetVolume(AudioChannelType.SFX, 1);
        }
        public void SetMusicPlaylist(List<string> playlist, bool shuffle = false)
        {
            _audioService.SetMusicPlaylist(playlist, shuffle);
        }
    }
}