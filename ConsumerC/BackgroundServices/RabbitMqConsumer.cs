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
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
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
                    body: body);

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
