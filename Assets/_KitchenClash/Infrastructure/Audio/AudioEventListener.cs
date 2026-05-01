using System;
using System.Collections.Generic;
using KitchenClash.Application;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Audio
{
    /// <summary>
    /// Bridge between domain SFX/Music events and the actual audio playback via IAudioService.
    /// Subscribes to IEventBus and delegates to IAudioService.
    /// </summary>
    public sealed class AudioEventListener : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IAudioService _audioService;
        private readonly AudioSettings _audioSettings;

        public AudioEventListener(IEventBus eventBus, IAudioService audioService, AudioSettings audioSettings)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _audioSettings = audioSettings;

            _eventBus.Subscribe<SFXEvent>(OnSFXEvent);
            _eventBus.Subscribe<MusicEvent>(OnMusicEvent);
        }

        private void OnSFXEvent(SFXEvent e)
        {
            AudioClip clip = _audioSettings.GetSFXClip(e.Type);
            if (clip != null)
            {
                _audioService.PlaySFX(clip);
            }
        }

        private void OnMusicEvent(MusicEvent e)
        {
            AudioClip clip = _audioSettings.GetMusicClip(e.Track);
            if (clip != null)
            {
                _audioService.PlayMusic(clip, e.FadeTime);
            }
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<SFXEvent>(OnSFXEvent);
            _eventBus.Unsubscribe<MusicEvent>(OnMusicEvent);
        }
    }
}
