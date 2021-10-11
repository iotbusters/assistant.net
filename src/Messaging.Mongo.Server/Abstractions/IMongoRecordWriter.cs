using Assistant.Net.Messaging.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     
    /// </summary>
    public interface IMongoRecordWriter
    {
        /// <summary>
        ///     
        /// </summary>
        Task Update(MongoRecord record, CancellationToken token);
    }
}
