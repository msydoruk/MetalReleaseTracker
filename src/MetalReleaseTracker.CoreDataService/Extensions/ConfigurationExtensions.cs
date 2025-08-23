namespace MetalReleaseTracker.CoreDataService.Extensions;

public static class ConfigurationExtensions
{
    public static T BindAndConfigure<T>(this IServiceCollection services, IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        var settings = new T();
        var section = configuration.GetSection(sectionName);
        section.Bind(settings);
        services.Configure<T>(section);
        return settings;
    }
}