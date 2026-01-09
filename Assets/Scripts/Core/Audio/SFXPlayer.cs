using System.Collections;
using Core.Core.Shared.Utilities;
using UnityEngine;

namespace Core.Core.Audio
{
    /// <summary>
    /// Handles SFX playback using audio pool
    /// </summary>
    public class SFXPlayer : ISFXPlayer
    {
        private readonly AudioPoolManager _poolManager;
        private readonly IAudioVolumeController _volumeController;

        public SFXPlayer(AudioPoolManager poolManager, IAudioVolumeController volumeController)
        {
            _poolManager = poolManager;
            _volumeController = volumeController;
        }

        public AudioSource PlaySFX(AudioClip clip, float pitch = 1f)
        {
            if (clip == null) return null;

            AudioSource source = _poolManager.GetAudioSource();
            if (source == null) return null;

            source.clip = clip;
            source.volume = _volumeController.GetSFXVolume();
            source.pitch = pitch;
            source.spatialBlend = 0f;
            source.Play();

            // Return to pool after clip finishes
            CoroutineRunner.Run(ReturnToPoolAfterPlay(source, clip.length));

            return source;
        }

        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return null;

            AudioSource source = _poolManager.GetAudioSource();
            if (source == null) return null;

            source.clip = clip;
            source.volume = _volumeController.GetSFXVolume();
            source.pitch = 1f;
            source.spatialBlend = 1f;
            source.transform.position = position;
            source.Play();

            CoroutineRunner.Run(ReturnToPoolAfterPlay(source, clip.length));

            return source;
        }

        public void StopSound(AudioSource source, float fadeTime)
        {
            if (source == null) return;

            if (fadeTime > 0f)
            {
                CoroutineRunner.Run(FadeOutAndReturn(source, fadeTime));
            }
            else
            {
                _poolManager.ReturnAudioSource(source);
            }
        }

        private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (source != null)
            {
                _poolManager.ReturnAudioSource(source);
            }
        }

        private IEnumerator FadeOutAndReturn(AudioSource source, float fadeTime)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime && source != null)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            if (source != null)
            {
                _poolManager.ReturnAudioSource(source);
            }
        }
    }
}
