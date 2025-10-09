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
            if (_settings.VideoFileExtensions.Exists(x => string.Equals(x, fileInfo.Extension, StringComparison.OrdinalIgnoreCase)))
            {
                yield return fileInfo;
            }
        }
    }
}