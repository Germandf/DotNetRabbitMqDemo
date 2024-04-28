using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ConsumerC.BackgroundServices;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(ILogger<RabbitMqConsumer> logger)
    {
        _logger = logger;

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
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, args) =>
        {
            await Task.Delay(500);
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                if (message.Contains("test"))
                    throw new Exception("test");

                _logger.LogInformation($"{args.RoutingKey}: {message}");
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
}
