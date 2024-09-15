using System.Collections;
using System.Collections.Generic;
using Core.EventSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Netcode;

namespace Tests
{
    [EventType("TestLocalEvent")]
    public class TestLocalEventArgs : IEventArgs
    {
        public int Value { get; set; }
    }

    [EventType("TestNetworkEvent", isReliable: true)]
    public class TestNetworkEventArgs : INetworkEventArgs
    {
        public int Value { get; set; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int tempValue = Value;
            serializer.SerializeValue(ref tempValue);
            Value = tempValue;
        }
    }

    public class EventManagerTests
    {
        private GameObject _eventManagerObject;
        private EventManager _eventManager;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            _eventManagerObject = new GameObject("EventManager");
            _eventManager = _eventManagerObject.AddComponent<EventManager>();

            // Create and set EventManagerConfig
            var config = ScriptableObject.CreateInstance<EventManagerConfig>();
            config.networkEventProcessInterval = 0.1f;
            config.maxBatchSize = 10;

            _eventManager.SetConfig(config);

            // Wait for a frame to ensure everything is initialized
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestLocalEventTrigger()
        {
            int callCount = 0;
            TestLocalEventArgs receivedArgs = null;

            _eventManager.AddLocalListener<TestLocalEventArgs>(args =>
            {
                callCount++;
                receivedArgs = args;
            });

            var eventArgs = new TestLocalEventArgs { Value = 42 };
            _eventManager.TriggerLocalEvent(eventArgs);

            // Wait for a frame to ensure event processing
            yield return null;

            Assert.AreEqual(1, callCount);
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(42, receivedArgs.Value);
        }

        [UnityTest]
        public IEnumerator TestLocalEventTriggerAsync()
        {
            int callCount = 0;
            TestLocalEventArgs receivedArgs = null;

            _eventManager.AddLocalListener<TestLocalEventArgs>(args =>
            {
                callCount++;
                receivedArgs = args;
            });

            var eventArgs = new TestLocalEventArgs { Value = 42 };
            _eventManager.TriggerLocalEventAsync(eventArgs);

            // Wait for a frame to ensure async event processing
            yield return null;

            Assert.AreEqual(1, callCount);
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(42, receivedArgs.Value);
        }

        [UnityTest]
        public IEnumerator TestLocalEventPriority()
        {
            List<int> executionOrder = new List<int>();

            _eventManager.AddLocalListener<TestLocalEventArgs>(args => executionOrder.Add(2), 2);
            _eventManager.AddLocalListener<TestLocalEventArgs>(args => executionOrder.Add(1), 1);
            _eventManager.AddLocalListener<TestLocalEventArgs>(args => executionOrder.Add(3), 3);

            var eventArgs = new TestLocalEventArgs { Value = 42 };
            _eventManager.TriggerLocalEvent(eventArgs);

            // Wait for a frame to ensure event processing
            yield return null;

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, executionOrder);
        }

        [UnityTest]
        public IEnumerator TestRemoveLocalListener()
        {
            int callCount = 0;
            System.Action<TestLocalEventArgs> callback = args => callCount++;

            _eventManager.AddLocalListener<TestLocalEventArgs>(callback);
            _eventManager.TriggerLocalEvent(new TestLocalEventArgs());

            // Wait for a frame to ensure event processing
            yield return null;

            Assert.AreEqual(1, callCount);

            _eventManager.RemoveLocalListener<TestLocalEventArgs>(callback);
            _eventManager.TriggerLocalEvent(new TestLocalEventArgs());

            // Wait for another frame
            yield return null;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void TestEventTypeCache()
        {
            var eventType = typeof(TestNetworkEventArgs);
            var attribute = _eventManager.GetEventTypeAttribute(eventType);

            Assert.IsNotNull(attribute);
            Assert.AreEqual("TestNetworkEvent", attribute.EventName);
            Assert.IsTrue(attribute.IsReliable);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_eventManagerObject != null)
            {
                Object.DestroyImmediate(_eventManagerObject);
            }
            yield return null;
        }
    }
}