﻿using ConsumerC.Models;
using MediatR;
using Shared;

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
            logger.LogInformation($"{nameof(CustomerCreated)}: {request.CustomerCreated}");
            return Unit.Value;
        }
    }
}