using Assistant.Net.Messaging.Models;
using Assistant.Net.Unions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     
    /// </summary>
    public interface IMongoRecordProcessor
    {
        /// <summary>
        ///     
        /// </summary>
        Task<Option<MongoRecord>> Process(MongoRecord record, CancellationToken token);
    }
}
