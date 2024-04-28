using Producer.Models;
using Producer.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMessageProducer, MessageProducer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapPost("/cities", (CreateCityRequest city, IMessageProducer messageProducer) =>
{
    var cityCreated = new CityCreated
    {
        Id = Guid.NewGuid(),
        Name = city.Name,
    };
    messageProducer.SendMessage(cityCreated, "city.created");
    return Results.Ok();
});

app.MapPost("/customers", (CreateCustomerRequest customer, IMessageProducer messageProducer) =>
{
    var customerCreated = new CustomerCreated
    {
        Id = Guid.NewGuid(),
        Name = customer.Name,
    };
    messageProducer.SendMessage(customerCreated, "customer.created");
    return Results.Ok();
});

app.MapPost("/flights", (CreateFlightRequest flight, IMessageProducer messageProducer) =>
{
    var flightCreated = new FlightCreated
    {
        Id = Guid.NewGuid(),
        CustomerId = flight.CustomerId,
        From = flight.From,
        To = flight.To,
    };
    messageProducer.SendMessage(flightCreated, "flight.created");
    return Results.Ok();
});

Console.WriteLine("Welcome to Producer!");

app.Run();
