using System.IO.Abstractions;
using MediaOrganizer.Configuration;

namespace MediaOrganizer.Services;

public interface IMediaFileProvider
{
    IEnumerable<string> GetMediaFiles(string directoryPath);
}

public class MediaFileProvider : IMediaFileProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly bool _includeSubdirectories;
    private readonly HashSet<string> _videoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".3gp"
    };

    public MediaFileProvider(IFileSystem fileSystem, MediaOrganizerSettings settings)
    {
        _fileSystem = fileSystem;
        _includeSubdirectories = settings.IncludeSubdirectories;
    }

    public IEnumerable<string> GetMediaFiles(string directoryPath)
    {
        if (!_fileSystem.Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var searchOption = _includeSubdirectories
            ? SearchOption.AllDirectories 
            : SearchOption.TopDirectoryOnly;
        
        var files = _fileSystem.Directory.EnumerateFiles(directoryPath, "*", searchOption);

        foreach (var file in files)
        {
            var extension = _fileSystem.Path.GetExtension(file);
            if (_videoExtensions.Contains(extension))
            {
                yield return file;
            }
        }
    }
}