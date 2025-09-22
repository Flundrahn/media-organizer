using MediaDirectoryManager.Services;
using MediaDirectoryManager.Configuration;
using System.IO.Abstractions.TestingHelpers;

namespace MediaDirectoryManager.Tests.Services;

public class MediaFileProviderTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly MediaOrganizerSettings _settings;

    public MediaFileProviderTests()
    {
        _mockFileSystem = new MockFileSystem();
        _settings = new MediaOrganizerSettings();
    }

    [Fact]
    public void GetMediaFiles_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        const string nonExistentPath = @"C:\NonExistent";
        var sut = new MediaFileProvider(_mockFileSystem, _settings);

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => sut.GetMediaFiles(nonExistentPath).ToList());
    }

    [Fact]
    public void GetMediaFiles_WithVideoFiles_ReturnsOnlyVideoFiles()
    {
        // Arrange
        const string mediaPath = @"C:\Media";
        const string photoFile = @"C:\Media\photo.jpg";
        const string videoFile = @"C:\Media\video.mp4";
        const string documentFile = @"C:\Media\document.txt";
        const string movieFile = @"C:\Media\movie.avi";
        const string musicFile = @"C:\Media\music.mp3";

        _mockFileSystem.AddFile(photoFile, new MockFileData(""));
        _mockFileSystem.AddFile(videoFile, new MockFileData(""));
        _mockFileSystem.AddFile(documentFile, new MockFileData(""));
        _mockFileSystem.AddFile(movieFile, new MockFileData(""));
        _mockFileSystem.AddFile(musicFile, new MockFileData(""));
        var sut = new MediaFileProvider(_mockFileSystem, _settings);

        // Act
        var result = sut.GetMediaFiles(mediaPath);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(videoFile, result);
        Assert.Contains(movieFile, result);
        Assert.DoesNotContain(photoFile, result);
        Assert.DoesNotContain(documentFile, result);
        Assert.DoesNotContain(musicFile, result);
    }

    [Fact]
    public void GetMediaFiles_WithIncludeSubdirectoriesTrue_IncludesSubdirectoryFiles()
    {
        // Arrange
        const string mediaPath = @"C:\Media";
        const string videoFile = @"C:\Media\video.mp4";
        const string subfolderMovieFile = @"C:\Media\Subfolder\movie.mkv";

        _mockFileSystem.AddFile(videoFile, new MockFileData(""));
        _mockFileSystem.AddFile(subfolderMovieFile, new MockFileData(""));
        var settings = new MediaOrganizerSettings { IncludeSubdirectories = true };
        var sut = new MediaFileProvider(_mockFileSystem, settings);

        // Act
        var result = sut.GetMediaFiles(mediaPath);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(videoFile, result);
        Assert.Contains(subfolderMovieFile, result);
    }

    [Fact]
    public void GetMediaFiles_WithIncludeSubdirectoriesFalse_ExcludesSubdirectoryFiles()
    {
        // Arrange
        const string mediaPath = @"C:\Media";
        const string videoFile = @"C:\Media\video.mp4";
        const string subfolderMovieFile = @"C:\Media\Subfolder\movie.mkv";

        _mockFileSystem.AddFile(videoFile, new MockFileData(""));
        _mockFileSystem.AddFile(subfolderMovieFile, new MockFileData(""));
        var settings = new MediaOrganizerSettings { IncludeSubdirectories = false };
        var sut = new MediaFileProvider(_mockFileSystem, settings);

        // Act
        var result = sut.GetMediaFiles(mediaPath);

        // Assert
        Assert.Single(result);
        Assert.Contains(videoFile, result);
    }

    [Fact]
    public void GetMediaFiles_WithCaseInsensitiveExtensions_ReturnsAllVideoFiles()
    {
        // Arrange
        const string mediaPath = @"C:\Media";
        const string videoFileUpper = @"C:\Media\video.MP4";
        const string movieFileMixed = @"C:\Media\movie.Avi";

        _mockFileSystem.AddFile(videoFileUpper, new MockFileData(""));
        _mockFileSystem.AddFile(movieFileMixed, new MockFileData(""));
        var sut = new MediaFileProvider(_mockFileSystem, _settings);

        // Act
        var result = sut.GetMediaFiles(mediaPath);

        // Assert
        Assert.Equal(2, result.Count());
    }
}