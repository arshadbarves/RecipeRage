using System.Collections.Generic;
using System.Threading.Tasks;
using Audio.Config;
using Audio.Manager;
using Audio.Music;
using Audio.SFX;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Audio.Core
{
    public class AudioService : IStartable
    {
        private readonly AudioContainer _audioContainer;
        private readonly AudioManager _audioManager;
        private readonly AudioMixer _audioMixer;
        private readonly Dictionary<string, PlayList> _playlists;
        private readonly Dictionary<string, SfxLibrary> _sfxLibraries;

        private bool _isInitialized;

        [Inject]
        public AudioService(AudioManager audioManager, AudioMixer audioMixer, AudioContainer audioContainer)
        {
            _audioManager = audioManager;
            _audioMixer = audioMixer;
            _audioContainer = audioContainer;

            _playlists = new Dictionary<string, PlayList>();
            _sfxLibraries = new Dictionary<string, SfxLibrary>();
        }

        public void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            InitializePlaylists();
            InitializeSfxLibraries();
            SetupDefaultAudioSettings();

            _isInitialized = true;
        }

        private void InitializePlaylists()
        {
            foreach (AudioContainer.PlaylistContainer playlistContainer in _audioContainer.playlists)
            {
                RegisterPlaylist(playlistContainer.key, playlistContainer.playlist);
            }
        }

        private void InitializeSfxLibraries()
        {
            foreach (AudioContainer.SfxLibraryContainer libraryContainer in _audioContainer.sfxLibraries)
            {
                RegisterSfxLibrary(libraryContainer.key, libraryContainer.library);
            }
        }

        private void SetupDefaultAudioSettings()
        {
            // Load saved settings or apply defaults
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

            _audioMixer.SetVolume("MasterVolume", masterVolume);
            _audioMixer.SetVolume("MusicVolume", musicVolume);
            _audioMixer.SetVolume("SFXVolume", sfxVolume);
        }

    #region Playlist Management

        public void RegisterPlaylist(string key, PlayList playlist)
        {
            if (playlist != null)
            {
                _playlists.TryAdd(key, playlist);
            }
        }

        public void TransitionToPlaylist(string playlistKey, float fadeOutDuration = 1f, float fadeInDuration = 1f)
        {
            if (!_playlists.TryGetValue(playlistKey, out PlayList playlist))
                return;

            _audioManager.StopMusic(fadeOutDuration);
            _audioManager.PlayMusic(playlist.GetNextTrack().clip, fadeInDuration);
        }

    #endregion

    #region SFX Management

        public void RegisterSfxLibrary(string key, SfxLibrary library)
        {
            if (library != null && !_sfxLibraries.ContainsKey(key))
            {
                library.Initialize();
                _sfxLibraries.Add(key, library);
            }
        }

        public async Task PlayGameSound(string eventName, Vector3 position)
        {
            foreach (SfxLibrary library in _sfxLibraries.Values)
            {
                SfxLibrary.AudioClipData clipData = library.GetClipData(eventName);
                if (clipData != null)
                {
                    await _audioManager.PlaySfx(clipData.clip, position, clipData.volume, clipData.spatialize);
                    break;
                }
            }
        }

        public void PlayUISound(string eventName)
        {
            if (_sfxLibraries.TryGetValue("UI", out SfxLibrary uiLibrary))
            {
                SfxLibrary.AudioClipData clipData = uiLibrary.GetClipData(eventName);
                if (clipData != null)
                {
                    _audioManager.PlaySFXOneShot(clipData.clip, clipData.volume);
                }
            }
        }

    #endregion

    #region Audio State Management

        public async Task TransitionToGameplayAudio()
        {
            await _audioMixer.FadeOut(0.5f);
            _audioMixer.TransitionToSnapshot("GameplayMix");
            TransitionToPlaylist("Gameplay");
            await _audioMixer.FadeIn(0.5f);
        }

        public async Task TransitionToMenuAudio()
        {
            await _audioMixer.FadeOut(0.5f);
            _audioMixer.TransitionToSnapshot("MenuMix");
            TransitionToPlaylist("MainMenu");
            await _audioMixer.FadeIn(0.5f);
        }

        public void PauseGameAudio()
        {
            _audioMixer.TransitionToSnapshot("PausedMix", 0.3f);
            _audioManager.PauseMusic();
        }

        public void ResumeGameAudio()
        {
            _audioMixer.TransitionToSnapshot("GameplayMix", 0.3f);
            _audioManager.ResumeMusic();
        }

    #endregion
    }
}