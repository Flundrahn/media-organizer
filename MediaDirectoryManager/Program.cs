using MediaDirectoryManager.Configuration;
using MediaDirectoryManager.Output;
using MediaDirectoryManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.Configure<MediaOrganizerSettings>(configuration.GetSection(MediaOrganizerSettings.SectionName));

services.AddSingleton<IOutputWriter, ConsoleOutputWriter>();
services.AddTransient<MediaDirectoryManagerService>();

var serviceProvider = services.BuildServiceProvider();

var settings = configuration.GetSection(MediaOrganizerSettings.SectionName).Get<MediaOrganizerSettings>();

if (settings == null)
{
    var output = serviceProvider.GetRequiredService<IOutputWriter>();
    output.WriteError("Failed to load configuration from appsettings.json");
    return 1;
}

var mediaService = new MediaDirectoryManagerService(
    serviceProvider.GetRequiredService<IOutputWriter>(),
    settings
);

return mediaService.Run();
