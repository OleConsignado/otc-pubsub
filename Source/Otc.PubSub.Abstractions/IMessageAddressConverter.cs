using System.Collections.Generic;

namespace Otc.PubSub.Abstractions
{
    public interface IMessageAddressConverter
    {
        IMessageAddress ToMessageAddress(IDictionary<string, object> messageAddressDictionary);

        IDictionary<string, object> ToDictionary(IMessageAddress messageAddress);
    }
}
