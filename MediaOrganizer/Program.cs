using MediaOrganizer.Configuration;
using MediaOrganizer.Output;
using MediaOrganizer.Services;
using MediaOrganizer.Validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
Console.WriteLine($"Running in environment: {environment}");
Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
var serviceProvider = services
    .AddSingleton<IConfiguration>(configuration)
    .Configure<MediaOrganizerSettings>(configuration.GetSection(MediaOrganizerSettings.SectionName))
    .AddLogging()
    .AddSingleton<IOutputWriter, ConsoleOutputWriter>()
    .AddTransient<IFileSystem, FileSystem>()
    .AddTransient<FileSystemValidator>()
    .AddTransient<IMediaFileProvider, MediaFileProvider>()
    .AddTransient<ITvShowEpisodeParser, TvShowEpisodeParser>()
    .AddTransient<MediaFileOrganizer>()
    .AddTransient<MediaOrganizerService>()
    .BuildServiceProvider();

// Validate settings early
var settingsOptions = serviceProvider.GetRequiredService<IOptions<MediaOrganizerSettings>>();
var settings = settingsOptions.Value;

if (settings is null)
{
    Console.WriteLine("Failed to load configuration from appsettings.json");
    return 1;
}


var mediaService = serviceProvider.GetRequiredService<MediaOrganizerService>();

return mediaService.Run();
