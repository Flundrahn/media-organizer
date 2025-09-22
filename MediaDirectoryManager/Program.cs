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

var services = new ServiceCollection();
var serviceProvider = services
    .Configure<MediaOrganizerSettings>(configuration.GetSection(MediaOrganizerSettings.SectionName))
    .AddSingleton<IOutputWriter, ConsoleOutputWriter>()
    .AddTransient<IFileSystem, FileSystem>()
    .AddTransient<FileSystemValidator>()
    .AddTransient<IMediaFileProvider, MediaFileProvider>()
    .AddTransient<MediaOrganizerService>()
    .BuildServiceProvider();

var settings = configuration.GetSection(MediaOrganizerSettings.SectionName).Get<MediaOrganizerSettings>();
var output = serviceProvider.GetRequiredService<IOutputWriter>();
var validator = serviceProvider.GetRequiredService<FileSystemValidator>();

if (settings is null)
{
    output.WriteError("Failed to load configuration from appsettings.json");
    return 1;
}

// Set the validator on the settings, cannot inject because needs parameterless ctor, could put in factory but keep simple for now
settings.SetValidator(validator);

var mediaService = new MediaOrganizerService(output, settings);

return mediaService.Run();
