using Otc.PubSub.Abstractions;
using Otc.PubSub.Kafka;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OtcKafkaPubSubServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaPubSub(this IServiceCollection services, Action<KafkaPubSubConfigurationLambda> config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<IPubSub, KafkaPubSub>();
            services.AddScoped<IMessageAddressConverter, KafkaMessageAddressConverter>();

            var configuration = new KafkaPubSubConfigurationLambda(services);
            config?.Invoke(configuration);

            return services;
        }

        public class KafkaPubSubConfigurationLambda
        {
            private readonly IServiceCollection services;

            public KafkaPubSubConfigurationLambda(IServiceCollection services)
            {
                this.services = services ?? throw new ArgumentNullException(nameof(services));
            }

            public void Configure(KafkaPubSubConfiguration kafkaPubSubConfiguration)
            {
                services.AddSingleton(kafkaPubSubConfiguration);
            }
        }
    }
}
