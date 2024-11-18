using System.Collections.Generic;
using System.Threading.Tasks;
using Audio.Config;
using UnityEngine;
using UnityEngine.Pool;

namespace Audio.SFX
{

    public class SfxPlayer
    {
        private readonly List<AudioSource> _activeSfx = new List<AudioSource>();
        private readonly AudioConfig _config;
        private readonly ObjectPool<AudioSource> _sourcePool;

        public SfxPlayer(AudioConfig config, ObjectPool<AudioSource> sourcePool)
        {
            _config = config;
            _sourcePool = sourcePool;
        }

        public async Task PlaySfx(AudioClip clip, Vector3 position, float volume = 1f, bool spatialize = true)
        {
            if (clip == null) return;

            AudioSource source = _sourcePool.Get();
            _activeSfx.Add(source);

            source.transform.position = position;
            source.clip = clip;
            source.volume = 0f;
            source.spatialBlend = spatialize ? _config.spatialBlend : 0f;
            source.Play();

            // Quick ramp up
            float elapsed = 0f;
            float rampDuration = 0.05f;
            while (elapsed < rampDuration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, volume, elapsed / rampDuration);
                await Task.Yield();
            }

            // Wait for clip to finish
            await Task.Delay((int)(clip.length * 1000));

            // Quick ramp down and release
            elapsed = 0f;
            while (elapsed < rampDuration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(volume, 0f, elapsed / rampDuration);
                await Task.Yield();
            }

            if (source != null)
            {
                _activeSfx.Remove(source);
                _sourcePool.Release(source);
            }
        }

        public void PlaySFXOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = _sourcePool.Get();
            source.PlayOneShot(clip, volume);
            _sourcePool.Release(source);
        }

        public void StopAllSfx()
        {
            foreach (AudioSource source in _activeSfx)
            {
                if (source != null)
                {
                    source.Stop();
                    _sourcePool.Release(source);
                }
            }
            _activeSfx.Clear();
        }
    }
}