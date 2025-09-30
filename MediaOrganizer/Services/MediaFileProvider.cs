using System.IO.Abstractions;
using MediaOrganizer.Configuration;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Services;

public interface IMediaFileProvider
{
    IEnumerable<IFileInfo> GetTvShowFiles();
    IEnumerable<IFileInfo> GetMovieFiles();
}

public class MediaFileProvider : IMediaFileProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly bool _includeSubdirectories;
    private readonly HashSet<string> _videoExtensions;
    private readonly MediaOrganizerSettings _settings;

    public MediaFileProvider(IFileSystem fileSystem, IOptions<MediaOrganizerSettings> settings)
    {
        _fileSystem = fileSystem;
        _includeSubdirectories = settings.Value.IncludeSubdirectories;
        _videoExtensions = new HashSet<string>(settings.Value.VideoFileExtensions, StringComparer.OrdinalIgnoreCase);
        _settings = settings.Value;
    }

    public IEnumerable<IFileInfo> GetTvShowFiles()
    {
        return GetMediaFilesFromDirectory(_settings.TvShowSourceDirectory);
    }

    public IEnumerable<IFileInfo> GetMovieFiles()
    {
        return GetMediaFilesFromDirectory(_settings.MovieSourceDirectory);
    }

    private IEnumerable<IFileInfo> GetMediaFilesFromDirectory(string directoryPath)
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