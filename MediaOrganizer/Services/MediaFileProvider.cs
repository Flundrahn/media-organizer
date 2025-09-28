using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

public interface IMediaFileProvider
{
    IEnumerable<IFileInfo> GetMediaFiles(string directoryPath);
}

public class MediaFileProvider : IMediaFileProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly bool _includeSubdirectories;
    private readonly HashSet<string> _videoExtensions;

    public MediaFileProvider(IFileSystem fileSystem, IOptions<MediaOrganizerSettings> settings)
    {
        _fileSystem = fileSystem;
        _includeSubdirectories = settings.Value.IncludeSubdirectories;
        _videoExtensions = new HashSet<string>(settings.Value.VideoFileExtensions, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<IFileInfo> GetMediaFiles(string directoryPath)
    {
        if (!_fileSystem.Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var searchOption = _includeSubdirectories
            ? SearchOption.AllDirectories 
            : SearchOption.TopDirectoryOnly;
        
        var files = _fileSystem.Directory.EnumerateFiles(directoryPath, "*", searchOption);

        foreach (var file in files)
        {
            var fileInfo = _fileSystem.FileInfo.New(file);
            if (_videoExtensions.Contains(fileInfo.Extension))
            {
                yield return fileInfo;
            }
        }
    }
}