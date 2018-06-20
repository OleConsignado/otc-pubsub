using Otc.PubSub.Abstractions;
using System;
using System.Collections.Generic;

namespace Otc.PubSub.Kafka
{
    public class KafkaMessageAddressConverter : IMessageAddressConverter
    {
        public IDictionary<string, object> ToDictionary(IMessageAddress messageAddress)
        {
            if (messageAddress == null)
            {
                throw new ArgumentNullException(nameof(messageAddress));
            }

            if(!(messageAddress is KafkaMessageAddress))
            {
                throw new ArgumentException(
                    $"Expected concrete type is '{nameof(KafkaMessageAddress)}', but '{messageAddress.GetType().FullName}' was provided.", 
                    nameof(messageAddress));
            }

            var kafkaMessageAddress = messageAddress as KafkaMessageAddress;

            return new Dictionary<string, object>()
            {
                { "Topic", kafkaMessageAddress.Topic },
                { "Partition", kafkaMessageAddress.Partition },
                { "Offset", kafkaMessageAddress.Offset }
            };
        }

        public IMessageAddress ToMessageAddress(IDictionary<string, object> messageAddressDictionary)
        {
            return new KafkaMessageAddress(messageAddressDictionary);
        }
    }
}
