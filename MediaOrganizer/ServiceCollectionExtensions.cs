using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using MediaOrganizer.IO;
using MediaOrganizer.Services;
using MediaOrganizer.UI;
using MediaOrganizer.Configuration;
using MediaOrganizer.Validations;
using MediaOrganizer.Infrastructure.ApiClients;
using TMDbLib.Client;

namespace MediaOrganizer;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core Media Organizer services with the provided IServiceCollection.
    /// This is intended to be used by both the application host and integration tests.
    /// </summary>
    public static IServiceCollection AddMediaOrganizerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton(configuration)
            .Configure<MediaOrganizerSettings>(configuration.GetSection(MediaOrganizerSettings.SectionName))
            .Configure<TmdbApiConfig>(configuration.GetSection(TmdbApiConfig.SectionName))
            .AddHttpClient()
            .AddLogging(builder => builder.AddSimpleConsole())
            .AddSingleton<IConsoleIO, ConsoleIO>()
            .AddTransient<IFileSystem, FileSystem>()
            .AddTransient<FileSystemValidator>()
            .AddTransient<IDirectoryCleaner, DirectoryCleaner>()
            .AddTransient<MediaOrganizerConsoleApp>()
            .AddMediaFileOrganizerFactory()
            .AddTmdbApi();

        return services;
    }

    private static IServiceCollection AddMediaFileOrganizerFactory(this IServiceCollection services)
    {
        services.AddTransient(provider =>
        {
            return new MediaFileOrganizerFactory(
                resolveFileSystem: () => provider.GetRequiredService<IFileSystem>(),
                resolveLogger: () => provider.GetRequiredService<ILogger<MediaFileOrganizer>>(),
                resolveSettings: () => provider.GetRequiredService<IOptions<MediaOrganizerSettings>>());
        });

        return services;
    }

    private static IServiceCollection AddTmdbApi(this IServiceCollection services)
    {
        services.AddTransient(provider =>
        {
            var tmdbApiConfig = provider.GetRequiredService<IOptions<TmdbApiConfig>>().Value;
            return new TMDbClient(tmdbApiConfig.ApiKey);
        });

        return services;
    }
}
