using System.Threading.Tasks;

namespace Assistant.Net.Messaging
{
    public class DeferredCachingResult
    {
        private readonly Task<object> task;

        public DeferredCachingResult(Task<object> task) => this.task = task;

        public Task<object> Get() => task;

        public static implicit operator DeferredCachingResult(Task<object> task) => new(task);
    }
}