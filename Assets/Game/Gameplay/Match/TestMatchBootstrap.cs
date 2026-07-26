using Playcenter;
using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dev bootstrap: starts a match on scene load. Removed when real flow lands in Slice 2.
    /// </summary>
    public sealed class TestMatchBootstrap : MonoBehaviour
    {
        private void Start()
        {
            ServiceLocator.Get<MatchController>().StartMatch(seed: 42);
        }
    }
}
