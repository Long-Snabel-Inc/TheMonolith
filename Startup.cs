using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TheMonolith.Database;

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
        
        services.AddRouting();
        services.AddControllers();
        
        services.AddSwaggerGen(ConfigureSwagger);
    }
    
    private void ConfigureSwagger(SwaggerGenOptions config)
    {
        config.SwaggerDoc("monolith", new OpenApiInfo {Title = "The Monolith API", Version = "v1"});
        config.IncludeXmlComments("TheMonolith.xml", true);
        config.IgnoreObsoleteActions();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        
        app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/monolith/swagger.json", "The Monolith API"));
        
        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}