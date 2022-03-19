using Microsoft.AspNetCore.Mvc;
using TheMonolith.Database.Repositories;
using TheMonolith.Models;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class GeolocationController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public static Location RegnecentralenLocation = new Location
    {
        Latitude = 56.1724716m,
        Longitude = 10.1877707m,
        Accuracy = 0
    };

    public GeolocationController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    [HttpPost]
    public TestResponse Post([FromBody] Location location)
    {
        var user = _userRepository.Get(int.Parse(User.Identity.Name));
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