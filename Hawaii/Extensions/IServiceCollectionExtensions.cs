using Hawaii.Interfaces;
using Hawaii.Services;

namespace Hawaii;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddHawaii(this IServiceCollection services)
    {
        services.AddSingleton<IGestureRecognitionService, GestureRecognitionService>();
		
        services.AddScoped<Scene>();
        services.AddScoped<SceneRenderer>();
        services.AddScoped<EventDispatcher>();


        return services;
    }
}