using Producer.Models;
using Producer.Services;

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

app.MapPost("/customers", (Customer customer, IMessageProducer messageProducer) =>
{
    messageProducer.SendMessage(customer, "customer.created");
    return Results.Ok();
});

app.MapPost("/flights", (Flight flight, IMessageProducer messageProducer) =>
{
    messageProducer.SendMessage(flight, "flight.created");
    return Results.Ok();
});

app.Run();
