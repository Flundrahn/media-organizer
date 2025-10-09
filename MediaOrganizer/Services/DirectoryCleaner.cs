using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

/// <summary>
/// Service for cleaning up empty directories in source and destination paths
/// </summary>
public class DirectoryCleaner : IDirectoryCleaner
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DirectoryCleaner> _logger;
    private readonly MediaOrganizerSettings _settings;

    public DirectoryCleaner(
        IFileSystem fileSystem,
        ILogger<DirectoryCleaner> logger,
        IOptions<MediaOrganizerSettings> settings)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <summary>
    /// Cleans up empty directories in all media source and destination directories and their subdirectories
    /// </summary>
    public void CleanEmptyDirectories()
    {
        CleanEmptyDirectoriesInPath(_settings.TvShowSourceDirectory);
        CleanEmptyDirectoriesInPath(_settings.TvShowDestinationDirectory);
        CleanEmptyDirectoriesInPath(_settings.MovieSourceDirectory);
        CleanEmptyDirectoriesInPath(_settings.MovieDestinationDirectory);
    }

    private void CleanEmptyDirectoriesInPath(string rootPath)
    {
        if (!_fileSystem.Directory.Exists(rootPath))
            return;

        try
        {
            var directories = _fileSystem.Directory
                .GetDirectories(rootPath, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length); // Process deepest directories first

            foreach (var directory in directories)
            {
                if (_fileSystem.Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    continue;
                }

                if (_settings.DryRun)
                {
                    _logger.LogInformation("[DRY RUN] Would remove empty directory: {DirectoryPath}", directory);
                }
                else
                {
                    _fileSystem.Directory.Delete(directory);
                    _logger.LogInformation("Removed empty directory: {DirectoryPath}", directory);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean empty directories in path: {RootPath}", rootPath);
        }
    }
}