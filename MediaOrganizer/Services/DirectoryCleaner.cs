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

    /// <summary>
    /// Cleans up empty directories in specific source and destination directories
    /// </summary>
    /// <param name="sourceDirectory">The source directory to clean</param>
    /// <param name="destinationDirectory">The destination directory to clean</param>
    /// <exception cref="ArgumentException">Thrown when directories are not within configured media directories</exception>
    public void CleanEmptyDirectories(string sourceDirectory, string destinationDirectory)
    {
        ValidateDirectoryPath(sourceDirectory, "sourceDirectory");
        ValidateDirectoryPath(destinationDirectory, "destinationDirectory");

        CleanEmptyDirectoriesInPath(sourceDirectory);
        CleanEmptyDirectoriesInPath(destinationDirectory);
    }

    /// <summary>
    /// Validates that a directory path is within or matches one of the configured media directories
    /// </summary>
    /// <param name="directoryPath">The directory path to validate</param>
    /// <param name="parameterName">The parameter name for exception messages</param>
    /// <exception cref="ArgumentException">Thrown when the directory is not within configured media directories</exception>
    private void ValidateDirectoryPath(string directoryPath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be null or empty", parameterName);
        }

        var normalizedPath = Path.GetFullPath(directoryPath);
        
        // Get all configured media directories, excluding empty/null/whitespace ones
        var allowedDirectories = new[]
        {
            _settings.TvShowSourceDirectory,
            _settings.TvShowDestinationDirectory,
            _settings.MovieSourceDirectory,
            _settings.MovieDestinationDirectory
        }.Where(dir => !string.IsNullOrWhiteSpace(dir))
         .Select(dir => Path.GetFullPath(dir));

        // Ensure we have at least one valid configured directory
        if (!allowedDirectories.Any())
        {
            throw new ArgumentException(
                "No valid media directories are configured. Cannot perform directory cleanup.",
                parameterName);
        }

        // Check if the path matches or is within any of the allowed directories
        var isAllowed = allowedDirectories.Any(allowedDir => 
            IsEqualOrSubdirectory(normalizedPath, allowedDir));

        if (!isAllowed)
        {
            throw new ArgumentException(
                $"Directory '{directoryPath}' is not within any configured media directories. " +
                $"Allowed directories: {string.Join(", ", allowedDirectories.ToArray())}", 
                parameterName);
        }
    }

    /// <summary>
    /// Checks if a directory is a subdirectory of another directory, or the directory itself
    /// </summary>
    /// <param name="candidateDir">The potential subdirectory</param>
    /// <param name="baseDir">The base directory</param>
    /// <returns>True if candidateDir is a the same or a subdirectory of baseDir</returns>
    private static bool IsEqualOrSubdirectory(string candidateDir, string baseDir)
    {
        // Safety check: never allow empty/null base directories
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            return false;
        }

        if (string.Equals(candidateDir, baseDir, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var candidateInfo = new DirectoryInfo(candidateDir);
        var baseInfo = new DirectoryInfo(baseDir);

        // Normalize paths for comparison
        var candidatePath = candidateInfo.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var basePath = baseInfo.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return candidatePath.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
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