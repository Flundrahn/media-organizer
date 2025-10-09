using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using MediaOrganizer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

/// <summary>
/// Factory for creating media file organizers with different configurations.
/// Uses delegate injection to resolve dependencies while maintaining proper service lifetimes.
/// </summary>
public class MediaFileOrganizerFactory
{
    private readonly Func<IFileSystem> _resolveFileSystem;
    private readonly Func<ILogger<MediaFileOrganizer>> _resolveLogger;
    private readonly Func<IOptions<MediaOrganizerSettings>> _resolveSettings;

    public MediaFileOrganizerFactory(
        Func<IFileSystem> resolveFileSystem,
        Func<ILogger<MediaFileOrganizer>> resolveLogger,
        Func<IOptions<MediaOrganizerSettings>> resolveSettings)
    {
        _resolveFileSystem = resolveFileSystem;
        _resolveLogger = resolveLogger;
        _resolveSettings = resolveSettings;
    }

    public MediaFileOrganizer CreateTvShowOrganizer()
    {
        var fileSystem = _resolveFileSystem();
        var settings = _resolveSettings();

        var mediaFileProvider = new MediaFileProvider(fileSystem, settings, MediaType.TvShow);
        var tvShowFiles = mediaFileProvider.GetMediaFiles();

        return new MediaFileOrganizer(
            fileSystem,
            _resolveLogger(),
            new TvShowEpisodeParser(),
            settings,
            tvShowFiles);
    }

    public MediaFileOrganizer CreateMovieOrganizer()
    {
        var fileSystem = _resolveFileSystem();
        var settings = _resolveSettings();
        
        var mediaFileProvider = new MediaFileProvider(fileSystem, settings, MediaType.Movie);
        var movieFiles = mediaFileProvider.GetMediaFiles();

        return new MediaFileOrganizer(
            fileSystem,
            _resolveLogger(),
            new MovieParser(),
            settings,
            movieFiles
        );
    }
}