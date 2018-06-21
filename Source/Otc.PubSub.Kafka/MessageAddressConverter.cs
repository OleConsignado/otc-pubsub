using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.PubSub.Kafka
{
    public static class MessageAddressConverter
    {
        public static IDictionary<string, object> ToDictionary(TopicPartitionOffset topicPartitionOffset)
        {
            if (topicPartitionOffset == null)
            {
                throw new ArgumentNullException(nameof(topicPartitionOffset));
            }

            return new Dictionary<string, object>()
            {
                { "Topic", topicPartitionOffset.Topic },
                { "Partition", topicPartitionOffset.Partition },
                { "Offset", topicPartitionOffset.Offset }
            };
        }

        public static Dictionary<string, object> BuildDictionary(string topic, int partition, long offset)
        {
            return new Dictionary<string, object>()
            {
                { "Topic", topic },
                { "Partition", partition },
                { "Offset", offset }
            };
        }

        public static TopicPartitionOffset ToTopicPartitionOffset(IDictionary<string, object> messageAddress)
        {
            if (messageAddress == null)
            {
                throw new ArgumentNullException(nameof(messageAddress));
            }

            if (!messageAddress.ContainsKey("Topic"))
            {
                throw new ArgumentException("Expected key Topic in dictionary", nameof(messageAddress));
            }

            if (!messageAddress.ContainsKey("Partition"))
            {
                throw new ArgumentException("Expected key Partition in dictionary", nameof(messageAddress));
            }

            if (!messageAddress.ContainsKey("Offset"))
            {
                throw new ArgumentException("Expected key Offset in dictionary", nameof(messageAddress));
            }

            try
            {
                string topic = Convert.ToString(messageAddress["Topic"]);
                int partition = Convert.ToInt32(messageAddress["Partition"]);
                long offset = Convert.ToInt64(messageAddress["Offset"]);

                return new TopicPartitionOffset(topic, partition, offset);
            }
            catch (Exception e) when (e is FormatException || e is OverflowException)
            {
                throw new ArgumentException("Could not convert to appropriate type, see innerException.", nameof(messageAddress), e);
            }
        }
    }
}
