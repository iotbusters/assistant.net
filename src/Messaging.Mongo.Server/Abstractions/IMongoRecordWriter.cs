using Assistant.Net.Messaging.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Complete message updating abstraction.
    /// </summary>
    public interface IMongoRecordWriter
    {
        /// <summary>
        ///     Persists message with new status after processing.
        /// </summary>
        Task Update(MongoRecord record, CancellationToken token);
    }
}
