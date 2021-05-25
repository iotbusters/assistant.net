using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Caching
{
    public class DeferredResult
    {
        private Task? task;
        private Result result;

        public DeferredResult(Task task)
        {
            result = new Result(new CommandDeferredException());
            this.task = task.ContinueWith(t =>
            {
                this.task = null;
                if (t.IsFaulted)
                    result = new Result(t.Exception!);
                else
                    result = new Result(((dynamic)t).Result!);
            });
        }

        public object Get() => result.Get();
    }
}