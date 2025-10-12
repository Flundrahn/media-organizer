using MediaOrganizer.Configuration;
using MediaOrganizer.UI;
using MediaOrganizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
Console.WriteLine($"Running in environment: {environment}");
Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var services = new ServiceCollection();
var serviceProvider = services
    .AddMediaOrganizerServices(configuration)
    .BuildServiceProvider();

// Validate settings early
var settingsOptions = serviceProvider.GetRequiredService<IOptions<MediaOrganizerSettings>>();
var settings = settingsOptions.Value;

if (settings is null)
{
    Console.WriteLine("Failed to load configuration from appsettings.json");
    return 1;
}

var mediaService = serviceProvider.GetRequiredService<MediaOrganizerConsoleApp>();
return mediaService.Run();
