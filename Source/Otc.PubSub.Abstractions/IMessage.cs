using System;
using System.Threading.Tasks;

namespace Otc.PubSub.Abstractions
{
    public interface IMessage
    {
        byte[] MessageBytes { get; }
        string Topic { get; }
        DateTimeOffset Timestamp { get; }
        IMessageAddress MessageAddress { get; }
        
        /// <exception cref="Exceptions.CommitException" />
        Task CommitAsync();
    }
}