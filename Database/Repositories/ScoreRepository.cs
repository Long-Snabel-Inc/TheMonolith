using Npgsql;
using TheMonolith.Data;

namespace TheMonolith.Database.Repositories
{
    public class ScoreRepository
    {
        private readonly Postgres _database;
        public ScoreRepository(Postgres database) => _database = database;

        private async Task Create(Score score)
        {
            await using var connection = await _database.Connection();
            await using var command = new NpgsqlCommand("INSERT INTO Scores (Type, Value, UserId) VALUES (@type, @value, @userId)", connection);
            command.Parameters.AddWithValue("type", score.Type);
            command.Parameters.AddWithValue("value", score.Value);
            command.Parameters.AddWithValue("userId", score.User.Id);

            await command.ExecuteNonQueryAsync();
        }
    }
}