using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using MediaOrganizer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

public class MediaFileOrganizer
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<MediaFileOrganizer> _logger;
    private readonly ITvShowEpisodeParser _parser;
    private readonly MediaOrganizerSettings _settings;

    public MediaFileOrganizer(
        IFileSystem fileSystem,
        ILogger<MediaFileOrganizer> logger,
        ITvShowEpisodeParser parser,
        IOptions<MediaOrganizerSettings> settings)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _parser = parser;
        _settings = settings.Value;
    }

    public bool OrganizeFiles(IEnumerable<IFileInfo> mediaFiles)
    {
        var successCount = 0;
        var processedCount = 0;

        foreach (var fileInfo in mediaFiles)
        {
            processedCount++;
            
            var result = OrganizeFile(fileInfo);
            if (result is not null)
            {
                successCount++;
            }
        }

        _logger.LogInformation("Organization complete: {SuccessCount}/{ProcessedCount} files processed successfully", successCount, processedCount);
        return successCount > 0 || processedCount == 0;
    }

    public TvShowEpisode? OrganizeFile(IFileInfo fileInfo)
    {
        var mediaFile = _parser.Parse(fileInfo);

        _logger.LogDebug("Parsed {FileName}: Valid={IsValid}, Show={TvShowName}, S{Season}E{Episode}",
                         fileInfo.Name,
                         mediaFile.IsValid,
                         mediaFile.TvShowName,
                         mediaFile.Season,
                         mediaFile.Episode);

        if (!mediaFile.IsValid)
        {
            _logger.LogWarning("Failed to move {FileName} - unparsable file", fileInfo.Name);
            return null;
        }

        string mediaFileRelativePath = mediaFile.GenerateRelativePath(_settings.TvShowPathTemplate);
        string mediaFileDestinationPath = _fileSystem.Path.Combine(_settings.DestinationDirectory, mediaFileRelativePath);
        string? mediaFileDestinationDir = _fileSystem.Path.GetDirectoryName(mediaFileDestinationPath);

        if (string.IsNullOrEmpty(mediaFileDestinationDir))
        {
            _logger.LogError("Failed to move {FileName} - invalid destination path has no directory component: {FileDestinationPath}", fileInfo.Name, mediaFileDestinationPath);
            return null;
        }

        // Skip if file is already organized in the correct location
        if (mediaFile.IsOrganized(_settings))
        {
            _logger.LogInformation("Skipped {FileName} - already organized in correct location", fileInfo.Name);
            return mediaFile;
        }

        if (!_settings.DryRun)
        {
            try
            {
                if (!_fileSystem.Directory.Exists(mediaFileDestinationDir))
                {
                    _fileSystem.Directory.CreateDirectory(mediaFileDestinationDir);
                }

                _fileSystem.File.Move(fileInfo.FullName, mediaFileDestinationPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move {FileName}", fileInfo.FullName);
                return null;
            }
        }

        _logger.LogInformation("{Prefix} {FileName} -> {FileDestinationPath}",
                               _settings.DryRun ? "[DRY RUN] Would move:" : "Moved:",
                               fileInfo.Name,
                               mediaFileDestinationPath);

        mediaFile.CurrentFile = _fileSystem.FileInfo.New(mediaFileDestinationPath);
        return mediaFile;
    }
}