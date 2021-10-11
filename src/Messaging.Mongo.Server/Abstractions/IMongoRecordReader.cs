using Assistant.Net.Messaging.Models;
using System.Collections.Generic;
using System.Threading;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     
    /// </summary>
    public interface IMongoRecordReader
    {
        /// <summary>
        ///     
        /// </summary>
        IAsyncEnumerable<MongoRecord> FindRequested(CancellationToken token);
    }
}
