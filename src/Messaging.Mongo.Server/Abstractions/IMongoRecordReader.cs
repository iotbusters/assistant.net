using Assistant.Net.Messaging.Models;
using Assistant.Net.Unions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Requested message lookup abstraction.
    /// </summary>
    public interface IMongoRecordReader
    {
        /// <summary>
        ///     Finds next requested message wrapped in <see cref="MongoRecord"/>.
        /// </summary>
        Task<Option<MongoRecord>> NextRequested(CancellationToken token);
    }
}
