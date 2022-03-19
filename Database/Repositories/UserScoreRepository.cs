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
    }
}