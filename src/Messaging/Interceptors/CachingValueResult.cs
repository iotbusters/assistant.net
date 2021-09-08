namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Value result for caching mechanism.
    /// </summary>
    public sealed class CachingValueResult<T> : CachingResult
    {
        /// <summary/>
        public CachingValueResult(T value) =>
            Value = value;

        /// <summary>
        ///     Successful result value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     Restores result value.
        /// </summary>
        public override object GetValue() => Value!;
    }
}