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
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));        
            producer = new KafkaProducerWrapper(configuration, loggerFactory);
        }

        public async Task PublishAsync(string topic, byte[] message)
        {
            await producer.PublishAsync(topic, message);
        }

        public ISubscription Subscribe(IMessageHandler messageHandler, string groupId, params string[] topics)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            if (groupId == null)
            {
                throw new ArgumentNullException(nameof(groupId));
            }

            if (topics == null)
            {
                throw new ArgumentNullException(nameof(topics));
            }

            if (topics.Length < 1)
            {
                throw new ArgumentException("At least one topic is necessary to subscribe.", nameof(topics));
            }

            var kafkaConsumerWrapper = new KafkaConsumerWrapper(configuration, loggerFactory, groupId);
            consumers.Add(kafkaConsumerWrapper);

            return new KafkaSubscription(kafkaConsumerWrapper, messageHandler, groupId, topics);

            //return Task.Run(() => kafkaConsumerWrapper.SubscribeAndStartPoll(messageHandler, topics, cancellationToken));
        }

        public void Dispose()
        {
            foreach(var consumer in consumers)
            {
                consumer.Dispose();
            }

            producer.Dispose();
            readFromParticularAddressConsumer?.Dispose();
        }

        private KafkaConsumerWrapper readFromParticularAddressConsumer = null;

        [ThreadStatic]
        private static int threadId = -1;

        private static object threadCounterLockPad = new object();
        private static int threadCounter = 0;

        private string ReadFromParticularAddressGroup
        { 
            get
            {
                if(threadId == -1)
                {
                    lock (threadCounterLockPad)
                    {
                        threadId = threadCounter++;
                    }
                }

                var processId = ($"{Environment.MachineName}{Environment.CommandLine}" +
                    $"{string.Join(",", Environment.GetCommandLineArgs())}").GetHashCode();

                return $"PubSubRPAG_{processId}_{threadId}";
            }
        }

        public IMessage ReadSingle(IDictionary<string, object> messageAddress)
        {
            if (messageAddress == null)
            {
                throw new ArgumentNullException(nameof(messageAddress));
            }

            if (readFromParticularAddressConsumer == null)
            {
                readFromParticularAddressConsumer = new KafkaConsumerWrapper(configuration, loggerFactory, ReadFromParticularAddressGroup);
            }

            return readFromParticularAddressConsumer.ReadFromParticularAddress(messageAddress);
        }
    }
}
