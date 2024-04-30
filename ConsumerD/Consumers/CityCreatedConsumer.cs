using MassTransit;
using Shared;
using System.Text;

namespace ConsumerD.Consumers;

public class CityCreatedConsumer(ILogger<CityCreatedConsumer> logger) : IConsumer<CityCreated>
{
    public Task Consume(ConsumeContext<CityCreated> context)
    {
        var body = context.ReceiveContext.GetBody();
        var message = Encoding.UTF8.GetString(body);
        logger.LogInformation($"{nameof(CityCreated)}: {message}");
        return Task.CompletedTask;
    }
}
