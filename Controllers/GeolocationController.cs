using Microsoft.AspNetCore.Mvc;
using TheMonolith.Data;
using TheMonolith.Models;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class GeolocationController : ControllerBase
{
    public static Location RegnecentralenLocation = new Location
    {
        Latitude = 56.1724716,
        Longitude = 10.1877707,
        Accuracy = 0
    };

    public const int RegnecentralenDist = 100;

    private static double CalcScore(double distance, double radius, double boundary)
    {
        return -Math.Tanh((distance - radius) / boundary);
    }

    [HttpPost]
    public TestResponse Post([FromBody] int userId, [FromBody] Location location)
    {
        var distance = location.DistanceTo(RegnecentralenLocation);
        
        
        return new TestResponse
        {
            Distance = distance,
            Score = CalcScore(distance, 100, 20)
        };
    }
}

public class TestResponse
{
    public double Distance { get; set; }
    public double Score { get; set; }
}