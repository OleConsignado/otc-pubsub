using Confluent.Kafka;
using Otc.PubSub.Abstractions;
using System.Text;

namespace Otc.PubSub.Kafka
{
    public class KafkaMessageCoordinates : IMessageCoordinates
    {
        public TopicPartitionOffset TopicPartitionOffset { get; }

        public KafkaMessageCoordinates(TopicPartitionOffset topicPartitionOffset)
        {
            TopicPartitionOffset = topicPartitionOffset ?? throw new System.ArgumentNullException(nameof(topicPartitionOffset));
        }

        public KafkaMessageCoordinates(string topic, int partition, long offset)
            : this(new TopicPartition(topic, partition), new Offset(offset))
        {

        }

        public KafkaMessageCoordinates(TopicPartition topicPartition, Offset offset)
            : this(new TopicPartitionOffset(topicPartition, offset))
        {

        }

        public byte[] Serialize()
        {
            return Encoding.UTF8.GetBytes(
                $"{TopicPartitionOffset.Topic}|{TopicPartitionOffset.Partition}|{TopicPartitionOffset.Offset}");
        }
    }
}
