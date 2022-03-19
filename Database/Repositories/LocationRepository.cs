using Npgsql;
using TheMonolith.Data;
using TheMonolith.Models;

namespace TheMonolith.Database.Repositories;

public class LocationRepository
{
    private readonly Postgres _database;

    public LocationRepository(Postgres database) => _database = database;
    
    public async Task UpdateLocation(User user, Location location)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand(
            @"INSERT INTO locations (""userId"", location) VALUES (@userId, point(@longitude, @latitude))
                        ON CONFLICT (""userId"") DO UPDATE SET location = point(@longitude, @latitude)", connection);
        command.Parameters.AddWithValue("@userId", user.Id);
        command.Parameters.AddWithValue("@longitude", location.Longitude);
        command.Parameters.AddWithValue("@latitude", location.Latitude);
        var rows = await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<User>> UsersInRange(Location location, double radius)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand(
            "SELECT \"userId\", location <@> point(@longitude, @latitude) as distance FROM locations", connection);
        command.Parameters.AddWithValue("@longitude", location.Longitude);
        command.Parameters.AddWithValue("@latitude", location.Latitude);

        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var userId = reader.GetInt32(0);
            var distance = reader.GetFloat(1);
            Console.WriteLine("UserId: " + userId + " Distance: " + distance);
        }

        return Array.Empty<User>();
    }
}