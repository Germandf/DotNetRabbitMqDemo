using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Welcome to ConsumerB!");
var exchange = "producer";
var queue = "consumer.b";
var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
};
using var connection = factory.CreateConnection("DotNetRabbitMqDemo.ConsumerB");
using var channel = connection.CreateModel();
channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true, autoDelete: false);
channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(queue, exchange, "*.*");
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, args) =>
{
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"{args.RoutingKey}: {message}");
    channel.BasicAck(args.DeliveryTag, false);
};
channel.BasicConsume(queue, false, consumer);
Console.ReadKey();
