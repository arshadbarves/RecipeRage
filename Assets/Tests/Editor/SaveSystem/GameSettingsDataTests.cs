using NUnit.Framework;
using Core.SaveSystem;
using System.Reflection;

namespace Tests.Editor.SaveSystem
{
    public class GameSettingsDataTests
    {
        [Test]
        public void GameSettingsData_HasNewGraphicsProperties()
        {
            var data = new GameSettingsData();
            var type = data.GetType();

            Assert.IsNotNull(type.GetField("TargetFrameRate"), "TargetFrameRate field is missing");
            Assert.IsNotNull(type.GetField("ShadowsEnabled"), "ShadowsEnabled field is missing");
            Assert.IsNotNull(type.GetField("BloomEnabled"), "BloomEnabled field is missing");
        }

        [Test]
        public void GameSettingsData_NewPropertiesHaveCorrectDefaults()
        {
            var data = new GameSettingsData();
            var type = data.GetType();

            // We expect these defaults based on typical mobile settings
            // TargetFrameRate default 60
            // Shadows default true
            // Bloom default true

            // Only check values if fields exist (to avoid null ref in test if previous test fails)
            var fpsField = type.GetField("TargetFrameRate");
            if (fpsField != null)
            {
                Assert.AreEqual(60, fpsField.GetValue(data));
            }

            var shadowsField = type.GetField("ShadowsEnabled");
            if (shadowsField != null)
            {
                Assert.AreEqual(true, shadowsField.GetValue(data));
            }

            var bloomField = type.GetField("BloomEnabled");
            if (bloomField != null)
            {
                Assert.AreEqual(true, bloomField.GetValue(data));
            }
        }
    }
}
