using Npgsql;
using NpgsqlTypes;
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

    public async IAsyncEnumerable<int> UsersInRange(User user, Location location, double radius)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand(
            @"SELECT ""userId"" FROM locations 
                        WHERE (location <@> point(@longitude, @latitude)) < @radius
                        AND ""userId"" != @userId", connection);
        command.Parameters.AddWithValue("@longitude", location.Longitude);
        command.Parameters.AddWithValue("@latitude", location.Latitude);
        command.Parameters.AddWithValue("@radius", radius / 1609.34);
        command.Parameters.AddWithValue("@userId", user.Id);

        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            yield return reader.GetInt32(0);
        }
    }

    public async Task<Location?> GetLocation(int userId)
    {
        await using var connection = await _database.Connection();
        await using var command =
            new NpgsqlCommand("SELECT location FROM locations WHERE \"userId\" = @userId", connection);
        command.Parameters.AddWithValue("@userId", userId);

        var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var point = reader.GetFieldValue<NpgsqlPoint>(0);
            return new Location
            {
                Longitude = point.X,
                Latitude = point.Y
            };
        }
        return null;
    }

    public async Task<int?> GetClosestUserId(int userId)
    {
        var location = await GetLocation(userId);
        if (location is null) return null;
        
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand(
            @"SELECT ""userId"", location <@> point(@longitude, @latitude) as distance FROM locations 
                WHERE ""userId"" != @userId
                ORDER BY distance LIMIT 1", connection);
        command.Parameters.AddWithValue("@longitude", location.Longitude);
        command.Parameters.AddWithValue("@latitude", location.Latitude);
        command.Parameters.AddWithValue("@userId", userId);

        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return reader.GetInt32(0);
        }

        return null;
    }
}