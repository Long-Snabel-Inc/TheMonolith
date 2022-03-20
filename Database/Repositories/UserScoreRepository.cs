using Npgsql;
using TheMonolith.Data;

namespace TheMonolith.Database.Repositories
{
    public class UserScoreRepository
    {
        private readonly Postgres _database;
        public UserScoreRepository(Postgres database) => _database = database;

        public async Task Upsert(User source, User target, int score)
        {
            await using var connection = await _database.Connection();
            await using var updateCommand = new NpgsqlCommand("UPDATE UserScores SET updatedat = @updatedat, score = @score WHERE source = @source AND target = @target;", connection);
            updateCommand.Parameters.AddWithValue("updatedat", DateTime.UtcNow);
            updateCommand.Parameters.AddWithValue("score", score);
            updateCommand.Parameters.AddWithValue("source", source.Id);
            updateCommand.Parameters.AddWithValue("target", target.Id);

            var updateResult = await updateCommand.ExecuteNonQueryAsync();
            if (updateResult < 1)
            {
                await using var command = new NpgsqlCommand("INSERT INTO UserScores (updatedat, source, target, score) VALUES (@updatedat, @source_id, @target_id, @score)", connection);
                command.Parameters.AddWithValue("updatedat", DateTime.UtcNow);
                command.Parameters.AddWithValue("source_id", source.Id);
                command.Parameters.AddWithValue("target_id", target.Id);
                command.Parameters.AddWithValue("score", score);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<DateTime> LatestUpdate(User user)
        {
            await using var connection = await _database.Connection();
            await using var command = new NpgsqlCommand("SELECT UpdatedAt FROM UserScores WHERE source = @source ORDER BY UpdatedAt DESC LIMIT 1", connection);
            command.Parameters.AddWithValue("source", user.Id);

            var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetDateTime(0);
            }

            return DateTime.MinValue;
        }

        public async Task<List<double>> ScoresForTarget(int userId)
        {
            await using var connection = await _database.Connection();
            await using var command = new NpgsqlCommand("SELECT score FROM UserScores WHERE target = @target", connection);
            command.Parameters.AddWithValue("target", userId);

            var reader = await command.ExecuteReaderAsync();
            var scores = new List<double>();
            while (await reader.ReadAsync())
            {
                scores.Add(reader.GetDouble(0));
            }
            return scores;
        } 
    }
}