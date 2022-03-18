using Microsoft.AspNetCore.Mvc;
using TheMonolith.Models;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class GeolocationController : ControllerBase
{
    public static Location RegnecentralenLocation = new Location
    {
        Latitude = 56.1724716m,
        Longitude = 10.1877707m,
        Accuracy = 0
    };
    
    [HttpPost]
    public TestResponse Post([FromBody] Location location)
    {
        return new TestResponse
        {
            Distance = location.DistanceTo(RegnecentralenLocation)
        };
    }
}

public class TestResponse
{
    public decimal Distance { get; set; }
    public decimal Score { get; set; }
}