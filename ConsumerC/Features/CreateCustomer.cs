using ConsumerC.Persistence;
using ConsumerC.Persistence.Entities;
using MediatR;
using Shared;
using System.Text.Json;

namespace ConsumerC.Features;

public class CreateCustomer
{
    public record Request(CustomerCreated CustomerCreated) : IRequest<Unit>;

    public class Handler(ApplicationDbContext dbContext, ILogger<Handler> logger) : IRequestHandler<Request, Unit>
    {
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                Id = request.CustomerCreated.Id,
                Name = request.CustomerCreated.Name,
            };
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"{nameof(CustomerCreated)}: {JsonSerializer.Serialize(request.CustomerCreated)}");
            return Unit.Value;
        }
    }
}
