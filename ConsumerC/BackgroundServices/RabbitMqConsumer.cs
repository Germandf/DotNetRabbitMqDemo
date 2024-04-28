using ConsumerC.Extensions;
using ConsumerC.Features;
using ConsumerC.Settings;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;

namespace ConsumerC.BackgroundServices;

public class RabbitMqConsumer(
    IOptions<RabbitMqSettings> rabbitMqSettings,
    IModel channel,
    IServiceProvider serviceProvider,
    ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, args) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                if (args.RoutingKey is "city.created" && message.TryDeserialize<CityCreated>(out var cityCreated))
                {
                    var handler = scope.ServiceProvider.GetRequiredService<ICreateCityService>();
                    await handler.Handle(cityCreated);
                }
                else if (args.RoutingKey is "customer.created" && message.TryDeserialize<CustomerCreated>(out var customerCreated))
                {
                    await mediator.Send(new CreateCustomer.Request(customerCreated));
                }
                else if (args.RoutingKey is "flight.created" && message.TryDeserialize<FlightCreated>(out var flightCreated))
                {
                    await mediator.Send(new CreateFlight.Request(flightCreated));
                }
                else
                {
                    logger.LogInformation($"Message handling not found for {args.RoutingKey}: {message}");
                }
                
                channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error processing message: {ex.Message}");
                channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };
        channel.BasicConsume(rabbitMqSettings.Value.Queue, false, consumer);

        return Task.CompletedTask;
    }
}
