using MassTransit;
using Shared;

namespace ConsumerD.Publishers;

public class MessagePublisher(IBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.Publish(new CityCreated
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
            }, stoppingToken);
            await bus.Publish(new CustomerCreated
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
            }, stoppingToken);
            await bus.Publish(new FlightCreated
            {
                Id = Guid.NewGuid(),
                From = Guid.NewGuid().ToString(),
                To = Guid.NewGuid().ToString(),
            }, stoppingToken);
            await Task.Delay(1000);
        }
    }
}
