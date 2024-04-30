using MassTransit;
using Shared;
using System.Text;

namespace ConsumerD.Consumers;

public class FlightCreatedConsumer(ILogger<FlightCreatedConsumer> logger) : IConsumer<FlightCreated>
{
    public Task Consume(ConsumeContext<FlightCreated> context)
    {
        var body = context.ReceiveContext.GetBody();
        var message = Encoding.UTF8.GetString(body);
        logger.LogInformation($"{nameof(FlightCreated)}: {message}");
        return Task.CompletedTask;
    }
}
