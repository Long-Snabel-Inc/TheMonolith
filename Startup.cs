using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using TheMonolith.Database;
using TheMonolith.Database.Repositories;
using TheMonolith.Services;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.AddConsole()
                .AddDebug();
        });

        services.Options<ServiceOptions>(Configuration);
        services.AddPostgres(Configuration);
        services.AddSingleton<ScoreRepository>()
                .AddSingleton<UserRepository>()
                .AddSingleton<LocationRepository>();
        services.AddSingleton<ScoreService>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                .WithOrigins("https://localhost:7180",
                                "http://localhost:5179",
                                "https://loevig.net")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        services.AddRouting();
        services.AddControllers();

        services.AddSwaggerGen(ConfigureSwagger);

        // configure jwt authentication
        var key = Encoding.ASCII.GetBytes(Configuration.Instance<ServiceOptions>().PrivateKey);
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var userRepository = context.HttpContext.RequestServices.GetRequiredService<UserRepository>();
                    var userId = int.Parse(context.Principal.Identity.Name);
                    var user = userRepository.Get(userId);
                    if (user == null)
                    {
                        // return unauthorized if user no longer exists
                        context.Fail("Unauthorized");
                    }
                    return Task.CompletedTask;
                }
            };
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    }

    private void ConfigureSwagger(SwaggerGenOptions config)
    {
        config.SwaggerDoc("monolith", new OpenApiInfo { Title = "The Monolith API", Version = "v1" });
        config.IncludeXmlComments("TheMonolith.xml", true);
        config.IgnoreObsoleteActions();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {

        app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/monolith/swagger.json", "The Monolith API"));

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseForwardedHeaders();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}