using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

internal static class Program
{
    private static (string environment, string[] args) DeriveEnvironment(string[] args) =>
        args.Length switch
        {
            0 => (Environment.MachineName, args),
            1 => (args[0], Array.Empty<string>()),
            _ => (args[0], args[1..])
        };

    private static IConfiguration BuildConfiguration(string[] args, string environment)
    {
        var builder = new ConfigurationBuilder();

        builder.AddIniFile("_config.ini")
               .AddIniFile($"_config.{environment}.ini", true)
               .AddEnvironmentVariables()
               .AddCommandLine(args)
               .AddUserSecrets("snabel-secrets");
        
        return builder.Build();
    }
    
    internal static async Task Main(string[] args)
    {
        var (environment, configurationArgs) = DeriveEnvironment(args);
        var configuration = BuildConfiguration(configurationArgs, environment);

        var options = configuration.Instance<ServiceOptions>();
        
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseUrls("http://localhost:8080")
                   .UseConfiguration(configuration)
                   .UseStartup<Startup>()
                   .UseEnvironment(environment)
                   .UseUrls(options.Urls.Split(';'))
                   .UseKestrel();

        var host = hostBuilder.Build();
        await host.RunAsync();
    }
}