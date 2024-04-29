using ConsumerC.BackgroundServices;
using ConsumerC.Features;
using ConsumerC.Persistence;
using ConsumerC.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// PostgreSql
builder.Services.Configure<PostgreSqlSettings>(builder.Configuration.GetSection(nameof(PostgreSqlSettings)));
builder.Services.AddDbContext<ApplicationDbContext>((sp, opts) =>
{
    var settings = sp.GetRequiredService<IOptions<PostgreSqlSettings>>().Value;

    var connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = settings.Host,
        Port = int.Parse(settings.Port),
        Username = settings.Username,
        Password = settings.Password,
        Database = settings.Database,
    }.ToString();

    opts.UseNpgsql(connectionString, o => o.SetPostgresVersion(12, 0));
});

// RabbitMq
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

    var factory = new ConnectionFactory
    {
        HostName = settings.Host,
        Port = int.Parse(settings.Port),
        UserName = settings.Username,
        Password = settings.Password,
        DispatchConsumersAsync = true,
    };

    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();

    channel.ExchangeDeclare(
        exchange: settings.Exchange,
        type: ExchangeType.Topic,
        durable: true,
        autoDelete: false);

    channel.QueueDeclare(
        queue: settings.Queue,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", settings.RetryExchange }
        });

    channel.QueueBind(
        queue: settings.Queue,
        exchange: settings.Exchange,
        routingKey: "*.*");

    channel.ExchangeDeclare(
        exchange: settings.RetryExchange,
        type: ExchangeType.Topic,
        durable: true,
        autoDelete: false);

    channel.QueueDeclare(
        queue: settings.RetryQueue,
        durable: true,
        exclusive: false,
        autoDelete: false,
        new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", settings.Exchange },
            { "x-message-ttl", settings.RetryInitialTTL }
        });

    channel.QueueBind(
        queue: settings.RetryQueue,
        exchange: settings.RetryExchange,
        routingKey: "*.*");

    return channel;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICreateCityService, CreateCityService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddHostedService<RabbitMqConsumer>();

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
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

Console.WriteLine("Welcome to ConsumerC!");

app.Run();
