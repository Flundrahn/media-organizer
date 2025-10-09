using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Configuration;
using MediaOrganizer.Models;

namespace MediaOrganizer.Tests.Models;

public class MovieTests
{
    private readonly MockFileSystem _mockFileSystem = new MockFileSystem();

    [Fact]
    public void Constructor_WithFileInfo_SetsOriginalFileProperty()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.1080p.mkv");

        // Act
        var movie = new Movie(fileInfo);

        // Assert
        Assert.Equal(fileInfo, movie.OriginalFile);
    }

    [Fact]
    public void Constructor_WithFileInfo_SetsCurrentFilePropertyToSameValue()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\Inception.2010.4K.mp4");

        // Act
        var movie = new Movie(fileInfo);

        // Assert
        Assert.Equal(fileInfo, movie.CurrentFile);
        Assert.Same(movie.OriginalFile, movie.CurrentFile);
    }

    [Fact]
    public void IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        // Act & Assert
        Assert.True(movie.IsValid);
    }

    [Fact]
    public void IsValid_WithEmptyTitle_ReturnsFalse()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "";
        movie.Year = 1999;

        // Act & Assert
        Assert.False(movie.IsValid);
    }

    [Fact]
    public void IsValid_WithNullYear_ReturnsTrue()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = null;

        // Act & Assert
        Assert.True(movie.IsValid);
    }

    [Fact]
    public void ToString_WithAllProperties_ReturnsFormattedString()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;
        movie.Quality = "1080p";

        // Act
        var result = movie.ToString();

        // Assert
        Assert.Equal("The Matrix (1999) [1080p]", result);
    }

    [Fact]
    public void ToString_WithoutQuality_ReturnsFormattedStringWithoutQuality()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "Inception";
        movie.Year = 2010;
        movie.Quality = "";

        // Act
        var result = movie.ToString();

        // Assert
        Assert.Equal("Inception (2010)", result);
    }

    [Fact]
    public void GenerateRelativePath_WithMoviesPattern_ReturnsFormattedPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.1080p.BluRay.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;
        movie.Quality = "1080p";

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "Movies/{Title} ({Year})"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("Movies/The Matrix (1999).mkv", result);
    }

    [Fact]
    public void GenerateRelativePath_WithTitleYearQualityPattern_ReturnsFormattedPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.1080p.BluRay.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;
        movie.Quality = "1080p";

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "{Title}/{Title} - {Year} [{Quality}]"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("The Matrix/The Matrix - 1999 [1080p].mkv", result);
    }

    [Fact]
    public void GenerateRelativePath_WithFilmsYearPattern_ReturnsFormattedPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.1080p.BluRay.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;
        movie.Quality = "1080p";

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "Films/{Year}/{Title}"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("Films/1999/The Matrix.mkv", result);
    }

    [Fact]
    public void GenerateRelativePath_WithTitleYearQualityInlinePattern_ReturnsFormattedPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.1080p.BluRay.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;
        movie.Quality = "1080p";

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "{Title} ({Year}) {Quality}"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("The Matrix (1999) 1080p.mkv", result);
    }

    [Fact]
    public void GenerateRelativePath_WithoutQuality_HandlesEmptyQuality_MoviesPattern()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\Inception.2010.mp4");
        var movie = new Movie(fileInfo);
        movie.Title = "Inception";
        movie.Year = 2010;
        movie.Quality = "";

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "Movies/{Title} ({Year})"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("Movies/Inception (2010).mp4", result);
    }

    [Fact]
    public void GenerateRelativePath_WithoutQuality_HandlesEmptyQuality_TitleYearPattern()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\Inception.2010.mp4");
        var movie = new Movie(fileInfo);
        movie.Title = "Inception";
        movie.Year = 2010;
        movie.Quality = "";

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "{Title}/{Year}"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("Inception/2010.mp4", result);
    }

    [Fact]
    public void GenerateRelativePath_WithInvalidMovie_ThrowsInvalidOperationException()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\invalid.mkv");
        var movie = new Movie(fileInfo);
        // Leave properties at default values (invalid state)

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "Movies/{Title} ({Year})"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            movie.GenerateRelativePath(settings));
    }

    [Fact]
    public void GenerateRelativePath_WithNullSettings_ThrowsNullReferenceException()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => movie.GenerateRelativePath(null!));
    }

    [Fact]
    public void GenerateRelativePath_WithPartialPatterns_MoviesTitle_ReturnsCorrectPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "Movies/{Title}"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("Movies/The Matrix.mkv", result);
    }

    [Fact]
    public void GenerateRelativePath_WithPartialPatterns_TitleOnly_ReturnsCorrectPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "{Title}"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("The Matrix.mkv", result);
    }

    [Fact]
    public void GenerateRelativePath_WithPartialPatterns_StaticPath_ReturnsCorrectPath()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\The.Matrix.1999.mkv");
        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        var settings = new MediaOrganizerSettings
        {
            MoviePathTemplate = "Static/Path/File"
        };

        // Act
        var result = movie.GenerateRelativePath(settings);

        // Assert
        Assert.Equal("Static/Path/File.mkv", result);
    }

    [Fact]
    public void IsOrganized_WithFileInCorrectLocation_ReturnsTrue()
    {
        // Arrange
        var correctPath = @"C:\destination\Movies\The Matrix (1999).mkv";
        var fileInfo = _mockFileSystem.FileInfo.New(correctPath);

        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        var settings = new MediaOrganizerSettings
        {
            MovieDestinationDirectory = @"C:\destination",
            MoviePathTemplate = "Movies/{Title} ({Year})"
        };

        // Act
        var result = movie.IsOrganized(settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOrganized_WithFileInWrongLocation_ReturnsFalse()
    {
        // Arrange
        var wrongPath = @"C:\source\The.Matrix.1999.mkv";
        var fileInfo = _mockFileSystem.FileInfo.New(wrongPath);

        var movie = new Movie(fileInfo);
        movie.Title = "The Matrix";
        movie.Year = 1999;

        var settings = new MediaOrganizerSettings
        {
            MovieDestinationDirectory = @"C:\destination",
            MoviePathTemplate = "Movies/{Title} ({Year})"
        };

        // Act
        var result = movie.IsOrganized(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOrganized_WithInvalidMovie_ReturnsFalse()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\invalid.mkv");
        var movie = new Movie(fileInfo); // Invalid movie (no properties set)

        var settings = new MediaOrganizerSettings
        {
            MovieDestinationDirectory = @"C:\destination",
            MoviePathTemplate = "Movies/{Title} ({Year})"
        };

        // Act
        var result = movie.IsOrganized(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Type_ReturnsMovie()
    {
        // Arrange
        var fileInfo = _mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var movie = new Movie(fileInfo);

        // Act & Assert
        Assert.Equal(MediaType.Movie, movie.Type);
    }

    [Fact]
    public void ValidPlaceholders_ContainsExpectedPlaceholders()
    {
        // Arrange & Act
        var placeholders = Movie.ValidPlaceholders;

        // Assert
        Assert.Contains("Title", placeholders);
        Assert.Contains("Year", placeholders);
        Assert.Contains("Quality", placeholders);
        Assert.Equal(3, placeholders.Count);
    }
}