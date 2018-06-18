using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Otc.PubSub.Abstractions.Exceptions
{
    /// <summary>
    /// Base exception for PubSub
    /// </summary>
    public class PubSubException : Exception
    {
        public virtual object Error { get; }

        public PubSubException(object error, string message) : base(message)
        {
            Error = error;
        }
    }
}
