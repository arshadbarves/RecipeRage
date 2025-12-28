using NUnit.Framework;
using UI.Components.Tabs;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Core.Animation;
using System;

namespace Tests.Editor.UI
{
    public class TabSystemTests
    {
        private TabSystem _system;
        private VisualElement _root;
        private MockTabComponent _tab1;
        private MockTabComponent _tab2;
        private Button _btn1;
        private Button _btn2;

        [SetUp]
        public void Setup()
        {
            _root = new VisualElement();
            _system = new TabSystem(_root, new MockUIAnimator());
            
            _tab1 = new MockTabComponent("tab1");
            _tab2 = new MockTabComponent("tab2");
            
            _btn1 = new Button();
            _btn2 = new Button();
            
            _system.AddTab("tab1", _btn1, _tab1);
            _system.AddTab("tab2", _btn2, _tab2);
        }

        [Test]
        public void SwitchToTab_CallsOnShow_And_OnHide()
        {
            _system.SwitchToTab("tab1", true);
            Assert.IsTrue(_tab1.IsVisible);
            
            _system.SwitchToTab("tab2", true);
            Assert.IsFalse(_tab1.IsVisible);
            Assert.IsTrue(_tab2.IsVisible);
        }

        [Test]
        public void SwitchToTab_UpdatesButtonClasses()
        {
            _system.SwitchToTab("tab1", true);
            Assert.IsTrue(_btn1.ClassListContains("active"));
            
            _system.SwitchToTab("tab2", true);
            Assert.IsFalse(_btn1.ClassListContains("active"));
            Assert.IsTrue(_btn2.ClassListContains("active"));
        }
    }

    public class MockTabComponent : ITabComponent
    {
        public string TabId { get; }
        public bool IsVisible { get; private set; }
        public MockTabComponent(string id) => TabId = id;
        public void Initialize(VisualElement root) { }
        public void OnShow() => IsVisible = true;
        public void OnHide() => IsVisible = false;
        public void Update(float deltaTime) { }
        public void Dispose() { }
    }

    public class MockUIAnimator : IUIAnimator
    {
        public void FadeIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void FadeOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void SlideIn(VisualElement element, Vector2 from, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void SlideOut(VisualElement element, Vector2 to, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void ScaleIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void ScaleOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void PopupIn(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void PopupOut(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
        public void Pulse(VisualElement element, float duration, Action onComplete = null) { onComplete?.Invoke(); }
    }
}
