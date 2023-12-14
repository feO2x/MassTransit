namespace MassTransit.Tests
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class Issue4831
    {
        [Test]
        public async Task IgnoreGenericBaseClass()
        {
            await using var serviceProvider =
                new ServiceCollection()
                   .AddLogging()
                   .AddMassTransit(x =>
                    {
                        x.AddConsumer<SomeEventConsumer>();
                        x.UsingInMemory((context, config) =>
                        {
                            config.Publish(typeof(BaseClass<>), e => e.Exclude = true);
                            config.ConfigureEndpoints(context);
                        });
                    })
                   .BuildServiceProvider();

            await serviceProvider.GetRequiredService<IBusDepot>().Start(default);

            try
            {
                await serviceProvider.GetRequiredService<IBus>().Publish(new SomeEvent { Content = "Foo" });

            }
            finally
            {
                await serviceProvider.GetRequiredService<IBusDepot>().Stop(default);
            }
        }


        sealed class SomeEventConsumer : IConsumer<SomeEvent>
        {
            public Task Consume(ConsumeContext<SomeEvent> context) => Task.CompletedTask;
        }


        abstract class BaseClass<T>
        {
            public T Content { get; set; }
        }


        sealed class SomeEvent : BaseClass<string> { }
    }
}
