namespace Producer.Models;

public class CreateFlightRequest
{
    public required Guid CustomerId { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
}
