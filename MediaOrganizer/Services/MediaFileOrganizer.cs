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
    private readonly ILogger<MediaFileOrganizer> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IMediaFileParser _parser;
    private readonly MediaOrganizerSettings _settings;
    private readonly OrganizationResult _result = new();
    private readonly Stack<IFileInfo> _files;
    private readonly IReadOnlyList<IFileInfo> _allFiles;

    public int RemainingCount => _files.Count;
    public IReadOnlyList<IFileInfo> AllFiles => _allFiles;
    public OrganizationResult Result => _result;

    internal MediaFileOrganizer(
        IFileSystem fileSystem,
        // NOTE: Investigate how logger injected from subclass works
        ILogger<MediaFileOrganizer> logger,
        IMediaFileParser parser,
        IOptions<MediaOrganizerSettings> settings,
        IEnumerable<IFileInfo> mediaFiles)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _parser = parser;
        _settings = settings.Value;
        
        // TODO: performance, later avoid doing toList if possible, although important part is probably when making changes later
        // note also that this would mean 'AllFiles' are the initial state of the files, after organize
        var mediaFilesList = mediaFiles.ToList();
        _allFiles = mediaFilesList.AsReadOnly();
        _files = new Stack<IFileInfo>(mediaFilesList);
    }

    public IMediaFile? PeekFile()
    {
        if (_files.Count == 0) return null;

        var nextFile = _files.Peek();
        return _parser.Parse(nextFile);
    }

    public IMediaFile? OrganizeFile()
    {
        if (_files.Count == 0) return null;

        var fileInfo = _files.Pop();

        // Currently counting as processed as soon as we remove from file stack, no matter result.
        _result.ProcessedCount++;

        return OrganizeFileInternal(fileInfo);
    }

    public void SkipFile()
    {
        if (_files.Count == 0) return;

        var fileInfo = _files.Pop();
        _result.ProcessedCount++;
        _result.SkippedCount++;

        _logger.LogDebug("Skipped {FileName} by user request", fileInfo.Name);
    }

    public OrganizationResult OrganizeAllFiles()
    {
        while (_files.Count > 0)
        {
            OrganizeFile();
        }

        _logger.LogInformation(
            "Organization complete: {OrganizedCount} organized, {SkippedCount} skipped, {FailedCount} failed out of {ProcessedCount} total files",
            _result.OrganizedCount,
            _result.SkippedCount,
            _result.FailedCount,
            _result.ProcessedCount);

        return Result;
    }

    private IMediaFile? OrganizeFileInternal(IFileInfo fileInfo)
    {
        var mediaFile = _parser.Parse(fileInfo);

        _logger.LogDebug("Parsed {FileName}: Valid={IsValid}, Type={MediaType}",
                         fileInfo.Name,
                         mediaFile.IsValid,
                         mediaFile.Type);

        if (!mediaFile.IsValid)
        {
            _logger.LogWarning("Failed to move {FileName} - unparsable file", fileInfo.Name);
            _result.FailedCount++;
            return null;
        }

        string mediaFileDestinationPath = mediaFile.GenerateFullPath(_settings);
        string? mediaFileDestinationDir = _fileSystem.Path.GetDirectoryName(mediaFileDestinationPath);

        if (string.IsNullOrEmpty(mediaFileDestinationDir))
        {
            _logger.LogError("Failed to move {FileName} - invalid destination path has no directory component: {FileDestinationPath}", fileInfo.Name, mediaFileDestinationPath);
            _result.FailedCount++;
            return null;
        }

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

                var originalFilePath = fileInfo.FullName;
                _fileSystem.File.Move(originalFilePath, mediaFileDestinationPath);
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