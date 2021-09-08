namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Execution result for caching mechanism.
    /// </summary>
    public abstract class CachingResult
    {
        /// <summary>
        ///     Gets cached result.
        /// </summary>
        public abstract object GetValue();
    }
}