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
channel.QueueDeclare("flights", durable: true, exclusive: false, autoDelete: false);
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

        Console.WriteLine($"A message has been received: {message}");
        channel.BasicAck(args.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing message: {ex.Message}");
        channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
    }
};
channel.BasicConsume("flights", false, consumer);
Console.ReadKey();
