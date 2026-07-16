using System;
using NUnit.Framework;
using Playcenter.UI;

namespace KitchenClash.Tests.EditMode.Playcenter
{
    public class UIScreenStackManagerTests
    {
        private UIScreenStackManager _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new UIScreenStackManager();
        }

        [Test]
        public void Push_ThenPeek_ReturnsPushedType()
        {
            Type screen = typeof(string);

            _sut.Push(screen, UIScreenCategory.Screen);

            Assert.AreEqual(screen, _sut.Peek(UIScreenCategory.Screen));
            Assert.AreEqual(1, _sut.GetStackDepth(UIScreenCategory.Screen));
        }

        [Test]
        public void Pop_EmptyCategory_ReturnsNull()
        {
            Assert.IsNull(_sut.Pop(UIScreenCategory.Modal));
        }

        [Test]
        public void Pop_AfterPush_ReturnsTypeAndEmpties()
        {
            Type screen = typeof(int);

            _sut.Push(screen, UIScreenCategory.Popup);

            Assert.AreEqual(screen, _sut.Pop(UIScreenCategory.Popup));
            Assert.AreEqual(0, _sut.GetStackDepth(UIScreenCategory.Popup));
        }

        [Test]
        public void PopSpecific_RemovesMiddleEntry()
        {
            _sut.Push(typeof(int), UIScreenCategory.Screen);
            _sut.Push(typeof(string), UIScreenCategory.Screen);
            _sut.Push(typeof(float), UIScreenCategory.Screen);

            _sut.PopSpecific(typeof(string), UIScreenCategory.Screen);

            Assert.IsFalse(_sut.IsInHistory(typeof(string)));
            Assert.IsTrue(_sut.IsInHistory(typeof(int)));
            Assert.IsTrue(_sut.IsInHistory(typeof(float)));
            Assert.AreEqual(2, _sut.GetStackDepth(UIScreenCategory.Screen));
        }

        [Test]
        public void ClearCategory_And_ClearAll_EmptyStacks()
        {
            _sut.Push(typeof(int), UIScreenCategory.Screen);
            _sut.Push(typeof(string), UIScreenCategory.Modal);

            _sut.ClearCategory(UIScreenCategory.Screen);

            Assert.AreEqual(0, _sut.GetStackDepth(UIScreenCategory.Screen));
            Assert.AreEqual(1, _sut.GetStackDepth(UIScreenCategory.Modal));

            _sut.ClearAll();

            Assert.AreEqual(0, _sut.GetStackDepth(UIScreenCategory.Modal));
            Assert.IsFalse(_sut.IsInHistory(typeof(string)));
        }
    }
}
