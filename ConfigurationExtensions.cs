public static class ConfigurationExtensions
{
    public static T Instance<T>(this IConfiguration configuration)
        where T: class, new()
    {
        var typeName = typeof(T).Name;
        var optionsIdx = typeName.IndexOf("Options", StringComparison.InvariantCulture);
        var optionsName = optionsIdx == -1 ? typeName : typeName[..optionsIdx];

        var section = configuration.GetSection(optionsName);
        var instance = new T();
        section.Bind(instance);

        return instance;
    }
    public static IServiceCollection Options<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class, new()
    {
        var instance = configuration.Instance<T>();
        services.AddSingleton(instance);
        
        return services;
    }
}