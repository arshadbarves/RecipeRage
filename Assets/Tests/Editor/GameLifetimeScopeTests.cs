using NUnit.Framework;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Core.Bootstrap;
using Core.Events;
using Core.Animation;
using Core.RemoteConfig;
using Core.SaveSystem;
using UI;
using System.Reflection;
using RecipeRage.Modules.Auth.Core;

namespace Tests.Editor
{
    public class GameLifetimeScopeTests
    {
        private class TestableGameLifetimeScope : GameLifetimeScope
        {
            public new void Configure(IContainerBuilder builder)
            {
                base.Configure(builder);
            }
        }

        [Test]
        public void Configure_RegistersExpectedServices()
        {
            // Arrange
            var go = new GameObject("TestScope");
            var scope = go.AddComponent<TestableGameLifetimeScope>();
            var uiDocProvider = new GameObject("UIDocProvider").AddComponent<UIDocumentProvider>();

            // Inject private field _uiDocumentProvider
            var field = typeof(GameLifetimeScope).GetField("_uiDocumentProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "Field _uiDocumentProvider not found");
            field.SetValue(scope, uiDocProvider);

            var builder = new ContainerBuilder();

            // Act
            scope.Configure(builder);
            var container = builder.Build();

            // Assert
            Assert.DoesNotThrow(() => container.Resolve<IEventBus>(), "Failed to resolve IEventBus");
            Assert.DoesNotThrow(() => container.Resolve<ISaveService>(), "Failed to resolve ISaveService");
            Assert.DoesNotThrow(() => container.Resolve<INTPTimeService>(), "Failed to resolve INTPTimeService");
            Assert.DoesNotThrow(() => container.Resolve<IRemoteConfigService>(), "Failed to resolve IRemoteConfigService");
            Assert.DoesNotThrow(() => container.Resolve<IAnimationService>(), "Failed to resolve IAnimationService");
            Assert.DoesNotThrow(() => container.Resolve<IUIService>(), "Failed to resolve IUIService");
            
            // Check UIDocumentProvider registration (component registration)
            // It registers the instance
            Assert.DoesNotThrow(() => container.Resolve<UIDocumentProvider>(), "Failed to resolve UIDocumentProvider");
            
            // Cleanup
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(uiDocProvider.gameObject);
        }
    }
}
