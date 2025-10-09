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

    [Fact]
    public void GetMediaFiles_WithIgnoredFolders_ExcludesFilesInIgnoredFolders()
    {
        // Arrange
        const string normalFile = @"C:\TvShows\episode.mp4";
        const string featurettesFile = @"C:\TvShows\Featurettes\making_of.mp4";
        const string extrasFile = @"C:\TvShows\Extras\deleted_scene.mkv";
        const string behindScenesFile = @"C:\TvShows\Behind the Scenes\interview.avi";

        _mockFileSystem.AddFile(normalFile, new MockFileData(""));
        _mockFileSystem.AddFile(featurettesFile, new MockFileData(""));
        _mockFileSystem.AddFile(extrasFile, new MockFileData(""));
        _mockFileSystem.AddFile(behindScenesFile, new MockFileData(""));
        
        _settings.IncludeSubdirectories = true;
        _settings.IgnoredFolders = new List<string> { "Featurettes", "Extras", "Behind the Scenes" };

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, f => f.FullName == normalFile);
        Assert.DoesNotContain(result, f => f.FullName == featurettesFile);
        Assert.DoesNotContain(result, f => f.FullName == extrasFile);
        Assert.DoesNotContain(result, f => f.FullName == behindScenesFile);
    }

    [Fact]
    public void GetMediaFiles_WithIgnoredFoldersCaseInsensitive_ExcludesFiles()
    {
        // Arrange
        const string normalFile = @"C:\TvShows\episode.mp4";
        const string extrasFileUpperCase = @"C:\TvShows\EXTRAS\deleted_scene.mkv";
        const string featurettesFileLowerCase = @"C:\TvShows\featurettes\making_of.mp4";

        _mockFileSystem.AddFile(normalFile, new MockFileData(""));
        _mockFileSystem.AddFile(extrasFileUpperCase, new MockFileData(""));
        _mockFileSystem.AddFile(featurettesFileLowerCase, new MockFileData(""));
        
        _settings.IncludeSubdirectories = true;
        _settings.IgnoredFolders = new List<string> { "Extras", "Featurettes" };

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, f => f.FullName == normalFile);
        Assert.DoesNotContain(result, f => f.FullName == extrasFileUpperCase);
        Assert.DoesNotContain(result, f => f.FullName == featurettesFileLowerCase);
    }

    [Fact]
    public void GetMediaFiles_WithNestedIgnoredFolders_ExcludesDeepNesting()
    {
        // Arrange
        const string normalFile = @"C:\TvShows\Season 1\episode.mp4";
        const string nestedIgnoredFile = @"C:\TvShows\Season 1\Extras\Deleted Scenes\scene.mkv";

        _mockFileSystem.AddFile(normalFile, new MockFileData(""));
        _mockFileSystem.AddFile(nestedIgnoredFile, new MockFileData(""));
        
        _settings.IncludeSubdirectories = true;
        _settings.IgnoredFolders = new List<string> { "Extras" };

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, f => f.FullName == normalFile);
        Assert.DoesNotContain(result, f => f.FullName == nestedIgnoredFile);
    }

    [Fact]
    public void GetMediaFiles_WithEmptyIgnoredFolders_DoesNotExcludeAnyFiles()
    {
        // Arrange
        const string normalFile = @"C:\TvShows\episode.mp4";
        const string extrasFile = @"C:\TvShows\Extras\extra.mkv";

        _mockFileSystem.AddFile(normalFile, new MockFileData(""));
        _mockFileSystem.AddFile(extrasFile, new MockFileData(""));
        
        _settings.IncludeSubdirectories = true;
        _settings.IgnoredFolders = new List<string>();

        var sut = new MediaFileProvider(_mockFileSystem,
                                        Options.Create(_settings),
                                        MediaType.TvShow);

        // Act
        var result = sut.GetMediaFiles().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.FullName == normalFile);
        Assert.Contains(result, f => f.FullName == extrasFile);
    }
}