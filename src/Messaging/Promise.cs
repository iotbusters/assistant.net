using System.Threading.Tasks;

namespace Assistant.Net.Messaging
{
    public class Promise
    {
        private readonly Task task;
        public Promise(Task task)
        {
            this.task = task;
        }

        public Task WaitComplete() => task;
    }

    public class Promise<T>
    {
        private readonly Task<T> task;
        public Promise(Task<T> task)
        {
            this.task = task;
        }

        public Task<T> WaitResponse() => task;
    }
}