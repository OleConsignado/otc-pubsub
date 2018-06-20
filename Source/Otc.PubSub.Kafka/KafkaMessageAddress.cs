using Confluent.Kafka;
using Otc.PubSub.Abstractions;
using System;
using System.Collections.Generic;

namespace Otc.PubSub.Kafka
{
    public class KafkaMessageAddress : IMessageAddress
    {
        public KafkaMessageAddress(TopicPartitionOffset topicPartitionOffset)
            : this(topicPartitionOffset?.Topic, topicPartitionOffset.Partition, topicPartitionOffset.Offset)
        {

        }

        public KafkaMessageAddress(string topic, int partition, long offset)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Partition = partition;
            Offset = offset;
        }

        public KafkaMessageAddress(IDictionary<string, object> messageAddressDictionary)
        {
            if (messageAddressDictionary == null)
            {
                throw new ArgumentNullException(nameof(messageAddressDictionary));
            }

            if (!messageAddressDictionary.ContainsKey("Topic"))
            {
                throw new ArgumentException("Expected key Topic in dictionary", nameof(messageAddressDictionary));
            }

            if (!messageAddressDictionary.ContainsKey("Partition"))
            {
                throw new ArgumentException("Expected key Partition in dictionary", nameof(messageAddressDictionary));
            }

            if (!messageAddressDictionary.ContainsKey("Offset"))
            {
                throw new ArgumentException("Expected key Offset in dictionary", nameof(messageAddressDictionary));
            }

            try
            {
                Topic = Convert.ToString(messageAddressDictionary["Topic"]);
                Partition = Convert.ToInt32(messageAddressDictionary["Partition"]);
                Offset = Convert.ToInt64(messageAddressDictionary["Offset"]);
            }
            catch(Exception e) when (e is FormatException || e is OverflowException)
            {
                throw new ArgumentException("Could not convert to appropriate type, see innerException.", nameof(messageAddressDictionary), e);
            }
        }

        public string Topic { get; }
        public int Partition { get; }
        public long Offset { get; }

        internal TopicPartitionOffset GetTopicPartitionOffset()
        {
            return new TopicPartitionOffset(Topic, Partition, Offset);
        }
    }
}
