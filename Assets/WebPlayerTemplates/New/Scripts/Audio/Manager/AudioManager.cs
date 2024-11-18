using System.Threading.Tasks;
using Audio.Config;
using Audio.Music;
using Audio.SFX;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Audio.Manager
{
    public class AudioManager : MonoBehaviour
    {
        private const int PoolInitialSize = 10;
        private AudioConfig _config;
        private MusicPlayer _musicPlayer;
        private SfxPlayer _sfxPlayer;

        private ObjectPool<AudioSource> _sourcePool;

        private void OnDestroy()
        {
            StopAllAudio();
            _sourcePool.Clear();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (Application.isMobilePlatform)
            {
                if (pauseStatus)
                {
                    PauseMusic();
                    _sfxPlayer.StopAllSfx();
                }
                else
                {
                    ResumeMusic();
                }
            }
        }

        [Inject]
        public void Construct(AudioConfig config, AudioMixer audioMixer)
        {
            _config = config;
            InitializeAudioSystem();
        }

        private void InitializeAudioSystem()
        {
            _sourcePool = new ObjectPool<AudioSource>(
                CreateAudioSource,
                EnableAudioSource,
                DisableAudioSource,
                DestroyAudioSource,
                defaultCapacity: PoolInitialSize,
                maxSize: _config.maxSimultaneousSounds
            );

            _musicPlayer = new MusicPlayer(_config, _sourcePool);
            _sfxPlayer = new SfxPlayer(_config, _sourcePool);

            OptimizeForMobile();
        }

        private void OptimizeForMobile()
        {
            if (Application.isMobilePlatform)
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();
                config.sampleRate = _config.mobileAudioSampleRate;
                config.speakerMode = AudioSpeakerMode.Stereo;
                config.dspBufferSize = 0;
                config.numRealVoices = _config.maxSimultaneousSounds;
                config.numVirtualVoices = _config.maxSimultaneousSounds * 2;
                AudioSettings.Reset(config);
            }
        }

        private AudioSource CreateAudioSource()
        {
            GameObject audioObj = new GameObject("AudioSource");
            audioObj.transform.SetParent(transform);
            AudioSource source = audioObj.AddComponent<AudioSource>();
            ConfigureAudioSource(source);
            return source;
        }

        private void ConfigureAudioSource(AudioSource source)
        {
            source.spatialBlend = _config.spatialBlend;
            source.minDistance = _config.minDistance;
            source.maxDistance = _config.maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.dopplerLevel = 0f;
        }

        private void EnableAudioSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }

        private void DisableAudioSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }

        private void DestroyAudioSource(AudioSource source)
        {
            if (source != null)
            {
                Destroy(source.gameObject);
            }
        }

        public void StopAllAudio()
        {
            StopMusic();
            _sfxPlayer.StopAllSfx();
        }

    #region Music Methods

        public void PlayMusic(AudioClip musicClip, float fadeInDuration = 1f)
        {
            _musicPlayer.PlayMusic(musicClip, fadeInDuration);
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            _musicPlayer.StopMusic(fadeOutDuration);
        }

        public void PauseMusic()
        {
            _musicPlayer.PauseMusic();
        }

        public void ResumeMusic()
        {
            _musicPlayer.ResumeMusic();
        }

    #endregion

    #region SFX Methods

        public async Task PlaySfx(AudioClip clip, Vector3 position, float volume = 1f, bool spatialize = true)
        {
            await _sfxPlayer.PlaySfx(clip, position, volume, spatialize);
        }

        public void PlaySFXOneShot(AudioClip clip, float volume = 1f)
        {
            _sfxPlayer.PlaySFXOneShot(clip, volume);
        }

    #endregion
    }
}