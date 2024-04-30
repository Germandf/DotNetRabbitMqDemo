using ConsumerD.Publishers;
using ConsumerD.Settings;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));
builder.Services.AddMassTransit(options =>
{
    options.SetKebabCaseEndpointNameFormatter();
    options.AddConsumers(typeof(Program).Assembly);
    options.UsingRabbitMq((context, cfg) =>
    {
        var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
        cfg.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, x =>
        {
            x.Username(settings.Username);
            x.Password(settings.Password);
            x.ConnectionName(settings.ClientProvidedName);
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddHostedService<MessagePublisher>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
