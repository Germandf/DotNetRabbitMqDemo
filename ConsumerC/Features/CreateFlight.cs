using ConsumerC.Models;
using MediatR;
using Shared;

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
                CustomerId = request.FlightCreated.CustomerId,
                From = request.FlightCreated.From,
                To = request.FlightCreated.To,
            };
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"{nameof(FlightCreated)}: {request.FlightCreated}");
            return Unit.Value;
        }
    }
}
