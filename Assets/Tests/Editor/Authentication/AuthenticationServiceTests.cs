using NUnit.Framework;
using Core.Authentication;
using Core.Events;
using Core.Maintenance;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Tests.Editor.Mocks;

namespace Tests.Editor.Authentication
{
    public class AuthenticationServiceTests
    {
        private AuthenticationService _authService;
        private MockSaveService _mockSaveService;
        private MockEventBus _mockEventBus;
        private MockMaintenanceService _mockMaintenanceService;
        private MockEOSWrapper _mockEOSWrapper;
        private bool _sessionCreated;

        [SetUp]
        public void Setup()
        {
            _mockSaveService = new MockSaveService();
            _mockEventBus = new MockEventBus();
            _mockMaintenanceService = new MockMaintenanceService();
            _mockEOSWrapper = new MockEOSWrapper();
            _sessionCreated = false;

            _authService = new AuthenticationService(
                _mockSaveService,
                _mockEventBus,
                _mockMaintenanceService,
                _mockEOSWrapper,
                () => _sessionCreated = true
            );
        }

        [Test]
        public async void InitializeAsync_WhenEOSNotReady_FailsGracefully()
        {
            _mockEOSWrapper.IsReady = false;
            _mockSaveService.Settings.LastLoginMethod = "DeviceID";

            bool result = await _authService.InitializeAsync();

            Assert.IsFalse(result);
            Assert.IsFalse(_sessionCreated);
        }

        [Test]
        public async void LoginAsGuestAsync_WhenEOSNotReady_ReturnsFalse_And_PublishesError()
        {
            _mockEOSWrapper.IsReady = false;

            bool result = await _authService.LoginAsGuestAsync();

            Assert.IsFalse(result);
            Assert.IsNotNull(_mockEventBus.LastPublishedEvent);
            Assert.IsInstanceOf<LoginFailedEvent>(_mockEventBus.LastPublishedEvent);
            Assert.AreEqual("EOS SDK not initialized", ((LoginFailedEvent)_mockEventBus.LastPublishedEvent).Error);
        }
    }
}