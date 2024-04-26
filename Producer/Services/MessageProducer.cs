using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Producer.Services;

public interface IMessageProducer
{
    public void SendMessage<T>(T message, string routingKey);
}

public class MessageProducer : IMessageProducer
{
    public void SendMessage<T>(T message, string routingKey)
    {
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
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        channel.BasicPublish("dotnet.rabbitmq.demo", routingKey, body: body);
    }
}
