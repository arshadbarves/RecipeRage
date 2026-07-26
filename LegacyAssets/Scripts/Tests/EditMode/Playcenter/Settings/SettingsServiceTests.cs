using System.Threading.Tasks;
using KitchenClash.Infrastructure.Settings;
using NUnit.Framework;
using Playcenter.Services;

namespace KitchenClash.Tests.EditMode.Playcenter.Settings
{
    public sealed class SettingsServiceTests
    {
        [Test]
        public void Apply_CopiesFieldsIntoCurrent()
        {
            var store = new DictionarySettingsStore();
            var service = new PlayerPrefsSettingsService(store);

            service.Apply(new GameSettings
            {
                MasterVolume = 0.5f,
                MusicVolume = 0.25f,
                SfxVolume = 0.75f,
                ReduceMotion = true,
                LanguageCode = "es"
            });

            Assert.AreEqual(0.5f, service.Current.MasterVolume, 0.0001f);
            Assert.AreEqual(0.25f, service.Current.MusicVolume, 0.0001f);
            Assert.AreEqual(0.75f, service.Current.SfxVolume, 0.0001f);
            Assert.IsTrue(service.Current.ReduceMotion);
            Assert.AreEqual("es", service.Current.LanguageCode);
        }

        [Test]
        public void Apply_ClampsVolumesToUnitRange()
        {
            var service = new PlayerPrefsSettingsService(new DictionarySettingsStore());

            service.Apply(new GameSettings
            {
                MasterVolume = 2f,
                MusicVolume = -1f,
                SfxVolume = 1.5f
            });

            Assert.AreEqual(1f, service.Current.MasterVolume, 0.0001f);
            Assert.AreEqual(0f, service.Current.MusicVolume, 0.0001f);
            Assert.AreEqual(1f, service.Current.SfxVolume, 0.0001f);
        }

        [Test]
        public async Task SaveAndLoad_RoundTripsThroughStore()
        {
            var store = new DictionarySettingsStore();
            var writer = new PlayerPrefsSettingsService(store);
            writer.Apply(new GameSettings
            {
                MasterVolume = 0.4f,
                MusicVolume = 0.6f,
                SfxVolume = 0.2f,
                ReduceMotion = true,
                LanguageCode = "fr"
            });
            await writer.SaveAsync();

            Assert.AreEqual(1, store.SaveCallCount);
            Assert.AreEqual(0.4f, store.GetFloat(PlayerPrefsSettingsService.MasterVolumeKey, -1f), 0.0001f);
            Assert.AreEqual(1, store.GetInt(PlayerPrefsSettingsService.ReduceMotionKey, 0));
            Assert.AreEqual("fr", store.GetString(PlayerPrefsSettingsService.LanguageCodeKey, ""));

            var reader = new PlayerPrefsSettingsService(store);
            await reader.LoadAsync();

            Assert.AreEqual(0.4f, reader.Current.MasterVolume, 0.0001f);
            Assert.AreEqual(0.6f, reader.Current.MusicVolume, 0.0001f);
            Assert.AreEqual(0.2f, reader.Current.SfxVolume, 0.0001f);
            Assert.IsTrue(reader.Current.ReduceMotion);
            Assert.AreEqual("fr", reader.Current.LanguageCode);
        }

        [Test]
        public void Constructor_LoadsDefaultsWhenStoreEmpty()
        {
            var service = new PlayerPrefsSettingsService(new DictionarySettingsStore());

            Assert.AreEqual(1f, service.Current.MasterVolume, 0.0001f);
            Assert.AreEqual(1f, service.Current.MusicVolume, 0.0001f);
            Assert.AreEqual(1f, service.Current.SfxVolume, 0.0001f);
            Assert.IsFalse(service.Current.ReduceMotion);
            Assert.AreEqual("en", service.Current.LanguageCode);
        }
    }
}
