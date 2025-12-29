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
using Core.Events;

namespace RecipeRage.Modules.Auth.Tests
{
    public class AuthServiceTests
    {
        [Test]
        public void ServiceContainer_Resolves_IAuthService()
        {
            // Arrange
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

        [Test]
        public void EOSAuthService_CanBeInstantiated()
        {
            // Arrange
            var eventBus = new EventBus(); // Using real EventBus since it's simple
            
            // Act
            var service = new EOSAuthService(eventBus);
            
            // Assert
            Assert.IsNotNull(service);
        }
    }
}