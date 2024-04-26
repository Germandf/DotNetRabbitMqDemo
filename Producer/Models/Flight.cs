namespace Producer.Models;

public class Flight
{
    public required int Id { get; set; }
    public required int CustomerId { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
}
