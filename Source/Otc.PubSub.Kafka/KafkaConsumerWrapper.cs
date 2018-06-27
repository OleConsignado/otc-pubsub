using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Otc.PubSub.Abstractions;
using Otc.PubSub.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Otc.PubSub.Kafka
{
    internal class KafkaConsumerWrapper : IDisposable
    {
        private readonly KafkaPubSubConfiguration configuration;
        private readonly string group;
        private readonly ILogger logger;
        private readonly Consumer _kafkaConsumer;

        public KafkaConsumerWrapper(KafkaPubSubConfiguration configuration, ILoggerFactory loggerFactory, string group)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.group = group ?? throw new ArgumentNullException(nameof(group));
            logger = loggerFactory?.CreateLogger<KafkaConsumerWrapper>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            _kafkaConsumer = new Consumer(configuration.CreateKafkaConsumerConfigurationDictionary(group));
            KafkaConsumerEventsSubscribe();   
        }

        private IMessageHandler messageHandler = null;

        private DateTimeOffset _reloadAt = DateTimeOffset.MaxValue;

        public DateTimeOffset ReloadAt
        {
            get => _reloadAt;
            set
            {
                _reloadAt = value;
                logger.LogDebug($"{nameof(ReloadAt)}_set: Setted ReloadAt to '{{ReloadAt}}'", _reloadAt);
            }
        }

        public void SubscribeAndStartPoll(IMessageHandler messageHandler, string[] topics, CancellationToken cancellationToken)
        {
            CheckDisposed();

            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            if (topics == null)
            {
                throw new ArgumentNullException(nameof(topics));
            }

            logger.LogDebug($"{nameof(SubscribeAndStartPoll)}: Subscribling to topics: {{@Topics}}", topics);

            this.messageHandler = messageHandler;

            _kafkaConsumer.OnMessage += _kafkaConsumer_OnMessage;
            _kafkaConsumer.OnConsumeError += _kafkaConsumer_OnConsumeError;

            _kafkaConsumer.Subscribe(topics);

            logger.LogDebug($"{nameof(SubscribeAndStartPoll)}: Starting to poll messages ...");

            while (!cancellationToken.IsCancellationRequested)
            {
                _kafkaConsumer.Poll(100);

                if(ReloadAt <= DateTimeOffset.Now)
                {
                    logger.LogInformation($"{nameof(SubscribeAndStartPoll)}: Performing Unsubscribe/Subscribe due to " +
                        $"ReloadAt <= DateTimeOffset.Now. ReloadAt: {{ReloadAt}}; DateTimeOffset.Now: {{DateTimeOffset.Now}}",
                        ReloadAt, DateTimeOffset.Now);

                    _kafkaConsumer.Unsubscribe();
                    _kafkaConsumer.Subscribe(topics);
                }
            }

            logger.LogDebug($"{nameof(SubscribeAndStartPoll)}: OperationCancelled, disposing and throwing an OperationCanceledException.");

            Dispose();

            throw new OperationCanceledException(cancellationToken);
        }

        private void _kafkaConsumer_OnMessage(object sender, Message kafkaMessage)
        {
            logger.LogDebug($"{nameof(_kafkaConsumer_OnMessage)}: Reading message ...");

            var pubSubMessage = new PubSubMessage(_kafkaConsumer, kafkaMessage, logger);
            messageHandler.OnMessageAsync(pubSubMessage)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            logger.LogDebug($"{nameof(_kafkaConsumer_OnMessage)}: Message read successfully.");
        }

        private void _kafkaConsumer_OnConsumeError(object sender, Message kafkaMessage)
        {
            logger.LogWarning($"{nameof(_kafkaConsumer_OnConsumeError)}: @{{Error}}", kafkaMessage.Error);

            var pubSubMessage = new PubSubMessage(_kafkaConsumer, kafkaMessage, logger);
            messageHandler.OnErrorAsync(kafkaMessage.Error, pubSubMessage)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public IMessage ReadFromParticularAddress(IDictionary<string, object> messageAddress)
        {
            Message kafkaMessage = null;
            _kafkaConsumer.Assign(MessageAddressConverter.ToTopicPartitionOffset(messageAddress));

            int i = 0;

            while (i < 50 && kafkaMessage == null)
            {
                _kafkaConsumer.Consume(out kafkaMessage, 100);
                i++;
            }

            if(kafkaMessage == null)
            {
                throw new ReadException($"Read null message (tried {i} times) :/");
            }

            if(kafkaMessage.Error)
            {
                throw new ReadException(kafkaMessage.Error);
            }

            string providedTopic = Convert.ToString(messageAddress["Topic"]);
            int providedPartition = Convert.ToInt32(messageAddress["Partition"]);
            long providedOffset = Convert.ToInt64(messageAddress["Offset"]);

            if (kafkaMessage.Topic != providedTopic || kafkaMessage.Partition != providedPartition ||
                kafkaMessage.Offset.Value != providedOffset)
            {
                throw new ReadException($"Read message from wrong Topic/Partition/Offset. " +
                    $"Expected {providedTopic}/{providedPartition}/{providedOffset}, " +
                    $"read {kafkaMessage.Topic}/{kafkaMessage.Partition}/{kafkaMessage.Offset}");
            }

            return new PubSubMessage(_kafkaConsumer, kafkaMessage, logger);
            
        }

        #region [ Confluent.Kafka.Consumer informational events ]

        private void KafkaConsumerEventsSubscribe()
        {
            _kafkaConsumer.OnError += _kafkaConsumer_OnError;
            _kafkaConsumer.OnLog += _kafkaConsumer_OnLog;
            _kafkaConsumer.OnPartitionEOF += _kafkaConsumer_OnPartitionEOF;
            _kafkaConsumer.OnPartitionsAssigned += _kafkaConsumer_OnPartitionsAssigned;
            _kafkaConsumer.OnPartitionsRevoked += _kafkaConsumer_OnPartitionsRevoked;
            _kafkaConsumer.OnStatistics += _kafkaConsumer_OnStatistics;
        }

        private void KafkaConsumerEventsUnsubscribe()
        {
            if (messageHandler != null)
            {
                _kafkaConsumer.OnMessage -= _kafkaConsumer_OnMessage;
                _kafkaConsumer.OnConsumeError -= _kafkaConsumer_OnConsumeError;
            }

            _kafkaConsumer.OnError -= _kafkaConsumer_OnError;
            _kafkaConsumer.OnLog -= _kafkaConsumer_OnLog;
            _kafkaConsumer.OnPartitionEOF -= _kafkaConsumer_OnPartitionEOF;
            _kafkaConsumer.OnPartitionsAssigned -= _kafkaConsumer_OnPartitionsAssigned;
            _kafkaConsumer.OnPartitionsRevoked -= _kafkaConsumer_OnPartitionsRevoked;
            _kafkaConsumer.OnStatistics -= _kafkaConsumer_OnStatistics;
        }

        private void _kafkaConsumer_OnStatistics(object sender, string statitics)
        {
            logger.LogDebug($"Confluent.Kafka.Consumer statistics: {statitics}");
        }

        private void _kafkaConsumer_OnPartitionsRevoked(object sender, List<TopicPartition> partitions)
        {
            _kafkaConsumer.Unassign();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Confluent.Kafka.Consumer partitions revoked: {TopicPartitionList}",
                    partitions.Select(tp => new { tp.Topic, tp.Partition })
                    .ToArray());
            }
        }

        private void _kafkaConsumer_OnPartitionsAssigned(object sender, List<TopicPartition> partitions)
        {
            _kafkaConsumer.Assign(partitions);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Confluent.Kafka.Consumer partitions assigned: {TopicPartitionList}",
                    partitions.Select(tp => new { tp.Topic, tp.Partition })
                    .ToArray());
            }
        }

        private void _kafkaConsumer_OnPartitionEOF(object sender, TopicPartitionOffset topicPartitionOffset)
        {
            logger.LogInformation("Confluent.Kafka.Consumer partition EOF (no more messages). " +
                "TopicPartitionOffset: {TopicPartitionOffset}",
                new { topicPartitionOffset.Topic, topicPartitionOffset.Partition, topicPartitionOffset.Offset });
        }

        private void _kafkaConsumer_OnLog(object sender, LogMessage logMessage)
        {
            KafkaLogHelper.LogKafkaMessage(logger, logMessage);
        }

        private void _kafkaConsumer_OnError(object sender, Error error)
        {
            logger.LogWarning("Confluent.Kafka.Consumer error: {@Error}", error);
        }

        #endregion

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(KafkaPubSub));
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                logger.LogDebug($"{nameof(Dispose)}: Disposing ...");

                KafkaConsumerEventsUnsubscribe();
                _kafkaConsumer.Dispose();

                logger.LogDebug($"{nameof(Dispose)}: Disposed.");
            }
        }
    }
}