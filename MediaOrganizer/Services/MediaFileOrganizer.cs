using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using MediaOrganizer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

public class OrganizationResult
{
    public int ProcessedCount { get; set; }
    public int OrganizedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public bool Success => OrganizedCount > 0 || ProcessedCount == 0;
}

public class MediaFileOrganizer
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<MediaFileOrganizer> _logger;
    private readonly ITvShowEpisodeParser _parser;
    private readonly MediaOrganizerSettings _settings;
    
    private Stack<IFileInfo> _fileStack;
    private OrganizationResult _result;

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
        _fileStack = new Stack<IFileInfo>();
        _result = new OrganizationResult();
    }

    public void Initialize(IEnumerable<IFileInfo> mediaFiles)
    {
        _fileStack = new Stack<IFileInfo>(mediaFiles.Reverse());
        _result = new OrganizationResult();
    }

    public int RemainingCount => _fileStack.Count;
    public OrganizationResult Result => _result;

    public TvShowEpisode? PeekFile()
    {
        if (_fileStack.Count == 0) return null;
        
        var nextFile = _fileStack.Peek();
        return _parser.Parse(nextFile);
    }

    public TvShowEpisode? OrganizeFile()
    {
        if (_fileStack.Count == 0) return null;
        
        var fileInfo = _fileStack.Pop();
        _result.ProcessedCount++;
        
        return OrganizeFileInternal(fileInfo);
    }

    public void SkipFile()
    {
        if (_fileStack.Count == 0) return;
        
        var fileInfo = _fileStack.Pop();
        _result.ProcessedCount++;
        _result.SkippedCount++;
        
        _logger.LogDebug("Skipped {FileName} by user request", fileInfo.Name);
    }

    public OrganizationResult OrganizeAllFiles()
    {
        while (_fileStack.Count > 0)
        {
            OrganizeFile();
        }
        
        _logger.LogInformation("Organization complete: {OrganizedCount} organized, {SkippedCount} skipped, {FailedCount} failed out of {ProcessedCount} total files", 
            _result.OrganizedCount, _result.SkippedCount, _result.FailedCount, _result.ProcessedCount);
        
        return _result;
    }

    private TvShowEpisode? OrganizeFileInternal(IFileInfo fileInfo)
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
            _result.FailedCount++;
            return null;
        }

        string mediaFileRelativePath = mediaFile.GenerateRelativePath(_settings.TvShowPathTemplate);
        string mediaFileDestinationPath = _fileSystem.Path.Combine(_settings.DestinationDirectory, mediaFileRelativePath);
        string? mediaFileDestinationDir = _fileSystem.Path.GetDirectoryName(mediaFileDestinationPath);

        if (string.IsNullOrEmpty(mediaFileDestinationDir))
        {
            _logger.LogError("Failed to move {FileName} - invalid destination path has no directory component: {FileDestinationPath}", fileInfo.Name, mediaFileDestinationPath);
            _result.FailedCount++;
            return null;
        }

        // Skip if file is already organized in the correct location
        if (mediaFile.IsOrganized(_settings))
        {
            _logger.LogInformation("Skipped {FileName} - already organized in correct location", fileInfo.Name);
            _result.OrganizedCount++;
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
                _result.FailedCount++;
                return null;
            }
        }

        _logger.LogInformation("{Prefix} {FileName} -> {FileDestinationPath}",
                               _settings.DryRun ? "[DRY RUN] Would move:" : "Moved:",
                               fileInfo.Name,
                               mediaFileDestinationPath);

        mediaFile.CurrentFile = _fileSystem.FileInfo.New(mediaFileDestinationPath);
        _result.OrganizedCount++;
        return mediaFile;
    }
}