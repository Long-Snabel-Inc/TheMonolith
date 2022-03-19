using Npgsql;
using TheMonolith.Data;

namespace TheMonolith.Database.Repositories
{
    public class ScoreRepository
    {
        private readonly Postgres _database;
        public ScoreRepository(Postgres database) => _database = database;

        public async Task Create(Score score)
        {
            await using var connection = await _database.Connection();
            await using var command = new NpgsqlCommand("INSERT INTO Scores (Type, Value, UserId) VALUES (@type, @value, @userId)", connection);
            command.Parameters.AddWithValue("type", score.Type);
            command.Parameters.AddWithValue("value", score.Value);
            command.Parameters.AddWithValue("userId", score.User.Id);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<double> Score(User user)
        {
            await using var connection = await _database.Connection();
            await using var command = new NpgsqlCommand("SELECT SUM(Value) FROM scores WHERE userId = @userId ", connection);
            command.Parameters.AddWithValue("userId", user.Id);

            return (double)(await command.ExecuteScalarAsync() ?? 0.0d);
        }

        public async IAsyncEnumerable<Score> GetUserScores(User user)
        {
            await using var connection = await _database.Connection();
            await using var command = new NpgsqlCommand("SELECT type, value FROM scores WHERE userId = @userId", connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var type = reader.GetString(0);
                var value = reader.GetDouble(1);

                yield return new Score(user, type, value);
            }
        }
    }
}