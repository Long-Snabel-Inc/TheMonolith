using System.Security.Cryptography;
using System.Text;
using Npgsql;
using TheMonolith.Data;
using TheMonolith.Models;

namespace TheMonolith.Database.Repositories;

public class UserRepository
{
    private readonly Postgres _database;
    private readonly SHA256 _hash = SHA256.Create();

    public UserRepository(Postgres database) => _database = database;

    private async Task<string> HashPassword(string password)
    {
        // TODO: Salt? Pepper?
        var bytes = Encoding.UTF8.GetBytes(password);
        var stream = new MemoryStream(bytes);
        var digest = await _hash.ComputeHashAsync(stream);
        
        return BitConverter.ToString(digest);
    }

    public async Task<User?> Get(int id)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand("SELECT username, email, fullname, password FROM Users WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var userName = reader.GetString(0);
            var email = reader.GetString(1);
            var fullname = reader.GetString(2);
            var password = reader.GetString(3);

            return new User
            {
                Id = id,
                UserName = userName,
                Email = email,
                FullName = fullname,
                Password = password
            };
        }

        return null;
    }

    public async Task<User?> Get(string userName)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand("SELECT id, email, fullname, password FROM Users WHERE username = @username", connection);
        command.Parameters.AddWithValue("username", userName);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);
            var email = reader.GetString(1);
            var fullname = reader.GetString(2);
            var password = reader.GetString(3);

            return new User
            {
                Id = id,
                UserName = userName,
                Email = email,
                FullName = fullname,
                Password = password
            };
        }

        return null;
    }
    
    

    public async IAsyncEnumerable<User> GetAll()
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand("SELECT id, email, fullname, password, username FROM Users ORDER BY id", connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);
            var email = reader.GetString(1);
            var fullname = reader.GetString(2);
            var password = reader.GetString(3);
            var userName = reader.GetString(4);

            yield return new User
            {
                Id = id,
                UserName = userName,
                Email = email,
                FullName = fullname,
                Password = password
            };
        }
    }

    public async Task<bool> ExistsUsername(string userName)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand("SELECT COUNT(1) FROM Users WHERE username = @username LIMIT 1", connection);
        command.Parameters.AddWithValue("username", userName);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ExistsEmail(string email)
    {
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand("SELECT COUNT(1) FROM Users WHERE email = @email LIMIT 1", connection);
        command.Parameters.AddWithValue("email", email);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<User> Create(string userName, string email, string fullName, string password)
    {
        var hashedPassword = await HashPassword(password);
        
        await using var connection = await _database.Connection();
        await using var command = new NpgsqlCommand("INSERT INTO Users (username, email, fullname, password) VALUES (@username, @email, @fullname, @password) RETURNING id", connection);
        command.Parameters.AddWithValue("username", userName);
        command.Parameters.AddWithValue("email", email);
        command.Parameters.AddWithValue("fullname", fullName);
        command.Parameters.AddWithValue("password", hashedPassword);

        var id = (int)await command.ExecuteScalarAsync();
        return new User()
        {
            Id = id,
            UserName = userName,
            Email = email,
            FullName = fullName,
            Password = hashedPassword
        };
    }

    public async Task<bool> Login(User user, string password)
    {
        var hashedPassword = await HashPassword(password);
        return hashedPassword == user.Password;
    }
}