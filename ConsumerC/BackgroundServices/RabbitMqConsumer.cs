using ConsumerC.Extensions;
using ConsumerC.Features;
using ConsumerC.Settings;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;

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
                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                if (args.RoutingKey is "city.created")
                {
                    var message = args.Body.Deserialize<CityCreated>();
                    var handler = scope.ServiceProvider.GetRequiredService<ICreateCityService>();
                    await handler.Handle(message);
                }
                else if (args.RoutingKey is "customer.created")
                {
                    var message = args.Body.Deserialize<CustomerCreated>();
                    await mediator.Send(new CreateCustomer.Request(message));
                }
                else if (args.RoutingKey is "flight.created")
                {
                    var message = args.Body.Deserialize<FlightCreated>();
                    await mediator.Send(new CreateFlight.Request(message));
                }
                else
                {
                    logger.LogInformation($"Message handling not found for {args.RoutingKey}");
                }
                
                channel.BasicAck(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing message: {ex.Message}");

                channel.BasicNack(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);

                /*
                channel.BasicPublish(
                    exchange: rabbitMqSettings.Value.RetryExchange,
                    routingKey: args.RoutingKey,
                    basicProperties: args.BasicProperties,
                    body: args.Body);

                channel.BasicAck(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
                */
            }
        };
        channel.BasicConsume(
            queue: rabbitMqSettings.Value.Queue,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }
}
