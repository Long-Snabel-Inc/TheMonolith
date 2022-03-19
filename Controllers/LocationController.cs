using Microsoft.AspNetCore.Mvc;
using TheMonolith.Data;
using TheMonolith.Database.Repositories;
using TheMonolith.Models;

namespace TheMonolith.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationController : ControllerBase
{
    public record LocationConfig(string Name, double Radius, double Falloff, Location Location);

    public static readonly LocationConfig Regnecentralen = new("_RC", 100, 20, new Location()
    {
        Latitude = 56.1724716,
        Longitude = 10.1877707,
        Accuracy = 0
    });
    
    private readonly ScoreRepository _scoreRepository;
    private readonly UserRepository _userRepository;
    private readonly LocationRepository _locationRepository;

    public LocationController(ScoreRepository scoreRepository, UserRepository userRepository, LocationRepository locationRepository)
    {
        _scoreRepository = scoreRepository;
        _userRepository = userRepository;
        _locationRepository = locationRepository;
    }

    private static double CalcScore(double distance, double radius, double boundary)
    {
        return -Math.Tanh((distance - radius) / boundary);
    }

    [HttpPost]
    public async Task Post([FromBody] Location location)
    {
        if (int.TryParse(User.Identity?.Name, out var id))
        {
            var user = await _userRepository.Get(id);
            if (user is not null)
                await UpdateLocation(user, location);
            else
            {
                Console.WriteLine("LocationController: User does not exists");
            }
        }
        else
        {
            Console.WriteLine("LocationController: Post but not logged in");
        }
    }

    private async Task UpdateLocation(User user, Location location)
    {
        await _locationRepository.UpdateLocation(user, location);
        
        // RC distance
        var distance = location.DistanceTo(Regnecentralen.Location);
        var value = CalcScore(distance, 100, 20);
        var score = new Score(user, Score.LocationType + Regnecentralen.Name, value);
        await _scoreRepository.Create(score);
    }
}