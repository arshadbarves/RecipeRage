using KitchenClash.Application;
using NUnit.Framework;

namespace KitchenClash.Tests.EditMode
{
    public class LobbyOpResultTests
    {
        [Test]
        public void Ok_IsSuccess_WithNoError()
        {
            var result = LobbyOpResult.Ok();
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.ErrorCode);
        }

        [Test]
        public void Fail_IsNotSuccess_AndPreservesCode()
        {
            var result = LobbyOpResult.Fail("TimedOut", "lobby create timed out");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("TimedOut", result.ErrorCode);
            Assert.AreEqual("lobby create timed out", result.Message);
        }
    }
}
