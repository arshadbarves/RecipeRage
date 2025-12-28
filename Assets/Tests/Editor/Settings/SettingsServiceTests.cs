using NUnit.Framework;
using Core.Settings;
using Core.SaveSystem;
using Core.Audio;
using UnityEngine;
using System;
using Tests.Editor.Mocks;

namespace Tests.Editor.Settings
{
    public class SettingsServiceTests
    {
        private SettingsService _service;
        private MockAudioService _mockAudio;
        private MockSaveService _mockSave;

        [SetUp]
        public void Setup()
        {
            _mockAudio = new MockAudioService();
            _mockSave = new MockSaveService();
            _service = new SettingsService(_mockAudio, _mockSave);
        }

        [Test]
        public void ApplyGraphicsSettings_SetsQualityLevel()
        {
            var data = new GameSettingsData { GraphicsQuality = 1 };
            _service.ApplyGraphicsSettings(data);
            
            Assert.AreEqual(1, QualitySettings.GetQualityLevel());
        }

        [Test]
        public void ApplyAudioSettings_CallsAudioService()
        {
            var data = new GameSettingsData { MasterVolume = 0.5f, IsMuted = true };
            _service.ApplyAudioSettings(data);
            
            Assert.AreEqual(0.5f, _mockAudio.MasterVolume);
            Assert.IsTrue(_mockAudio.IsMuted);
        }
    }

    public class MockAudioService : IAudioService
    {
        public float MasterVolume;
        public float MusicVolume;
        public float SFXVolume;
        public bool IsMuted;

        public void PlayMusic(AudioClip clip, float fadeTime = 1) { }
        public void StopMusic(float fadeTime = 1) { }
        public void PauseMusic() { }
        public void ResumeMusic() { }
        public AudioSource PlaySFX(AudioClip clip, float pitch = 1) => null;
        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position) => null;
        public void StopSound(AudioSource source, float fadeTime = 0) { }
        
        public void SetMasterVolume(float volume) => MasterVolume = volume;
        public void SetMusicVolume(float volume) => MusicVolume = volume;
        public void SetSFXVolume(float volume) => SFXVolume = volume;
        public void SetMute(bool mute) => IsMuted = mute;
    }
}
