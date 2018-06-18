using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Otc.PubSub.Abstractions;
using Otc.PubSub.Abstractions.Exceptions;
using System;
using System.Threading.Tasks;

namespace Otc.PubSub.Kafka
{
    public class PubSubMessage : IMessage
    {
        private readonly Consumer kafkaConsumer;
        private readonly Message kafkaMessage;
        private readonly ILogger logger;

        public PubSubMessage(Consumer kafkaConsumer, Message kafkaMessage, ILogger logger)
        {
            this.kafkaConsumer = kafkaConsumer ?? throw new ArgumentNullException(nameof(kafkaConsumer));
            this.kafkaMessage = kafkaMessage ?? throw new ArgumentNullException(nameof(kafkaMessage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            MessageBytes = kafkaMessage.Value;
            Topic = kafkaMessage.Topic;
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(kafkaMessage.Timestamp.UnixTimestampMs);
            MessageCoordinates = new KafkaMessageCoordinates(kafkaMessage.TopicPartitionOffset);
        }

        public byte[] MessageBytes { get; }

        public string Topic { get; }

        public DateTimeOffset Timestamp { get; }

        public IMessageCoordinates MessageCoordinates { get; }

        public async Task CommitAsync()
        {
            logger.LogDebug($"{nameof(CommitAsync)}: Committing message.");

            var commitedOffset = await kafkaConsumer.CommitAsync(kafkaMessage);

            if (commitedOffset.Error)
            {
                logger.LogWarning($"{nameof(CommitAsync)}: Commit error ({{@Error}}), throwing a CommitException.", commitedOffset.Error);

                throw new CommitException(commitedOffset.Error, this);
            }

            logger.LogDebug($"{nameof(CommitAsync)}: Message committed successfully");
        }
    }
}
