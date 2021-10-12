using Assistant.Net.Messaging.Models;
using Assistant.Net.Unions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Requested message processing abstraction.
    /// </summary>
    public interface IMongoRecordProcessor
    {
        /// <summary>
        ///     Processes requested message wrapped in <paramref name="record"/>.
        /// </summary>
        Task<Option<MongoRecord>> Process(MongoRecord record, CancellationToken token);
    }
}
