using ConsumerC.BackgroundServices;
using ConsumerC.Features;
using ConsumerC.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RabbitMqConsumer>();
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseInMemoryDatabase("consumerC"));
builder.Services.AddScoped<ICreateCustomerService, CreateCustomerService>();
builder.Services.AddScoped<ICreateFlightService, CreateFlightService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

Console.WriteLine("Welcome to ConsumerC!");

app.Run();
