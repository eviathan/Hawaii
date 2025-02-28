using Hawaii.Interfaces;
using Hawaii.Services;
using Hawaii.Test.Nodes;
using Hawaii.Test.SceneBuilders;
using Microsoft.Extensions.Logging;

namespace Hawaii.Test;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		
		builder.Services.AddSingleton<IGestureRecognitionService, GestureRecognitionService>();
		
		builder.Services.AddScoped<Scene>();
		builder.Services.AddScoped<SceneRenderer>();
		builder.Services.AddScoped<EventDispatcher>();
		
		builder.Services.AddScoped<ISceneBuilder, FeatureSceneBuilder>(); 
		
		builder.Services.AddTransient<FeatureNode>();
		builder.Services.AddTransient<FeatureHandleNode>();
		builder.Services.AddTransient<ImageNode>();
		
		return builder.Build();
	}
}
