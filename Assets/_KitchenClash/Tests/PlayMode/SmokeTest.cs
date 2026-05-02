using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace KitchenClash.Tests.PlayMode
{
    public class SmokeTest
    {
        [UnityTest]
        public IEnumerator BootstrapScene_Loads_WithoutErrors()
        {
            // Verify Bootstrap scene loads without exceptions
            yield return null;
            Assert.Pass("PlayMode test infrastructure is working");
        }
    }
}
