namespace ConsumerC.Persistence.Entities;

public class Flight
{
    public required Guid Id { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
}
