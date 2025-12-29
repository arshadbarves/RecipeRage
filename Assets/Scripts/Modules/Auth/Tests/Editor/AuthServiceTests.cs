using NUnit.Framework;
using Core.Bootstrap;
using RecipeRage.Modules.Auth.Core;
using RecipeRage.Modules.Auth.Tests;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UI;

namespace RecipeRage.Modules.Auth.Tests
{
    public class AuthServiceTests
    {
        [Test]
        public void ServiceContainer_Resolves_IAuthService()
        {
            // Arrange
            // We need a UIDocumentProvider to create ServiceContainer
            var go = new GameObject("TestObject");
            var uiDoc = go.AddComponent<UIDocument>();
            var provider = go.AddComponent<UIDocumentProvider>();
            
            // Act
            var container = new ServiceContainer(provider);
            
            // Assert
            Assert.IsNotNull(container.AuthService, "AuthService should be initialized");
            Assert.IsInstanceOf<MockAuthService>(container.AuthService, "AuthService should be MockAuthService");
            
            // Cleanup
            container.Dispose();
            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator MockAuthService_Login_ReturnsTrue() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var authService = new MockAuthService();
            
            // Act
            var result = await authService.LoginAsync(AuthType.DeviceID);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(authService.IsLoggedIn());
        });
    }
}
