using System.Collections.Generic;

namespace Otc.PubSub.Kafka
{
    internal static class KafkaPubSubConfigurationExtensions
    {
        public static IDictionary<string, object> CreateKafkaProducerConfigurationDictionary(this KafkaPubSubConfiguration configuration)
        {
            return new Dictionary<string, object>
            {
                { "bootstrap.servers", configuration.BrokerList }
            };
        }

        public static IDictionary<string, object> CreateKafkaConsumerConfigurationDictionary(this KafkaPubSubConfiguration configuration, string groupId)
        {
            return new Dictionary<string, object>
            {
                { "group.id", groupId },
                { "enable.auto.commit", false },
                { "bootstrap.servers", configuration.BrokerList },
                { "auto.offset.reset", "earliest" }
            };
        }
    }
}
