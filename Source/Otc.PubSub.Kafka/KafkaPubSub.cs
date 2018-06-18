using Microsoft.Extensions.Logging;
using Otc.PubSub.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Otc.PubSub.Kafka
{
    public class KafkaPubSub : IPubSub
    {
        private readonly KafkaProducerWrapper producer;
        private readonly ICollection<KafkaConsumerWrapper> consumers = new List<KafkaConsumerWrapper>();
        private readonly KafkaPubSubConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;

        public KafkaPubSub(KafkaPubSubConfiguration configuration, ILoggerFactory loggerFactory)
        {
            producer = new KafkaProducerWrapper(configuration, loggerFactory);
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task PublishAsync(string topic, byte[] message)
        {
            await producer.PublishAsync(topic, message);
        }

        public Task SubscribeAsync(IMessageHandler messageHandler, string group, CancellationToken cancellationToken, params string[] topics)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (topics == null)
            {
                throw new ArgumentNullException(nameof(topics));
            }

            if (topics.Length < 1)
            {
                throw new ArgumentException("At least one topic is necessary to subscribe.", nameof(topics));
            }

            var kafkaConsumerWrapper = new KafkaConsumerWrapper(configuration, loggerFactory, group);
            consumers.Add(kafkaConsumerWrapper);

            return Task.Run(() => kafkaConsumerWrapper.SubscribeAndStartPoll(messageHandler, topics, cancellationToken));
        }

        public void Dispose()
        {
            foreach(var consumer in consumers)
            {
                consumer.Dispose();
            }

            producer.Dispose();
            readFromParticularCoordinatesConsumer?.Dispose();
        }

        private KafkaConsumerWrapper readFromParticularCoordinatesConsumer = null;

        public IMessage ReadFromParticularCoordinates(IMessageCoordinates messageCoordinates)
        {
            if(!(messageCoordinates is KafkaMessageCoordinates))
            {
                throw new InvalidOperationException($"Argument {nameof(messageCoordinates)} must be of type '{typeof(KafkaMessageCoordinates).FullName}'.");
            }

            if(readFromParticularCoordinatesConsumer == null)
            {
                readFromParticularCoordinatesConsumer = new KafkaConsumerWrapper(configuration, loggerFactory, $"PCG{Environment.CommandLine}{Environment.MachineName}");
            }

            return readFromParticularCoordinatesConsumer.ReadFromParticularCoordinates(messageCoordinates as KafkaMessageCoordinates);
        }
    }
}
