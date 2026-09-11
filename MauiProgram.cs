using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using prototipoGeminiready.Services;


namespace prototipoGeminiready;

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
			});
        builder.Services.AddHttpClient(); // ✅ Necesario para inyectar HttpClient
        builder.Services.AddSingleton<GeminiService>();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<UsuarioService>();
        builder.Services.AddSingleton<AlertService>();
        builder.Services.AddSingleton<MensajeService>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddSingleton<SesionUsuarioService>();
        builder.Services.AddSingleton<SesionUsuario>();
        builder.Services.AddSingleton<SessionState>();   // ⬅️  NUEVO







#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
