namespace Producer.Models;

public class Flight
{
    public required int Id { get; set; }
    public required string PassengerName { get; set; }
    public required string PassportNumber { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
    public required int Status { get; set; }
}
