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
    private readonly Func<IDirectoryCleaner> _resolveDirectoryCleaner;

    public MediaFileOrganizerFactory(
        Func<IFileSystem> resolveFileSystem,
        Func<ILogger<MediaFileOrganizer>> resolveLogger,
        Func<IOptions<MediaOrganizerSettings>> resolveSettings,
        Func<IDirectoryCleaner> resolveDirectoryCleaner)
    {
        _resolveFileSystem = resolveFileSystem;
        _resolveLogger = resolveLogger;
        _resolveSettings = resolveSettings;
        _resolveDirectoryCleaner = resolveDirectoryCleaner;
    }

    public MediaFileOrganizer CreateTvShowOrganizer()
    {
        var fileSystem = _resolveFileSystem();
        var settings = _resolveSettings();

        var mediaFileProvider = new MediaFileProvider(fileSystem, settings, MediaType.TvShow);
        var mediaFiles = mediaFileProvider.GetMediaFiles();

        return new MediaFileOrganizer(
            fileSystem,
            _resolveLogger(),
            new TvShowEpisodeParser(),
            settings,
            _resolveDirectoryCleaner(),
            mediaFiles);
    }

    public MediaFileOrganizer CreateMovieOrganizer()
    {
        var fileSystem = _resolveFileSystem();
        var settings = _resolveSettings();
        
        // Configuration logic for movie organizer (to be implemented)
        // var parser = new MovieParser(); // Future implementation
        // var mediaFileProvider = new MediaFileProvider(fileSystem, settings, MediaType.Movie);

        throw new NotImplementedException("Movie organizer will be implemented in the future");
    }
}