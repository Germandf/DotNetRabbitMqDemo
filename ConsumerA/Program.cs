using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Welcome to ConsumerA!");
var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
};
//factory.AutomaticRecoveryEnabled = true;
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.ExchangeDeclare("dotnet.rabbitmq.demo", ExchangeType.Topic, durable: true, autoDelete: false);
channel.QueueDeclare("dotnet.rabbitmq.demo.consumer.a", durable: true, exclusive: false, autoDelete: false);
channel.QueueBind("dotnet.rabbitmq.demo.consumer.a", "dotnet.rabbitmq.demo", "flight.created");
channel.QueueBind("dotnet.rabbitmq.demo.consumer.a", "dotnet.rabbitmq.demo", "customer.created");
var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, args) =>
{
    await Task.Delay(500);
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    try
    {
        if (message.Contains("test"))
            throw new Exception("test");

        Console.WriteLine($"{args.RoutingKey}: {message}");
        channel.BasicAck(args.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing message: {ex.Message}");
        channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
    }
};
channel.BasicConsume("dotnet.rabbitmq.demo.consumer.a", false, consumer);
Console.ReadKey();
