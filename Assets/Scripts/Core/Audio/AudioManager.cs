using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Core.Patterns;
using Core.SaveSystem;

namespace Core.Audio
{
    /// <summary>
    /// Manages all audio in the game.
    /// </summary>
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _musicVolumeParam = "MusicVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";
        [SerializeField] private string _voiceVolumeParam = "VoiceVolume";
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private Transform _poolContainer;
        
        [Header("Audio Settings")]
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private int _maxPoolSize = 20;
        [SerializeField] private float _spatialBlend2D = 0f;
        [SerializeField] private float _spatialBlend3D = 1f;
        
        // Audio pools
        private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
        private Queue<AudioSource> _voicePool = new Queue<AudioSource>();
        private Dictionary<AudioSource, Coroutine> _activeAudioSources = new Dictionary<AudioSource, Coroutine>();
        
        // Currently playing music
        private AudioClip _currentMusic;
        private AudioClip _nextMusic;
        private Coroutine _musicFadeCoroutine;
        
        // Save manager reference
        private SaveManager _saveManager;
        
        /// <summary>
        /// Initialize the audio manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Create pool container if not set
            if (_poolContainer == null)
            {
                GameObject poolObj = new GameObject("AudioSourcePool");
                poolObj.transform.SetParent(transform);
                _poolContainer = poolObj.transform;
            }
            
            // Create music source if not set
            if (_musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f; // 2D sound
            }
            
            // Initialize audio pools
            InitializeAudioPools();
            
            Debug.Log("[AudioManager] Initialized");
        }
        
        /// <summary>
        /// Set up the audio manager.
        /// </summary>
        private void Start()
        {
            // Get save manager reference
            _saveManager = SaveManager.Instance;
            
            if (_saveManager != null)
            {
                // Load audio settings
                GameSettingsData settings = _saveManager.GetSettings();
                ApplyAudioSettings(settings);
                
                // Subscribe to settings changes
                _saveManager.OnSettingsChanged += ApplyAudioSettings;
            }
            else
            {
                Debug.LogWarning("[AudioManager] SaveManager not found. Audio settings will not be saved.");
            }
        }
        
        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_saveManager != null)
            {
                _saveManager.OnSettingsChanged -= ApplyAudioSettings;
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Play a music track.
        /// </summary>
        /// <param name="clip">The music clip to play</param>
        /// <param name="fadeTime">Time to fade in/out in seconds</param>
        /// <param name="volume">Volume of the music (0-1)</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusic(AudioClip clip, float fadeTime = 1f, float volume = 1f, bool loop = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null music clip");
                return;
            }
            
            // If the same music is already playing, don't restart it
            if (_currentMusic == clip && _musicSource.isPlaying)
            {
                return;
            }
            
            // Stop any existing fade
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }
            
            // If we're already playing music, fade it out and then start the new music
            if (_musicSource.isPlaying && fadeTime > 0f)
            {
                _nextMusic = clip;
                _musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(0f, fadeTime, () => {
                    _musicSource.clip = _nextMusic;
                    _musicSource.volume = 0f;
                    _musicSource.loop = loop;
                    _musicSource.Play();
                    _currentMusic = _nextMusic;
                    _nextMusic = null;
                    _musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(volume, fadeTime, null));
                }));
            }
            else
            {
                // No music playing or no fade time, just start the new music
                _musicSource.clip = clip;
                _musicSource.volume = fadeTime > 0f ? 0f : volume;
                _musicSource.loop = loop;
                _musicSource.Play();
                _currentMusic = clip;
                
                if (fadeTime > 0f)
                {
                    _musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(volume, fadeTime, null));
                }
            }
            
            Debug.Log($"[AudioManager] Playing music: {clip.name}");
        }
        
        /// <summary>
        /// Stop the currently playing music.
        /// </summary>
        /// <param name="fadeTime">Time to fade out in seconds</param>
        public void StopMusic(float fadeTime = 1f)
        {
            // Stop any existing fade
            if (_musicFadeCoroutine != null)
            {
                StopCoroutine(_musicFadeCoroutine);
            }
            
            if (_musicSource.isPlaying)
            {
                if (fadeTime > 0f)
                {
                    _musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(0f, fadeTime, () => {
                        _musicSource.Stop();
                        _currentMusic = null;
                    }));
                }
                else
                {
                    _musicSource.Stop();
                    _currentMusic = null;
                }
                
                Debug.Log("[AudioManager] Stopping music");
            }
        }
        
        /// <summary>
        /// Pause the currently playing music.
        /// </summary>
        public void PauseMusic()
        {
            if (_musicSource.isPlaying)
            {
                _musicSource.Pause();
                Debug.Log("[AudioManager] Pausing music");
            }
        }
        
        /// <summary>
        /// Resume the paused music.
        /// </summary>
        public void ResumeMusic()
        {
            if (!_musicSource.isPlaying && _currentMusic != null)
            {
                _musicSource.UnPause();
                Debug.Log("[AudioManager] Resuming music");
            }
        }
        
        /// <summary>
        /// Play a sound effect.
        /// </summary>
        /// <param name="clip">The sound clip to play</param>
        /// <param name="volume">Volume of the sound (0-1)</param>
        /// <param name="pitch">Pitch of the sound (0.5-1.5)</param>
        /// <param name="loop">Whether to loop the sound</param>
        /// <returns>The AudioSource playing the sound, or null if the sound couldn't be played</returns>
        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            return PlaySound(clip, SoundCategory.SFX, null, volume, pitch, _spatialBlend2D, loop);
        }
        
        /// <summary>
        /// Play a sound effect at a position in 3D space.
        /// </summary>
        /// <param name="clip">The sound clip to play</param>
        /// <param name="position">The position to play the sound at</param>
        /// <param name="volume">Volume of the sound (0-1)</param>
        /// <param name="pitch">Pitch of the sound (0.5-1.5)</param>
        /// <param name="spatialBlend">How much the sound is affected by 3D space (0-1)</param>
        /// <param name="loop">Whether to loop the sound</param>
        /// <returns>The AudioSource playing the sound, or null if the sound couldn't be played</returns>
        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float? spatialBlend = null, bool loop = false)
        {
            return PlaySound(clip, SoundCategory.SFX, position, volume, pitch, spatialBlend ?? _spatialBlend3D, loop);
        }
        
        /// <summary>
        /// Play a voice clip.
        /// </summary>
        /// <param name="clip">The voice clip to play</param>
        /// <param name="volume">Volume of the voice (0-1)</param>
        /// <param name="pitch">Pitch of the voice (0.5-1.5)</param>
        /// <returns>The AudioSource playing the voice, or null if the voice couldn't be played</returns>
        public AudioSource PlayVoice(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return PlaySound(clip, SoundCategory.Voice, null, volume, pitch, _spatialBlend2D, false);
        }
        
        /// <summary>
        /// Play a voice clip at a position in 3D space.
        /// </summary>
        /// <param name="clip">The voice clip to play</param>
        /// <param name="position">The position to play the voice at</param>
        /// <param name="volume">Volume of the voice (0-1)</param>
        /// <param name="pitch">Pitch of the voice (0.5-1.5)</param>
        /// <param name="spatialBlend">How much the voice is affected by 3D space (0-1)</param>
        /// <returns>The AudioSource playing the voice, or null if the voice couldn't be played</returns>
        public AudioSource PlayVoiceAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float? spatialBlend = null)
        {
            return PlaySound(clip, SoundCategory.Voice, position, volume, pitch, spatialBlend ?? _spatialBlend3D, false);
        }
        
        /// <summary>
        /// Play a UI sound effect.
        /// </summary>
        /// <param name="clip">The UI sound clip to play</param>
        /// <param name="volume">Volume of the sound (0-1)</param>
        /// <param name="pitch">Pitch of the sound (0.5-1.5)</param>
        /// <returns>The AudioSource playing the sound, or null if the sound couldn't be played</returns>
        public AudioSource PlayUISound(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return PlaySound(clip, SoundCategory.UI, null, volume, pitch, 0f, false);
        }
        
        /// <summary>
        /// Stop a sound.
        /// </summary>
        /// <param name="audioSource">The AudioSource to stop</param>
        /// <param name="fadeTime">Time to fade out in seconds</param>
        public void StopSound(AudioSource audioSource, float fadeTime = 0f)
        {
            if (audioSource == null)
            {
                return;
            }
            
            if (_activeAudioSources.TryGetValue(audioSource, out Coroutine fadeCoroutine))
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                
                if (fadeTime > 0f)
                {
                    _activeAudioSources[audioSource] = StartCoroutine(FadeSoundCoroutine(audioSource, 0f, fadeTime, () => {
                        ReturnAudioSourceToPool(audioSource);
                    }));
                }
                else
                {
                    ReturnAudioSourceToPool(audioSource);
                }
            }
        }
        
        /// <summary>
        /// Stop all sounds of a specific category.
        /// </summary>
        /// <param name="category">The category of sounds to stop</param>
        /// <param name="fadeTime">Time to fade out in seconds</param>
        public void StopAllSounds(SoundCategory category, float fadeTime = 0f)
        {
            List<AudioSource> sourcesToStop = new List<AudioSource>();
            
            foreach (var kvp in _activeAudioSources)
            {
                AudioSource source = kvp.Key;
                
                if (source.outputAudioMixerGroup != null && 
                    source.outputAudioMixerGroup.name.Contains(category.ToString()))
                {
                    sourcesToStop.Add(source);
                }
            }
            
            foreach (AudioSource source in sourcesToStop)
            {
                StopSound(source, fadeTime);
            }
        }
        
        /// <summary>
        /// Stop all sounds.
        /// </summary>
        /// <param name="fadeTime">Time to fade out in seconds</param>
        public void StopAllSounds(float fadeTime = 0f)
        {
            List<AudioSource> sourcesToStop = new List<AudioSource>(_activeAudioSources.Keys);
            
            foreach (AudioSource source in sourcesToStop)
            {
                StopSound(source, fadeTime);
            }
        }
        
        /// <summary>
        /// Set the master volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            SetMixerVolume(_masterVolumeParam, volume);
            
            // Update settings if save manager exists
            if (_saveManager != null)
            {
                _saveManager.UpdateSettings(settings => {
                    settings.MasterVolume = volume;
                });
            }
        }
        
        /// <summary>
        /// Set the music volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMusicVolume(float volume)
        {
            SetMixerVolume(_musicVolumeParam, volume);
            
            // Update settings if save manager exists
            if (_saveManager != null)
            {
                _saveManager.UpdateSettings(settings => {
                    settings.MusicVolume = volume;
                });
            }
        }
        
        /// <summary>
        /// Set the SFX volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            SetMixerVolume(_sfxVolumeParam, volume);
            
            // Update settings if save manager exists
            if (_saveManager != null)
            {
                _saveManager.UpdateSettings(settings => {
                    settings.SfxVolume = volume;
                });
            }
        }
        
        /// <summary>
        /// Set the voice volume.
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetVoiceVolume(float volume)
        {
            SetMixerVolume(_voiceVolumeParam, volume);
            
            // Update settings if save manager exists
            if (_saveManager != null)
            {
                _saveManager.UpdateSettings(settings => {
                    settings.VoiceVolume = volume;
                });
            }
        }
        
        /// <summary>
        /// Mute or unmute all audio.
        /// </summary>
        /// <param name="mute">Whether to mute the audio</param>
        public void SetMute(bool mute)
        {
            float volume = mute ? 0f : 1f;
            SetMixerVolume(_masterVolumeParam, volume);
            
            // Update settings if save manager exists
            if (_saveManager != null)
            {
                _saveManager.UpdateSettings(settings => {
                    settings.IsMuted = mute;
                });
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize the audio source pools.
        /// </summary>
        private void InitializeAudioPools()
        {
            // Create initial SFX pool
            for (int i = 0; i < _initialPoolSize; i++)
            {
                AudioSource source = CreateAudioSource("SFX_Pool_" + i, SoundCategory.SFX);
                _sfxPool.Enqueue(source);
            }
            
            // Create initial voice pool
            for (int i = 0; i < _initialPoolSize / 2; i++)
            {
                AudioSource source = CreateAudioSource("Voice_Pool_" + i, SoundCategory.Voice);
                _voicePool.Enqueue(source);
            }
            
            Debug.Log($"[AudioManager] Initialized audio pools: {_initialPoolSize} SFX, {_initialPoolSize / 2} Voice");
        }
        
        /// <summary>
        /// Create an audio source.
        /// </summary>
        /// <param name="name">Name of the audio source</param>
        /// <param name="category">Category of the audio source</param>
        /// <returns>The created audio source</returns>
        private AudioSource CreateAudioSource(string name, SoundCategory category)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(_poolContainer);
            
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D by default
            
            // Set the mixer group based on category
            if (_audioMixer != null)
            {
                AudioMixerGroup[] groups = _audioMixer.FindMatchingGroups(category.ToString());
                if (groups.Length > 0)
                {
                    source.outputAudioMixerGroup = groups[0];
                }
            }
            
            return source;
        }
        
        /// <summary>
        /// Get an audio source from the appropriate pool.
        /// </summary>
        /// <param name="category">Category of the audio source</param>
        /// <returns>An audio source from the pool</returns>
        private AudioSource GetAudioSourceFromPool(SoundCategory category)
        {
            AudioSource source = null;
            
            switch (category)
            {
                case SoundCategory.SFX:
                case SoundCategory.UI:
                    if (_sfxPool.Count > 0)
                    {
                        source = _sfxPool.Dequeue();
                    }
                    else if (_activeAudioSources.Count < _maxPoolSize)
                    {
                        source = CreateAudioSource("SFX_Pool_" + _sfxPool.Count, category);
                    }
                    break;
                    
                case SoundCategory.Voice:
                    if (_voicePool.Count > 0)
                    {
                        source = _voicePool.Dequeue();
                    }
                    else if (_activeAudioSources.Count < _maxPoolSize)
                    {
                        source = CreateAudioSource("Voice_Pool_" + _voicePool.Count, category);
                    }
                    break;
            }
            
            if (source != null)
            {
                source.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] No available audio sources for category: {category}");
            }
            
            return source;
        }
        
        /// <summary>
        /// Return an audio source to the appropriate pool.
        /// </summary>
        /// <param name="source">The audio source to return</param>
        private void ReturnAudioSourceToPool(AudioSource source)
        {
            if (source == null)
            {
                return;
            }
            
            // Remove from active sources
            if (_activeAudioSources.ContainsKey(source))
            {
                _activeAudioSources.Remove(source);
            }
            
            // Reset the audio source
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.transform.position = _poolContainer.position;
            
            // Return to the appropriate pool
            if (source.outputAudioMixerGroup != null)
            {
                string groupName = source.outputAudioMixerGroup.name;
                
                if (groupName.Contains("SFX") || groupName.Contains("UI"))
                {
                    _sfxPool.Enqueue(source);
                }
                else if (groupName.Contains("Voice"))
                {
                    _voicePool.Enqueue(source);
                }
            }
            else
            {
                // Default to SFX pool if no mixer group
                _sfxPool.Enqueue(source);
            }
            
            source.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Play a sound.
        /// </summary>
        /// <param name="clip">The sound clip to play</param>
        /// <param name="category">Category of the sound</param>
        /// <param name="position">Position to play the sound at (null for 2D)</param>
        /// <param name="volume">Volume of the sound (0-1)</param>
        /// <param name="pitch">Pitch of the sound (0.5-1.5)</param>
        /// <param name="spatialBlend">How much the sound is affected by 3D space (0-1)</param>
        /// <param name="loop">Whether to loop the sound</param>
        /// <returns>The AudioSource playing the sound, or null if the sound couldn't be played</returns>
        private AudioSource PlaySound(AudioClip clip, SoundCategory category, Vector3? position, float volume, float pitch, float spatialBlend, bool loop)
        {
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Attempted to play null {category} clip");
                return null;
            }
            
            // Get an audio source from the pool
            AudioSource source = GetAudioSourceFromPool(category);
            if (source == null)
            {
                return null;
            }
            
            // Set up the audio source
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = spatialBlend;
            source.loop = loop;
            
            // Set position if provided
            if (position.HasValue)
            {
                source.transform.position = position.Value;
            }
            else
            {
                source.transform.position = _poolContainer.position;
            }
            
            // Play the sound
            source.Play();
            
            // Add to active sources
            if (!loop)
            {
                // If not looping, return to pool after clip finishes
                _activeAudioSources[source] = StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
            }
            else
            {
                // If looping, just add to active sources without a coroutine
                _activeAudioSources[source] = null;
            }
            
            return source;
        }
        
        /// <summary>
        /// Return an audio source to the pool after it finishes playing.
        /// </summary>
        /// <param name="source">The audio source</param>
        /// <param name="delay">Delay before returning to pool</param>
        /// <returns>Coroutine</returns>
        private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (source != null)
            {
                ReturnAudioSourceToPool(source);
            }
        }
        
        /// <summary>
        /// Fade music volume.
        /// </summary>
        /// <param name="targetVolume">Target volume</param>
        /// <param name="duration">Duration of the fade</param>
        /// <param name="onComplete">Action to perform when fade completes</param>
        /// <returns>Coroutine</returns>
        private IEnumerator FadeMusicCoroutine(float targetVolume, float duration, Action onComplete)
        {
            float startVolume = _musicSource.volume;
            float time = 0f;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
                yield return null;
            }
            
            _musicSource.volume = targetVolume;
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Fade sound volume.
        /// </summary>
        /// <param name="source">The audio source</param>
        /// <param name="targetVolume">Target volume</param>
        /// <param name="duration">Duration of the fade</param>
        /// <param name="onComplete">Action to perform when fade completes</param>
        /// <returns>Coroutine</returns>
        private IEnumerator FadeSoundCoroutine(AudioSource source, float targetVolume, float duration, Action onComplete)
        {
            if (source == null)
            {
                yield break;
            }
            
            float startVolume = source.volume;
            float time = 0f;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Set a volume parameter on the audio mixer.
        /// </summary>
        /// <param name="paramName">Name of the mixer parameter</param>
        /// <param name="volume">Volume level (0-1)</param>
        private void SetMixerVolume(string paramName, float volume)
        {
            if (_audioMixer == null)
            {
                Debug.LogWarning("[AudioManager] Audio mixer not assigned");
                return;
            }
            
            // Convert linear volume (0-1) to decibels (-80-0)
            float decibelValue = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            _audioMixer.SetFloat(paramName, decibelValue);
        }
        
        /// <summary>
        /// Apply audio settings from the save system.
        /// </summary>
        /// <param name="settings">The game settings</param>
        private void ApplyAudioSettings(GameSettingsData settings)
        {
            // Apply master volume
            SetMixerVolume(_masterVolumeParam, settings.IsMuted ? 0f : settings.MasterVolume);
            
            // Apply category volumes
            SetMixerVolume(_musicVolumeParam, settings.MusicVolume);
            SetMixerVolume(_sfxVolumeParam, settings.SfxVolume);
            SetMixerVolume(_voiceVolumeParam, settings.VoiceVolume);
            
            Debug.Log("[AudioManager] Applied audio settings from save system");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Categories of sounds.
    /// </summary>
    public enum SoundCategory
    {
        Music,
        SFX,
        Voice,
        UI
    }
}
