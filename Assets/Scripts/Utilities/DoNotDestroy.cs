using UnityEngine;

namespace Utilities
{
    public class DoNotDestroy: MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}