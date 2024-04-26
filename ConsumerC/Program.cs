using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Welcome to ConsumerC!");
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
channel.QueueDeclare("dotnet.rabbitmq.demo.consumer.c", durable: true, exclusive: false, autoDelete: false);
channel.QueueBind("dotnet.rabbitmq.demo.consumer.c", "dotnet.rabbitmq.demo", "*.*");
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
channel.BasicConsume("dotnet.rabbitmq.demo.consumer.c", false, consumer);
Console.ReadKey();
