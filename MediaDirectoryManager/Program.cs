using MediaOrganizer.Configuration;
using MediaOrganizer.Output;
using MediaOrganizer.Services;
using MediaOrganizer.Validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

var settings = configuration.GetSection(MediaOrganizerSettings.SectionName).Get<MediaOrganizerSettings>();

if (settings is null)
{
    Console.WriteLine("Failed to load configuration from appsettings.json");
    return 1;
}

var services = new ServiceCollection();
var serviceProvider = services
    .AddSingleton<IOutputWriter, ConsoleOutputWriter>()
    .AddTransient<IFileSystem, FileSystem>()
    .AddTransient<FileSystemValidator>()
    .AddSingleton(settings) // Register the settings instance
    .AddTransient<IMediaFileProvider, MediaFileProvider>()
    .AddTransient<MediaOrganizerService>()
    .BuildServiceProvider();

var output = serviceProvider.GetRequiredService<IOutputWriter>();
var validator = serviceProvider.GetRequiredService<FileSystemValidator>();

// Set the validator on the settings, cannot inject because needs parameterless ctor, could put in factory but keep simple for now
settings.SetValidator(validator);

var mediaFileProvider = serviceProvider.GetRequiredService<IMediaFileProvider>();
var mediaService = new MediaOrganizerService(output, settings, mediaFileProvider);

return mediaService.Run();
