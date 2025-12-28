using NUnit.Framework;
using UI;
using UnityEngine.UIElements;
using Core.SaveSystem;
using System.Collections.Generic;

namespace Tests.Editor.UI
{
    public class HUDEditorScreenTests
    {
        [Test]
        public void ControlLayoutData_SerializesCorrectly()
        {
            var data = new ControlLayoutData
            {
                ControlId = "test",
                NormalizedPosition = new UnityEngine.Vector2(0.5f, 0.5f),
                SizeMultiplier = 1.2f,
                Opacity = 0.8f
            };

            string json = UnityEngine.JsonUtility.ToJson(data);
            var deserialized = UnityEngine.JsonUtility.FromJson<ControlLayoutData>(json);

            Assert.AreEqual(data.ControlId, deserialized.ControlId);
            Assert.AreEqual(data.NormalizedPosition.x, deserialized.NormalizedPosition.x);
            Assert.AreEqual(data.SizeMultiplier, deserialized.SizeMultiplier);
        }
    }
}
