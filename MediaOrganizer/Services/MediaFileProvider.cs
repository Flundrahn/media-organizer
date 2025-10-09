using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using MediaOrganizer.Models;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

public interface IMediaFileProvider
{
    IEnumerable<IFileInfo> GetMediaFiles();
}

public class MediaFileProvider : IMediaFileProvider
{
    private readonly string _mediaSourceDirectory;
    private readonly IFileSystem _fileSystem;
    private readonly MediaOrganizerSettings _settings;

    public MediaFileProvider(IFileSystem fileSystem, IOptions<MediaOrganizerSettings> settings, MediaType mediaType)
    {
        _fileSystem = fileSystem;
        _settings = settings.Value;
        _mediaSourceDirectory = mediaType == MediaType.TvShow
            ? _settings.TvShowSourceDirectory
            : _settings.MovieSourceDirectory;
    }

    public IEnumerable<IFileInfo> GetMediaFiles()
    {
        var searchOption = _settings.IncludeSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        var files = _fileSystem.Directory.EnumerateFiles(_mediaSourceDirectory, "*", searchOption);

        foreach (var file in files)
        {
            var fileInfo = _fileSystem.FileInfo.New(file);
            
            // Skip files that are in ignored folders
            if (IsFileInIgnoredFolder(fileInfo))
            {
                continue;
            }
            
            if (_settings.VideoFileExtensions.Exists(x => string.Equals(x, fileInfo.Extension, StringComparison.OrdinalIgnoreCase)))
            {
                yield return fileInfo;
            }
        }
    }

    private bool IsFileInIgnoredFolder(IFileInfo fileInfo)
    {
        if (_settings.IgnoredFolders == null || _settings.IgnoredFolders.Count == 0)
        {
            return false;
        }

        var relativePath = Path.GetRelativePath(_mediaSourceDirectory, fileInfo.FullName);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // Check if any part of the path matches an ignored folder (case-insensitive)
        return pathParts.Any(part => _settings.IgnoredFolders.Any(ignored => 
            string.Equals(part, ignored, StringComparison.OrdinalIgnoreCase)));
    }
}