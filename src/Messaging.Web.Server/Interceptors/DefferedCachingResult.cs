using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Deferred execution result for caching mechanism.
    /// </summary>
    public class DeferredCachingResult<T>
    {
        private readonly Task<T> task;

        /// <summary>
        ///     Ctor for deferred value.
        /// </summary>
        public DeferredCachingResult(Task<T> task) => this.task = task;

        /// <summary>
        ///     Gets deferred value.
        /// </summary>
        public Task<T> GetTask() => task;

        /// <summary/>
        public static implicit operator DeferredCachingResult<T>(Task<T> task) => new(task);
    }
}
