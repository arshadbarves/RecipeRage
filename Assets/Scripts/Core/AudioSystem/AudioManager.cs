using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Core.Patterns;
using Core.SaveSystem;

namespace Core.AudioSystem
{
    /// <summary>
    /// Manages all audio in the game.
    /// This class is a singleton that persists throughout the game.
    /// </summary>
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _ambienceSource;

        [Header("Volume Parameters")]
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _musicVolumeParam = "MusicVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";

        [Header("Audio Clips")]
        [SerializeField] private AudioClipLibrary _audioClipLibrary;

        [Header("Default Clips")]
        [SerializeField] private string _mainMenuMusicName = "MainMenuMusic";
        [SerializeField] private string _gameplayMusicName = "GameplayMusic";
        [SerializeField] private string _victoryMusicName = "VictoryMusic";
        [SerializeField] private string _defeatMusicName = "DefeatMusic";

        // Pool of audio sources for sound effects
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        private int _sfxPoolSize = 10;

        // Currently playing music
        private AudioClip _currentMusic;

        // Save manager reference
        private SaveManager _saveManager;

        /// <summary>
        /// Initialize the audio manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize SFX pool
            InitializeSfxPool();

            // Initialize audio clip library
            if (_audioClipLibrary != null)
            {
                _audioClipLibrary.Initialize();
            }

            // Get save manager reference
            _saveManager = SaveManager.Instance;

            // Subscribe to settings changed event
            if (_saveManager != null)
            {
                _saveManager.OnSettingsChanged += ApplyAudioSettings;
            }
        }

        /// <summary>
        /// Apply audio settings when enabled.
        /// </summary>
        private void OnEnable()
        {
            ApplyAudioSettings();
        }

        /// <summary>
        /// Unsubscribe from events when destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from settings changed event
            if (_saveManager != null)
            {
                _saveManager.OnSettingsChanged -= ApplyAudioSettings;
            }
        }

        /// <summary>
        /// Initialize the SFX audio source pool.
        /// </summary>
        private void InitializeSfxPool()
        {
            // Create a parent object for SFX sources
            GameObject sfxParent = new GameObject("SFX Pool");
            sfxParent.transform.SetParent(transform);

            // Create pool of audio sources
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                GameObject sfxObject = new GameObject($"SFX Source {i}");
                sfxObject.transform.SetParent(sfxParent.transform);

                AudioSource source = sfxObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;

                // Set output to SFX mixer group if available
                if (_audioMixer != null)
                {
                    AudioMixerGroup[] groups = _audioMixer.FindMatchingGroups("SFX");
                    if (groups.Length > 0)
                    {
                        source.outputAudioMixerGroup = groups[0];
                    }
                }

                _sfxPool.Add(source);
            }
        }

        /// <summary>
        /// Apply audio settings from the save manager.
        /// </summary>
        private void ApplyAudioSettings()
        {
            if (_saveManager == null)
            {
                return;
            }

            PlayerSettings settings = _saveManager.GetSettings();

            // Apply master volume
            SetMasterVolume(settings.MasterVolume);

            // Apply music volume
            SetMusicVolume(settings.MusicVolume);

            // Apply SFX volume
            SetSfxVolume(settings.SfxVolume);

            // Apply mute setting
            SetMuted(settings.Muted);
        }

        #region Volume Control

        /// <summary>
        /// Set the master volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            if (_audioMixer != null)
            {
                // Convert to logarithmic scale for better volume control
                float dbVolume = volume > 0.001f ? 20f * Mathf.Log10(volume) : -80f;
                _audioMixer.SetFloat(_masterVolumeParam, dbVolume);
            }

            // Update music source volume directly as fallback
            if (_musicSource != null)
            {
                _musicSource.volume = volume;
            }

            // Update ambience source volume directly as fallback
            if (_ambienceSource != null)
            {
                _ambienceSource.volume = volume;
            }
        }

        /// <summary>
        /// Set the music volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMusicVolume(float volume)
        {
            if (_audioMixer != null)
            {
                // Convert to logarithmic scale for better volume control
                float dbVolume = volume > 0.001f ? 20f * Mathf.Log10(volume) : -80f;
                _audioMixer.SetFloat(_musicVolumeParam, dbVolume);
            }
        }

        /// <summary>
        /// Set the SFX volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetSfxVolume(float volume)
        {
            if (_audioMixer != null)
            {
                // Convert to logarithmic scale for better volume control
                float dbVolume = volume > 0.001f ? 20f * Mathf.Log10(volume) : -80f;
                _audioMixer.SetFloat(_sfxVolumeParam, dbVolume);
            }
        }

        /// <summary>
        /// Set whether audio is muted.
        /// </summary>
        /// <param name="muted">Whether audio is muted</param>
        public void SetMuted(bool muted)
        {
            if (_audioMixer != null)
            {
                float dbVolume = muted ? -80f : 0f;
                _audioMixer.SetFloat(_masterVolumeParam, dbVolume);
            }

            // Update music source directly as fallback
            if (_musicSource != null)
            {
                _musicSource.mute = muted;
            }

            // Update ambience source directly as fallback
            if (_ambienceSource != null)
            {
                _ambienceSource.mute = muted;
            }

            // Update SFX sources
            foreach (AudioSource source in _sfxPool)
            {
                source.mute = muted;
            }
        }

        #endregion

        #region Music Playback

        /// <summary>
        /// Play music.
        /// </summary>
        /// <param name="clip">The music clip to play</param>
        /// <param name="fadeTime">Time to fade in (seconds)</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusic(AudioClip clip, float fadeTime = 1f, bool loop = true)
        {
            if (_musicSource == null || clip == null)
            {
                return;
            }

            // Don't restart if already playing this clip
            if (_currentMusic == clip && _musicSource.isPlaying)
            {
                return;
            }

            _currentMusic = clip;

            // Set up music source
            _musicSource.clip = clip;
            _musicSource.loop = loop;

            // Start playing
            _musicSource.Play();

            // TODO: Implement fade-in using DOTween or coroutines
        }

        /// <summary>
        /// Play main menu music.
        /// </summary>
        public void PlayMainMenuMusic()
        {
            PlayMusicByName(_mainMenuMusicName);
        }

        /// <summary>
        /// Play gameplay music.
        /// </summary>
        public void PlayGameplayMusic()
        {
            PlayMusicByName(_gameplayMusicName);
        }

        /// <summary>
        /// Play victory music.
        /// </summary>
        public void PlayVictoryMusic()
        {
            PlayMusicByName(_victoryMusicName, 0.5f, false);
        }

        /// <summary>
        /// Play defeat music.
        /// </summary>
        public void PlayDefeatMusic()
        {
            PlayMusicByName(_defeatMusicName, 0.5f, false);
        }

        /// <summary>
        /// Play music by name from the audio clip library.
        /// </summary>
        /// <param name="name">The name of the music clip</param>
        /// <param name="fadeTime">Time to fade in (seconds)</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusicByName(string name, float fadeTime = 1f, bool loop = true)
        {
            if (_audioClipLibrary == null)
            {
                Debug.LogWarning("[AudioManager] Audio clip library is not assigned");
                return;
            }

            AudioClipLibrary.AudioClipEntry entry = _audioClipLibrary.GetClipEntry(name);

            if (entry != null && entry.Clip != null)
            {
                PlayMusic(entry.Clip, fadeTime, loop || entry.Loop);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Music clip '{name}' not found in library");
            }
        }

        /// <summary>
        /// Stop music.
        /// </summary>
        /// <param name="fadeTime">Time to fade out (seconds)</param>
        public void StopMusic(float fadeTime = 1f)
        {
            if (_musicSource == null)
            {
                return;
            }

            // TODO: Implement fade-out using DOTween or coroutines

            _musicSource.Stop();
            _currentMusic = null;
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Play a sound effect by name from the audio clip library.
        /// </summary>
        /// <param name="name">The name of the sound effect clip</param>
        /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
        /// <param name="pitchMultiplier">Pitch multiplier (0.5-1.5)</param>
        /// <returns>The audio source playing the sound</returns>
        public AudioSource PlaySfxByName(string name, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
        {
            if (_audioClipLibrary == null)
            {
                Debug.LogWarning("[AudioManager] Audio clip library is not assigned");
                return null;
            }

            AudioClipLibrary.AudioClipEntry entry = _audioClipLibrary.GetClipEntry(name);

            if (entry != null && entry.Clip != null)
            {
                return PlaySfx(entry.Clip, entry.Volume * volumeMultiplier, entry.Pitch * pitchMultiplier);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX clip '{name}' not found in library");
                return null;
            }
        }

        /// <summary>
        /// Play a sound effect by name at a specific position in 3D space.
        /// </summary>
        /// <param name="name">The name of the sound effect clip</param>
        /// <param name="position">The position to play the sound at</param>
        /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
        /// <param name="pitchMultiplier">Pitch multiplier (0.5-1.5)</param>
        /// <param name="spatialBlend">How much the sound is affected by 3D space (0-1)</param>
        /// <returns>The audio source playing the sound</returns>
        public AudioSource PlaySfxAtPositionByName(string name, Vector3 position, float volumeMultiplier = 1f, float pitchMultiplier = 1f, float spatialBlend = 1f)
        {
            if (_audioClipLibrary == null)
            {
                Debug.LogWarning("[AudioManager] Audio clip library is not assigned");
                return null;
            }

            AudioClipLibrary.AudioClipEntry entry = _audioClipLibrary.GetClipEntry(name);

            if (entry != null && entry.Clip != null)
            {
                return PlaySfxAtPosition(entry.Clip, position, entry.Volume * volumeMultiplier, entry.Pitch * pitchMultiplier, spatialBlend);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX clip '{name}' not found in library");
                return null;
            }
        }

        /// <summary>
        /// Play a sound effect.
        /// </summary>
        /// <param name="clip">The sound effect clip to play</param>
        /// <param name="volume">Volume level (0-1)</param>
        /// <param name="pitch">Pitch level (0.5-1.5)</param>
        /// <returns>The audio source playing the sound</returns>
        public AudioSource PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                return null;
            }

            // Find an available audio source
            AudioSource source = GetAvailableSfxSource();

            if (source != null)
            {
                // Set up the audio source
                source.clip = clip;
                source.volume = volume;
                source.pitch = pitch;
                source.loop = false;

                // Play the sound
                source.Play();
            }

            return source;
        }

        /// <summary>
        /// Play a sound effect at a specific position in 3D space.
        /// </summary>
        /// <param name="clip">The sound effect clip to play</param>
        /// <param name="position">The position to play the sound at</param>
        /// <param name="volume">Volume level (0-1)</param>
        /// <param name="pitch">Pitch level (0.5-1.5)</param>
        /// <param name="spatialBlend">How much the sound is affected by 3D space (0-1)</param>
        /// <returns>The audio source playing the sound</returns>
        public AudioSource PlaySfxAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
        {
            if (clip == null)
            {
                return null;
            }

            // Find an available audio source
            AudioSource source = GetAvailableSfxSource();

            if (source != null)
            {
                // Set up the audio source
                source.transform.position = position;
                source.clip = clip;
                source.volume = volume;
                source.pitch = pitch;
                source.spatialBlend = spatialBlend;
                source.loop = false;

                // Play the sound
                source.Play();
            }

            return source;
        }

        /// <summary>
        /// Get an available audio source from the pool.
        /// </summary>
        /// <returns>An available audio source, or null if none are available</returns>
        private AudioSource GetAvailableSfxSource()
        {
            // First, try to find an audio source that's not playing
            foreach (AudioSource source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // If all sources are playing, find the one that's closest to finishing
            AudioSource oldestSource = null;
            float shortestTimeRemaining = float.MaxValue;

            foreach (AudioSource source in _sfxPool)
            {
                float timeRemaining = source.clip.length - source.time;

                if (timeRemaining < shortestTimeRemaining)
                {
                    shortestTimeRemaining = timeRemaining;
                    oldestSource = source;
                }
            }

            return oldestSource;
        }

        #endregion
    }
}
