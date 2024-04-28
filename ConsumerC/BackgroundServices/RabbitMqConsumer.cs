using ConsumerC.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace ConsumerC.BackgroundServices;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private ILogger<RabbitMqConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger, IServiceProvider serviceProvider)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            // https://www.rabbitmq.com/client-libraries/dotnet-api-guide#consuming-async
            DispatchConsumersAsync = true,
        };

        _connection = factory.CreateConnection();

        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            "dotnet.rabbitmq.demo",
            ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _channel.QueueDeclare(
            "dotnet.rabbitmq.demo.consumer.c",
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            "dotnet.rabbitmq.demo.consumer.c",
            "dotnet.rabbitmq.demo",
            "*.*");

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                using var scope = _serviceProvider.CreateScope();
                using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (TryDeserialize<CustomerCreated>(message, out var customerCreated))
                {
                    var customer = new Customer
                    {
                        Id = customerCreated.Id,
                        Name = customerCreated.Name,
                    };
                    dbContext.Customers.Add(customer);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"{args.RoutingKey}: Deserialized as {nameof(CustomerCreated)}: {message}");
                }
                else if (TryDeserialize<FlightCreated>(message, out var flightCreated))
                {
                    var flight = new Flight
                    {
                        Id = flightCreated.Id,
                        CustomerId = flightCreated.CustomerId,
                        From = flightCreated.From,
                        To = flightCreated.To,
                    };
                    dbContext.Flights.Add(flight);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"{args.RoutingKey}: Deserialized as {nameof(FlightCreated)}: {message}");
                }
                else
                {
                    if (message.Contains("test"))
                        throw new Exception("test");

                    _logger.LogInformation($"{args.RoutingKey}: {message}");
                }
                
                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error processing message: {ex.Message}");
                _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume("dotnet.rabbitmq.demo.consumer.c", false, consumer);

        return Task.CompletedTask;
    }

    private bool TryDeserialize<T>(string jsonString, [NotNullWhen(true)] out T? model) where T : class
    {
        try
        {
            model = JsonSerializer.Deserialize<T>(jsonString);
            return model != null;
        }
        catch (JsonException)
        {
            model = null;
            return false;
        }
    }
}
