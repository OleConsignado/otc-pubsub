using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.PubSub.Abstractions.Exceptions
{
    public class CommitException : PubSubException
    {
        public CommitException(object error, IMessage pubSubMessage) 
            : base(error, "Error on commit.")
        {
            PubSubMessage = pubSubMessage;
        }

        public IMessage PubSubMessage { get; }
    }
}
