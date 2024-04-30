using MassTransit;
using Shared;
using System.Text;

namespace ConsumerD.Consumers;

public class CustomerCreatedConsumer(ILogger<CustomerCreatedConsumer> logger) : IConsumer<CustomerCreated>
{
    public Task Consume(ConsumeContext<CustomerCreated> context)
    {
        var body = context.ReceiveContext.GetBody();
        var message = Encoding.UTF8.GetString(body);
        logger.LogInformation($"{nameof(CustomerCreated)}: {message}");
        return Task.CompletedTask;
    }
}
