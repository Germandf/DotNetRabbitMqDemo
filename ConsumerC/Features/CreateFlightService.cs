using ConsumerC.Models;
using Shared;

namespace ConsumerC.Features;

public interface ICreateFlightService
{
    Task Handle(FlightCreated flightCreated);
}

public class CreateFlightService : ICreateFlightService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateFlightService> _logger;

    public CreateFlightService(ApplicationDbContext dbContext, ILogger<CreateFlightService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(FlightCreated flightCreated)
    {
        var flight = new Flight
        {
            Id = flightCreated.Id,
            CustomerId = flightCreated.CustomerId,
            From = flightCreated.From,
            To = flightCreated.To,
        };
        _dbContext.Flights.Add(flight);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"{nameof(FlightCreated)}: {flightCreated}");
    }
}
