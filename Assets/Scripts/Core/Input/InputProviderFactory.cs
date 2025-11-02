namespace Core.Input
{
    /// <summary>
    /// Factory for creating platform-appropriate input providers
    /// </summary>
    public static class InputProviderFactory
    {
        public static IInputProvider CreateForPlatform()
        {
            IInputProvider provider;

#if UNITY_EDITOR || UNITY_STANDALONE
            provider = new InputSystemProvider();
            UnityEngine.Debug.Log("[InputProviderFactory] Created InputSystemProvider for Editor/Standalone");
#elif UNITY_IOS || UNITY_ANDROID
            provider = new TouchInputProvider();
            UnityEngine.Debug.Log("[InputProviderFactory] Created TouchInputProvider for Mobile");
#else
            provider = new InputSystemProvider();
            UnityEngine.Debug.Log("[InputProviderFactory] Created InputSystemProvider (fallback)");
#endif

            // Initialize the provider
            provider.Initialize();

            return provider;
        }
    }
}
