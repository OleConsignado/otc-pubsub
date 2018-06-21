using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Otc.PubSub.Abstractions
{
    public interface IMessage
    {
        byte[] MessageBytes { get; }
        string Topic { get; }
        DateTimeOffset Timestamp { get; }
        IDictionary<string, object> MessageAddress { get; }
        
        /// <exception cref="Exceptions.CommitException" />
        Task CommitAsync();
    }
}