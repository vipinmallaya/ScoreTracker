using CommunityToolkit.Maui;

namespace ScoreTracker;

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
                fonts.AddFont("FontIcons.ttf", "FontIcons");
            });
        builder.UseMauiApp<App>().UseMauiCommunityToolkit();
        return builder.Build();
	}
}
