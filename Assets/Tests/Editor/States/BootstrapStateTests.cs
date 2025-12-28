using NUnit.Framework;
using Core.State.States;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.RemoteConfig;
using Core.Update;
using Core.Authentication;
using UI;
using UI.Screens;
using UI.Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using Core.State;
using UnityEngine;
using Tests.Editor.Mocks;

namespace Tests.Editor.States
{
    public class BootstrapStateTests
    {
        private BootstrapState _bootstrapState;
        private MockUIService _mockUIService;
        private MockNTPTimeService _mockNTP;
        private MockRemoteConfigService _mockRemoteConfig;
        private MockAuthenticationService _mockAuth;
        private MockMaintenanceService _mockMaintenance;
        private MockGameStateManager _mockStateManager;
        private MockEventBus _mockEventBus;

        [SetUp]
        public void Setup()
        {
            _mockUIService = new MockUIService();
            _mockNTP = new MockNTPTimeService();
            _mockRemoteConfig = new MockRemoteConfigService();
            _mockAuth = new MockAuthenticationService();
            _mockMaintenance = new MockMaintenanceService();
            _mockStateManager = new MockGameStateManager();
            _mockEventBus = new MockEventBus();

            // We pass null for ServiceContainer as we don't expect it to be used in the tested flow
            // except for creating the next state, which we verify via MockStateManager
            _bootstrapState = new BootstrapState(
                _mockUIService,
                _mockNTP,
                _mockRemoteConfig,
                _mockAuth,
                _mockMaintenance,
                _mockStateManager,
                _mockEventBus,
                null
            );
        }

        [Test]
        public void Enter_StartsInitializationSequence()
        {
            // Act
            _bootstrapState.Enter();

            // Assert
            // 1. Shows Splash
            Assert.IsTrue(_mockUIService.ShownScreens.Contains(UIScreenType.Splash));
        }
    }
}
