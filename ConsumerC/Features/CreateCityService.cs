using ConsumerC.Models;
using Shared;

namespace ConsumerC.Features;

public interface ICreateCityService
{
    Task Handle(CityCreated cityCreated);
}

public class CreateCityService : ICreateCityService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateCityService> _logger;

    public CreateCityService(ApplicationDbContext dbContext, ILogger<CreateCityService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(CityCreated cityCreated)
    {
        var city = new City
        {
            Id = cityCreated.Id,
            Name = cityCreated.Name,
        };
        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"{nameof(CityCreated)}: {cityCreated}");
    }
}
