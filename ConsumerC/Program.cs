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
    opts.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgreSQL"),
        // Avoid exception calling EnsureDeleted with PostgreSQL provider
        // https://github.com/npgsql/efcore.pg/issues/2970
        o => o.SetPostgresVersion(12, 0)));
builder.Services.AddScoped<ICreateCityService, CreateCityService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

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
