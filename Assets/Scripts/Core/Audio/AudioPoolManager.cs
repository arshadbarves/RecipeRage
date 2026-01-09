using System;
using System.Collections.Generic;
using Core.Core.Logging;
using UnityEngine;

namespace Core.Core.Audio
{
    /// <summary>
    /// Manages audio source pooling - requires Transform for GameObject parenting
    /// </summary>
    public class AudioPoolManager
    {
        private readonly Transform _sourcesContainer;
        private readonly Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
        private readonly Dictionary<AudioSource, bool> _activeAudioSources = new Dictionary<AudioSource, bool>();
        private readonly int _initialPoolSize = 10;
        private readonly int _maxPoolSize = 20;

        public AudioPoolManager(Transform poolContainer)
        {
            if (poolContainer == null)
            {
                throw new ArgumentNullException(nameof(poolContainer));
            }

            GameObject containerObj = new GameObject("AudioPool");
            containerObj.transform.SetParent(poolContainer, false);
            containerObj.transform.localPosition = Vector3.zero;
            _sourcesContainer = containerObj.transform;

            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                AudioSource source = CreateAudioSource($"SFX_Pool_{i}");
                _sfxPool.Enqueue(source);
            }

            GameLogger.Log($"Initialized pool with {_initialPoolSize} audio sources");
        }

        private AudioSource CreateAudioSource(string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(_sourcesContainer, false);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            return source;
        }

        public AudioSource GetAudioSource()
        {
            AudioSource source = null;

            if (_sfxPool.Count > 0)
            {
                source = _sfxPool.Dequeue();
            }
            else if (_activeAudioSources.Count < _maxPoolSize)
            {
                source = CreateAudioSource($"SFX_Pool_{_activeAudioSources.Count}");
            }

            if (source != null)
            {
                source.gameObject.SetActive(true);
                _activeAudioSources[source] = true;
            }

            return source;
        }

        public void ReturnAudioSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.transform.position = _sourcesContainer.position;
            source.gameObject.SetActive(false);

            _activeAudioSources.Remove(source);

            _sfxPool.Enqueue(source);
        }
    }
}
