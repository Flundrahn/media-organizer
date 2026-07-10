using MediaOrganizer;
using MediaOrganizer.Configuration;
using MediaOrganizer.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
Console.WriteLine($"Running in environment: {environment}");
Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine("");

var hostBuilder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    // NOTE: Necessary since ContentRootPath defaults to CurrentDirectory i.e. where app is launched from,
    // and we want it to be location of app output folder
    Args = args,
    // ContentRootPath = AppContext.BaseDirectory
});

    // hostBuilder.Services.AddAppLayer(hostBuilder.Configuration)
    //                     .AddUi();

// var serviceProvider = services
hostBuilder.Services.AddMediaOrganizerServices(hostBuilder.Configuration);

var host = hostBuilder.Build();

// Validate settings early
var settingsOptions = host.Services.GetRequiredService<IOptions<MediaOrganizerSettings>>();
var settings = settingsOptions.Value;

if (settings is null)
{
    Console.WriteLine("Failed to load configuration from appsettings.json");
    return 1;
}

var mediaService = host.Services.GetRequiredService<MediaOrganizerConsoleApp>();
return mediaService.Run();
