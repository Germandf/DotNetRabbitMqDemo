using Microsoft.Extensions.Options;
using Producer.Extensions;
using Producer.Models;
using Producer.Settings;
using RabbitMQ.Client;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

    var factory = new ConnectionFactory
    {
        HostName = settings.Host,
        Port = settings.Port,
        UserName = settings.Username,
        Password = settings.Password,
        DispatchConsumersAsync = true,
    };

    var connection = factory.CreateConnection(settings.ClientProvidedName);

    var channel = connection.CreateModel();

    channel.ExchangeDeclare(
        exchange: settings.Exchange,
        type: ExchangeType.Topic,
        durable: true,
        autoDelete: false);

    return channel;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapPost("/cities", (CreateCityRequest city, IOptions<RabbitMqSettings> settings, IModel channel) =>
{
    var cityCreated = new CityCreated
    {
        Id = Guid.NewGuid(),
        Name = city.Name,
    };

    channel.BasicPublish(
        exchange: settings.Value.Exchange,
        routingKey: "city.created",
        body: cityCreated.GetJsonBytes());

    return Results.Ok();
});

app.MapPost("/customers", (CreateCustomerRequest customer, IOptions<RabbitMqSettings> settings, IModel channel) =>
{
    var customerCreated = new CustomerCreated
    {
        Id = Guid.NewGuid(),
        Name = customer.Name,
    };

    channel.BasicPublish(
        exchange: settings.Value.Exchange,
        routingKey: "customer.created",
        body: customerCreated.GetJsonBytes());

    return Results.Ok();
});

app.MapPost("/flights", (CreateFlightRequest flight, IOptions<RabbitMqSettings> settings, IModel channel) =>
{
    var flightCreated = new FlightCreated
    {
        Id = Guid.NewGuid(),
        From = flight.From,
        To = flight.To,
    };

    channel.BasicPublish(
        exchange: settings.Value.Exchange,
        routingKey: "flight.created",
        body: flightCreated.GetJsonBytes());

    return Results.Ok();
});

Console.WriteLine("Welcome to Producer!");

app.Run();
