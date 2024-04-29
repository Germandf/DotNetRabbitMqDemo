namespace ConsumerC.Settings;

public class RabbitMqSettings
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Exchange { get; set; }
    public required string Queue { get; set; }
    public required string RetryExchange { get; set; }
    public required string RetryQueue { get; set; }
    public required int RetryInitialTTL { get; set; }
}
