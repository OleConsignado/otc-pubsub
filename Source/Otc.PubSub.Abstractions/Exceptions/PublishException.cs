using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.PubSub.Abstractions.Exceptions
{
    public class PublishException : PubSubException
    {
        public string Topic { get; }
        public byte[] MessageBytes { get; }

        public PublishException(object error, string topic, byte[] messageBytes)
            : base(error, "Error on publish.")
        {
            Topic = topic;
            MessageBytes = messageBytes;
        }
    }
}
