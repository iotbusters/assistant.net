using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Deferred execution result for caching mechanism.
    /// </summary>
    public class DeferredCachingResult
    {
        private readonly Task<object> task;

        /// <summary>
        ///     Ctor for deferred value.
        /// </summary>
        public DeferredCachingResult(Task<object> task) => this.task = task;

        /// <summary>
        ///     Gets deferred value.
        /// </summary>
        public Task<object> GetTask() => task;

        /// <summary/>
        public static implicit operator DeferredCachingResult(Task<object> task) => new(task);
    }
}