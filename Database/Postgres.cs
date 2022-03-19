using Npgsql;

namespace TheMonolith.Database
{
    public class Postgres
    {
        private readonly PostgresOptions _options;
        public Postgres(PostgresOptions options) => _options = options;

        public async Task<NpgsqlConnection> Connection()
        {
            var connection = new NpgsqlConnection(_options.ConnectionString);
            await connection.OpenAsync();
        
            return connection;
        }
    }
}