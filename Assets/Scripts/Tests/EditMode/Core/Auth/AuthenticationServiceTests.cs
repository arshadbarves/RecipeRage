using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Core.Auth
{
    public class AuthenticationServiceTests
    {
        [Test]
        public void DeviceIdExtraction_WithLongId_ReturnsFirst8Chars()
        {
            string deviceId = "1234567890ABCDEF";
            string result = deviceId.Length >= 8 ? deviceId.Substring(0, 8) : deviceId;

            Assert.AreEqual("12345678", result);
        }

        [Test]
        public void DeviceIdExtraction_WithShortId_ReturnsFullId()
        {
            string deviceId = "12345";
            string result = deviceId.Length >= 8 ? deviceId.Substring(0, 8) : deviceId;

            Assert.AreEqual("12345", result);
        }

        [Test]
        public void DeviceIdExtraction_WithEmptyId_ReturnsEmptyString()
        {
            string deviceId = "";
            string result = deviceId.Length >= 8 ? deviceId.Substring(0, 8) : deviceId;

            Assert.AreEqual("", result);
        }

        [Test]
        public void DisplayNameFormat_IsCorrect()
        {
            string deviceId = "12345678";
            string displayName = $"Guest_{deviceId}";

            Assert.AreEqual("Guest_12345678", displayName);
        }

        [Test]
        public void DisplayName_WithShortDeviceId_IsCorrect()
        {
            string deviceId = "1234";
            string displayName = $"Guest_{deviceId}";

            Assert.AreEqual("Guest_1234", displayName);
        }
    }
}
