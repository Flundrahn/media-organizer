using System.IO.Abstractions;
using MediaOrganizer.Configuration;
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
            
            if (OrganizeFile(fileInfo))
            {
                successCount++;
            }
        }

        _logger.LogInformation("Organization complete: {SuccessCount}/{ProcessedCount} files processed successfully", successCount, processedCount);
        return successCount > 0 || processedCount == 0;
    }

    public bool OrganizeFile(IFileInfo fileInfo)
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
            return false;
        }

        string mediaFileRelativePath = mediaFile.GenerateRelativePath(_settings.TvShowPathTemplate);
        string mediaFileDestinationPath = _fileSystem.Path.Combine(_settings.DestinationDirectory, mediaFileRelativePath);
        string? mediaFileDestinationDir = _fileSystem.Path.GetDirectoryName(mediaFileDestinationPath);

        if (string.IsNullOrEmpty(mediaFileDestinationDir))
        {
            _logger.LogError("Failed to move {FileName} - invalid destination path has no directory component: {FileDestinationPath}", fileInfo.Name, mediaFileDestinationPath);
            return false;
        }

        if (_settings.DryRun)
        {
            _logger.LogInformation("[DRY RUN] Would move: {FileName} -> {FileDestinationPath}", fileInfo.Name, mediaFileDestinationPath);
            return true;
        }

        try
        {
            if (!_fileSystem.Directory.Exists(mediaFileDestinationDir))
            {
                _fileSystem.Directory.CreateDirectory(mediaFileDestinationDir);
            }

            _fileSystem.File.Move(fileInfo.FullName, mediaFileDestinationPath);
            _logger.LogInformation("Moved: {FileName} -> {FileDestinationPath}", fileInfo.Name, mediaFileDestinationPath);
            // TODO: update current file info on model object
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move {FileName}", fileInfo.FullName);
            return false;
        }
    }
}