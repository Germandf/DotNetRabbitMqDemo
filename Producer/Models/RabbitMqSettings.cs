﻿namespace Producer.Models;

public class RabbitMqSettings
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Exchange { get; set; }
}
