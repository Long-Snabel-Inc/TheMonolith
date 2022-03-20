using System.Security.Cryptography.X509Certificates;
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

    [HttpPost("TestLocations")]
    public async Task UpdateTestLocations()
    {
        double NextDouble(Random randGenerator, double minValue, double maxValue)
        {
            return randGenerator.NextDouble() * (maxValue - minValue) + minValue;
        }
        
        var rand = new Random();
        for (var i = 1; i <= 10; i++)
        {
            //56.17180510868501, 10.187760079584908
            //56.1714048485343, 10.190395035386228
            var lat = NextDouble(rand, 56.1714048485343, 56.17180510868501);
            var lon = NextDouble(rand, 10.187760079584908, 10.190395035386228);
            var user = await _userRepository.Get("test" + i);
            await _locationRepository.UpdateLocation(user!, new Location
            {
                Accuracy = 0,
                Latitude = lat,
                Longitude = lon
            });
        }
    }
    
    [HttpPost("{userId:int}/Range")]
    public async Task<IActionResult> TestUserRange(int userId, [FromBody] Location location)
    {
        var user = await _userRepository.Get(userId);
        if (user is null)
        {
            return NotFound();
        }

        var userIds = _locationRepository.UsersInRange(user, location, 50);
        return Ok();
    }

    private async Task UpdateLocation(User user, Location location)
    {
        await _locationRepository.UpdateLocation(user, location);
        await UpdateUserLocation(user, location);
        await UpdateLocationScores(user, location, new[]
        {
            Regnecentralen
        });
    }

    private async Task UpdateLocationScores(User user, Location location, IEnumerable<LocationConfig> configs)
    {
        foreach (var config in configs)
        {
            var distance = location.DistanceTo(config.Location);
            var value = CalcScore(distance, config.Radius, config.Falloff);
            var score = new Score(user, Score.LocationType + config.Name, value);
            await _scoreRepository.Create(score);
        }
    }

    private async Task UpdateUserLocation(User user, Location location)
    {
        var userIds = _locationRepository.UsersInRange(user, location, 2000);
    }
}