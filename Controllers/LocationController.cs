using Microsoft.AspNetCore.Mvc;
using TheMonolith.Data;
using TheMonolith.Database.Repositories;
using TheMonolith.Models;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationController : ControllerBase
{
    public const string GeolocationScoreType = "LOCATION";

    private readonly ScoreRepository _scoreRepository;
    
    public static Location RegnecentralenLocation = new Location
    {
        Latitude = 56.1724716,
        Longitude = 10.1877707,
        Accuracy = 0
    };

    public const int RegnecentralenDist = 100;

    public LocationController(ScoreRepository scoreRepository)
    {
        _scoreRepository = scoreRepository;
    }

    private static double CalcScore(double distance, double radius, double boundary)
    {
        return -Math.Tanh((distance - radius) / boundary);
    }

    [HttpPost]
    public async Task Post([FromBody] int userId, [FromBody] Location location)
    {
        // RC distance
        var distance = location.DistanceTo(RegnecentralenLocation);
        var value = CalcScore(distance, 100, 20);
        var user = new User();
        var score = new Score(user, GeolocationScoreType + "_RC", value);
        await _scoreRepository.Create(score);
    }
}