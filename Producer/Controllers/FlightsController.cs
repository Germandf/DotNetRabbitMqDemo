using Microsoft.AspNetCore.Mvc;
using Producer.Models;
using Producer.Services;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class FlightsController(IMessageProducer messageProducer) : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] Flight flight)
    {
        messageProducer.SendMessage(flight);
        return Ok();
    }
}
