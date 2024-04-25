using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Producer.Services;

public interface IMessageProducer
{
    public void SendMessage<T>(T message);
}

public class MessageProducer : IMessageProducer
{
    public void SendMessage<T>(T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare("flights", durable: true, exclusive: false, autoDelete: false);
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        channel.BasicPublish("", "flights", body: body);
    }
}
