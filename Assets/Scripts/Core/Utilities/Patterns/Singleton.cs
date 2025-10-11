namespace Core.Utilities.Patterns
{
    /// <summary>
    /// Generic singleton pattern implementation for non-MonoBehaviour classes.
    /// </summary>
    /// <typeparam name="T">Type of the singleton instance</typeparam>
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance, creating it if it doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                    return _instance;
                }
            }
        }
    }
}
