using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Otc.PubSub.Abstractions
{
    public interface IPubSub : IDisposable
    {
        /// <summary>
        /// Publish item
        /// </summary>
        /// <param name="topic">Where to publish</param>
        /// <param name="message">Message to be published</param>
        /// <exception cref="Exceptions.PublishException" />
        Task PublishAsync(string topic, byte[] message);

        /// <summary>
        /// Subscribe to topic(s)
        /// </summary>
        /// <param name="messageHandler" />
        /// <param name="groupId" />
        /// <param name="cancellationToken" />
        /// <param name="topics" />
        /// <exception cref="ArgumentException">If topic is a empty array.</exception>
        /// <exception cref="OperationCanceledException">When cancellationToken cancelled.</exception>
        Task SubscribeAsync(IMessageHandler messageHandler, string groupId, CancellationToken cancellationToken, params string[] topics);

        /// <exception cref="InvalidOperationException">Concrete type of messageCoordinates is not compatible with this particular PubSub implementation.</exception>
        /// <exception cref="Exceptions.ReadException" />
        IMessage ReadSingle(IDictionary<string, object> messageAddress);

    }
}
