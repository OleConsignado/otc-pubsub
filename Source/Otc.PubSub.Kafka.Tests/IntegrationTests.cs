using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Otc.PubSub.Abstractions;
using Otc.PubSub.Kafka;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Otc_PubSub.Kafka.Tests
{
    public class IntegrationTests
    {
        private IServiceProvider serviceProvider;

        public IntegrationTests()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddKafkaPubSub(config =>
            {
                config.Configure(new KafkaPubSubConfiguration()
                {
                    BrokerList = "192.168.145.100"
                });
            });

            services.AddLogging();

            serviceProvider = services.BuildServiceProvider();
        }

        class MessageHandler : IMessageHandler
        {
            public Task OnErrorAsync(object error, IMessage message)
            {
                throw new NotImplementedException();
            }

            public async Task OnMessageAsync(IMessage message)
            {
                await message.CommitAsync();
            }
        }

        [Fact]
        public async Task Test_Subscribe()
        {
            var cts = new CancellationTokenSource();
            var pubSub = serviceProvider.GetService<IPubSub>();

            var t = Task.Run(async () =>
            {
                await Task.Delay(10000);
                cts.Cancel();
            });

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await pubSub.SubscribeAsync(new MessageHandler(), "testxy", cts.Token, "teste");
            });

        }

        [Fact]
        public async Task Test_Publish()
        {
            var pubSub = serviceProvider.GetService<IPubSub>();
            await pubSub.PublishAsync("teste", Encoding.UTF8.GetBytes("teste"));
        }

        [Fact]
        public void Test_ReadFromParticularCoordinates()
        {
            var pubSub = serviceProvider.GetService<IPubSub>();
            var message = pubSub.ReadMessage(new KafkaMessageAddress("teste", 0, 0));
        }
    }
}
