namespace TheMonolith.Database
{
    public static class PostgresExtensions
    {
        public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
        {
            services.Options<PostgresOptions>(configuration)
                .AddSingleton<Postgres>();
            
            return services;
        }
    }
}