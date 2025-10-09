using MediaOrganizer.Services;
using MediaOrganizer.Configuration;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Options;
using MediaOrganizer.Models;

namespace MediaOrganizer.Tests.Services;

public class MediaFileProviderTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly MediaOrganizerSettings _settings;

    public MediaFileProviderTests()
    {
        _mockFileSystem = new MockFileSystem();
        _settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\TvShows",
            VideoFileExtensions = new List<string> { ".mp4", ".avi", ".mkv" }
        };
    }

    [Fact]
    public void GetMediaFiles_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        // Default mockFileSystem does not have the directory
        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => sut.GetMediaFiles().ToList());
    }

    [Fact]
    public void GetMediaFiles_WithVideoFiles_ReturnsOnlyVideoFiles()
    {
        // Arrange
        const string photoFile = @"C:\TvShows\photo.jpg";
        const string videoFile = @"C:\TvShows\video.mp4";
        const string documentFile = @"C:\TvShows\document.txt";
        const string movieFile = @"C:\TvShows\movie.avi";
        const string musicFile = @"C:\TvShows\music.mp3";

        _mockFileSystem.AddFile(photoFile, new MockFileData(""));
        _mockFileSystem.AddFile(videoFile, new MockFileData(""));
        _mockFileSystem.AddFile(documentFile, new MockFileData(""));
        _mockFileSystem.AddFile(movieFile, new MockFileData(""));
        _mockFileSystem.AddFile(musicFile, new MockFileData(""));

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.FullName == videoFile);
        Assert.Contains(result, f => f.FullName == movieFile);
        Assert.DoesNotContain(result, f => f.FullName == photoFile);
        Assert.DoesNotContain(result, f => f.FullName == documentFile);
        Assert.DoesNotContain(result, f => f.FullName == musicFile);
    }

    [Fact]
    public void GetMediaFiles_WithIncludeSubdirectoriesTrue_IncludesSubdirectoryFiles()
    {
        // Arrange
        const string videoFile = @"C:\TvShows\video.mp4";
        const string subfolderMovieFile = @"C:\TvShows\Subfolder\movie.mkv";

        _mockFileSystem.AddFile(videoFile, new MockFileData(""));
        _mockFileSystem.AddFile(subfolderMovieFile, new MockFileData(""));
        _settings.IncludeSubdirectories = true;

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.FullName == videoFile);
        Assert.Contains(result, f => f.FullName == subfolderMovieFile);
    }

    [Fact]
    public void GetMediaFiles_WithIncludeSubdirectoriesFalse_ExcludesSubdirectoryFiles()
    {
        // Arrange
        const string videoFile = @"C:\TvShows\video.mp4";
        const string subfolderMovieFile = @"C:\TvShows\Subfolder\movie.mkv";

        _mockFileSystem.AddFile(videoFile, new MockFileData(""));
        _mockFileSystem.AddFile(subfolderMovieFile, new MockFileData(""));
        _settings.IncludeSubdirectories = false;
        
        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, f => f.FullName == videoFile);
    }

    [Fact]
    public void GetMediaFiles_WithCaseInsensitiveExtensions_ReturnsAllVideoFiles()
    {
        // Arrange
        const string videoFileUpper = @"C:\TvShows\video.MP4";
        const string movieFileMixed = @"C:\TvShows\movie.Avi";

        _mockFileSystem.AddFile(videoFileUpper, new MockFileData(""));
        _mockFileSystem.AddFile(movieFileMixed, new MockFileData(""));

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }
}