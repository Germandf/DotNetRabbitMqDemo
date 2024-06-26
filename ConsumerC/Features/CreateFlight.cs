﻿using ConsumerC.Persistence;
using ConsumerC.Persistence.Entities;
using MediatR;
using Shared;
using System.Text.Json;

namespace ConsumerC.Features;

public class CreateFlight
{
    public record Request(FlightCreated FlightCreated) : IRequest<Unit>;

    public class Handler(ApplicationDbContext dbContext, ILogger<Handler> logger) : IRequestHandler<Request, Unit>
    {
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var flight = new Flight
            {
                Id = request.FlightCreated.Id,
                From = request.FlightCreated.From,
                To = request.FlightCreated.To,
            };
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"{nameof(FlightCreated)}: {JsonSerializer.Serialize(request.FlightCreated)}");
            return Unit.Value;
        }
    }
}
