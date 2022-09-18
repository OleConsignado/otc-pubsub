using Microsoft.Extensions.DependencyInjection;
using Otc.PubSub.Abstractions;
using Otc.PubSub.Kafka;
using System;
using System.Diagnostics;
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
                    BrokerList = "192.168.145.100",
                    AutoOffsetReset = "latest"
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

            Stopwatch stopwatch = new Stopwatch();

            public async Task OnMessageAsync(IMessage message)
            {
                if (!stopwatch.IsRunning)
                    stopwatch.Start();
                Debug.WriteLine(Encoding.UTF8.GetString(message.MessageBytes));
                await message.CommitAsync();
                Debug.WriteLine($"ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}");
            }
        }

        [Fact]
        public async Task Test_Subscribe()
        {
            var cts = new CancellationTokenSource();
            var pubSub = serviceProvider.GetService<IPubSub>();

            var t = Task.Run(async () =>
            {
                await Task.Delay(50000);
                cts.Cancel();
            });

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var subscription = pubSub.Subscribe(new MessageHandler(), "tssssdsssxyx", "teste");

                await subscription.StartAsync(cts.Token);
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
            var message = pubSub.ReadSingle(MessageAddressConverter.BuildDictionary("teste", 0, 0));
        }
    }
}
