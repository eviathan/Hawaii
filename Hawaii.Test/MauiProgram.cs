using Hawaii.Interfaces;
using Hawaii.Nodes;
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

		builder.Services.AddHawaii();

        builder.Services.AddScoped<SceneCamera>();
        builder.Services.AddScoped<ISceneBuilder, DebugSceneBuilder>(); 
		
		builder.Services.AddTransient<ImageNode>();
		builder.Services.AddTransient<FeatureNode>();
		builder.Services.AddTransient<FeatureHandleNode>();
		
		return builder.Build();
	}
}
