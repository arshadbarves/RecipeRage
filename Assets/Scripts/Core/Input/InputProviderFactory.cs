namespace Core.Input
{
    /// <summary>
    /// Factory for creating platform-appropriate input providers
    /// </summary>
    public static class InputProviderFactory
    {
        public static IInputProvider CreateForPlatform()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return new KeyboardInputProvider();
#elif UNITY_IOS || UNITY_ANDROID
            return new TouchInputProvider();
#else
            return new InputSystemProvider();
#endif
        }
    }
}
