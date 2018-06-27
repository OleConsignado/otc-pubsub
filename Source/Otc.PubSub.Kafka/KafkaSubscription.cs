using System;
using System.Threading;
using System.Threading.Tasks;
using Otc.PubSub.Abstractions;

namespace Otc.PubSub.Kafka
{
    internal class KafkaSubscription : ISubscription
    {
        private KafkaConsumerWrapper kafkaConsumerWrapper;
        private IMessageHandler messageHandler;
        private string groupId;
        private string[] topics;

        public KafkaSubscription(KafkaConsumerWrapper kafkaConsumerWrapper, IMessageHandler messageHandler, string groupId, string[] topics)
        {
            this.kafkaConsumerWrapper = kafkaConsumerWrapper ?? throw new System.ArgumentNullException(nameof(kafkaConsumerWrapper));
            this.messageHandler = messageHandler ?? throw new System.ArgumentNullException(nameof(messageHandler));
            this.groupId = groupId ?? throw new System.ArgumentNullException(nameof(groupId));
            this.topics = topics;
        }

        public void ReloadAt(DateTimeOffset time)
        {
            if(time < kafkaConsumerWrapper.ReloadAt)
            {
                kafkaConsumerWrapper.ReloadAt = time;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => kafkaConsumerWrapper.SubscribeAndStartPoll(messageHandler, topics, cancellationToken));
        }
    }
}