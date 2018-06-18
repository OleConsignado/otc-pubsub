using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Otc.PubSub.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Otc.PubSub.Kafka
{
    internal class KafkaProducerWrapper : IDisposable
    {
        private readonly KafkaPubSubConfiguration configuration;
        private readonly ILogger logger;

        public KafkaProducerWrapper(KafkaPubSubConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            logger = loggerFactory?.CreateLogger<KafkaProducerWrapper>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private Producer _kafkaProducer;

        private Producer KafkaProducer
        {
            get
            {
                if (_kafkaProducer == null)
                {
                    _kafkaProducer = new Producer(configuration.CreateKafkaProducerConfigurationDictionary());
                    KafkaProducerEventsSubscribe();
                }

                return _kafkaProducer;
            }
        }

        public async Task PublishAsync(string topic, byte[] message)
        {
            logger.LogDebug($"{nameof(PublishAsync)}: Begin produce to Kafka.");

            CheckDisposed();

            var producerResult = await KafkaProducer.ProduceAsync(topic, null, message);

            if (producerResult.Error)
            {
                throw new PublishException(producerResult.Error, topic, message);
            }

            logger.LogDebug($"{nameof(PublishAsync)}: Produced to Kafka successfully!");
        }

        #region [ Confluent.Kafka.Producer events ]

        private void KafkaProducerEventsSubscribe()
        {
            _kafkaProducer.OnError += _kafkaProducer_OnError;
            _kafkaProducer.OnLog += _kafkaProducer_OnLog;
            _kafkaProducer.OnStatistics += _kafkaProducer_OnStatistics;
        }

        private void KafkaProducerEventsUnsubscribe()
        {
            _kafkaProducer.OnError -= _kafkaProducer_OnError;
            _kafkaProducer.OnLog -= _kafkaProducer_OnLog;
            _kafkaProducer.OnStatistics -= _kafkaProducer_OnStatistics;
        }

        private void _kafkaProducer_OnStatistics(object sender, string statitics)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Confluent.Kafka.Producer statistics: {statitics}");
            }
        }

        private void _kafkaProducer_OnLog(object sender, LogMessage e)
        {
            KafkaLogHelper.LogKafkaMessage(logger, e);
        }

        private void _kafkaProducer_OnError(object sender, Error e)
        {
            logger.LogWarning("Confluent.Kafka.Producer error: {@Error}", e);
        }

        #endregion

        private bool disposed = false;

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(KafkaPubSub));
            }
        }

        public void Dispose()
        {
            if (_kafkaProducer != null)
            {
                logger.LogDebug($"{nameof(Dispose)}: Disposing ...");

                KafkaProducerEventsUnsubscribe();
                _kafkaProducer.Dispose();
                _kafkaProducer = null;

                logger.LogDebug($"{nameof(Dispose)}: Disposed.");
            }

            disposed = true;
        }
    }
}
