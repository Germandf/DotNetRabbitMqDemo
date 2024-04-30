namespace Producer.Settings;

public class RabbitMqSettings
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string ClientProvidedName { get; set; }
    public required string Exchange { get; set; }
}
