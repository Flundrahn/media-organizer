using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using MediaOrganizer.IO;
using MediaOrganizer.UI;
using MediaOrganizer.Configuration;
using MediaOrganizer.Validations;
using MediaOrganizer.Services;

namespace MediaOrganizer;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core Media Organizer services with the provided IServiceCollection.
    /// This is intended to be used by both the application host and integration tests.
    /// </summary>
    public static IServiceCollection AddMediaOrganizerServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSingleton(configuration)
            .Configure<MediaOrganizerSettings>(configuration.GetSection(MediaOrganizerSettings.SectionName))
            .AddLogging(builder => builder.AddSimpleConsole())
            .AddSingleton<IConsoleIO, ConsoleIO>()
            .AddTransient<IFileSystem, FileSystem>()
            .AddTransient<FileSystemValidator>()
            .AddTransient<TvShowEpisodeParser>()
            .AddTransient<IDirectoryCleaner, DirectoryCleaner>()
            .AddTransient(provider =>
                new MediaFileOrganizerFactory(
                    resolveFileSystem: () => provider.GetRequiredService<IFileSystem>(),
                    resolveLogger: () => provider.GetRequiredService<ILogger<MediaFileOrganizer>>(),
                    resolveSettings: () => provider.GetRequiredService<IOptions<MediaOrganizerSettings>>(),
                    resolveDirectoryCleaner: () => provider.GetRequiredService<IDirectoryCleaner>()))
            .AddTransient<MediaOrganizerConsoleApp>();
    }
}
