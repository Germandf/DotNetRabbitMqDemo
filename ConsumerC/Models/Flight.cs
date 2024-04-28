namespace ConsumerC.Models;

public class Flight
{
    public required Guid Id { get; set; }
    public required Guid CustomerId { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
}
