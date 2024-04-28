using ConsumerC.Extensions;
using ConsumerC.Features;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;

namespace ConsumerC.BackgroundServices;

public class RabbitMqConsumer : BackgroundService
{
    private const string Exchange = "dotnet.rabbitmq.demo";
    private const string Queue = "dotnet.rabbitmq.demo.consumer.c";

    private readonly IConnection _connection;
    private readonly IModel _channel;

    private readonly IServiceProvider _serviceProvider;
    private ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            DispatchConsumersAsync = true,
        };

        _connection = factory.CreateConnection();

        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _channel.QueueDeclare(
            queue: Queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: Queue,
            exchange: Exchange,
            routingKey: "*.*");

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                using var scope = _serviceProvider.CreateScope();
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
                    _logger.LogInformation($"Message handling not found for {args.RoutingKey}: {message}");
                }
                
                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error processing message: {ex.Message}");
                _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(Queue, false, consumer);

        return Task.CompletedTask;
    }
}
