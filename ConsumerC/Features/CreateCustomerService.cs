using ConsumerC.Models;
using Shared;

namespace ConsumerC.Features;

public interface ICreateCustomerService
{
    Task Handle(CustomerCreated customerCreated);
}

public class CreateCustomerService : ICreateCustomerService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateCustomerService> _logger;

    public CreateCustomerService(ApplicationDbContext dbContext, ILogger<CreateCustomerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(CustomerCreated customerCreated)
    {
        var customer = new Customer
        {
            Id = customerCreated.Id,
            Name = customerCreated.Name,
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"{nameof(CustomerCreated)}: {customerCreated}");
    }
}
