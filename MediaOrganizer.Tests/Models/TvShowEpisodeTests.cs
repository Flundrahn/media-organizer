using MediaOrganizer.Models;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Models;

public class TvShowEpisodeTests
{
    [Fact]
    public void Constructor_WithFileInfo_SetsOriginalFileProperty()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");

        // Act
        var episode = new TvShowEpisode(fileInfo);

        // Assert
        Assert.Equal(fileInfo, episode.OriginalFile);
    }

    [Fact]
    public void Constructor_WithFileInfo_SetsCurrentFilePropertyToSameValue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\Breaking.Bad.S02E13.mkv");

        // Act
        var episode = new TvShowEpisode(fileInfo);

        // Assert
        Assert.Equal(fileInfo, episode.CurrentFile);
        Assert.Same(episode.OriginalFile, episode.CurrentFile);
    }

    [Fact]
    public void IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        // Act & Assert
        Assert.True(episode.IsValid);
    }

    [Fact]
    public void IsValid_WithEmptyTvShowName_ReturnsFalse()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "";
        episode.Season = 1;
        episode.Episode = 1;

        // Act & Assert
        Assert.False(episode.IsValid);
    }

    [Fact]
    public void ToString_WithAllProperties_ReturnsFormattedString()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\test.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;
        episode.Title = "Pilot";
        episode.Year = 2005;

        // Act
        var result = episode.ToString();

        // Assert
        Assert.Equal("The Office S01E01 - Pilot (2005)", result);
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2} - {Title}.mkv", 
                "The Office/Season 1/The Office - S01E01 - Pilot.mkv")]
    [InlineData("{TvShowName}/S{Season:D2}/{TvShowName} S{Season:D2}E{Episode:D2}.mkv", 
                "The Office/S01/The Office S01E01.mkv")]
    [InlineData("TV Shows/{TvShowName} ({Year})/Season {Season}/{Episode:D2} - {Title}.mkv", 
                "TV Shows/The Office (2005)/Season 1/01 - Pilot.mkv")]
    [InlineData("{TvShowName}/{TvShowName} - {Season}x{Episode:D2}.mkv", 
                "The Office/The Office - 1x01.mkv")]
    public void GenerateRelativePath_WithValidPattern_ReturnsFormattedPath(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.Pilot.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;
        episode.Title = "Pilot";
        episode.Year = 2005;

        // Act
        var result = episode.GenerateRelativePath(pattern);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}.mkv", 
                "Breaking Bad/Season 2/Breaking Bad - S02E13.mkv")]
    [InlineData("Series/{TvShowName}/S{Season:D2}E{Episode:D2} - {TvShowName}.mkv", 
                "Series/Breaking Bad/S02E13 - Breaking Bad.mkv")]
    public void GenerateRelativePath_WithoutTitle_HandlesEmptyTitle(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\Breaking.Bad.S02E13.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "Breaking Bad";
        episode.Season = 2;
        episode.Episode = 13;
        episode.Title = "";

        // Act
        var result = episode.GenerateRelativePath(pattern);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}.avi", 
                "Game Of Thrones/Season 1/Game Of Thrones - S01E01.avi")]
    [InlineData("{TvShowName} ({Year})/S{Season:D2}/{Episode:D2}.avi", 
                "Game Of Thrones/S01/01.avi")]
    public void GenerateRelativePath_WithoutYear_HandlesNullYear(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\Game.Of.Thrones.S01E01.avi");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "Game Of Thrones";
        episode.Season = 1;
        episode.Episode = 1;
        episode.Title = "";
        episode.Year = null;

        // Act
        var result = episode.GenerateRelativePath(pattern);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void GenerateRelativePath_WithInvalidEpisode_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\invalid.mkv");
        var episode = new TvShowEpisode(fileInfo);
        // Leave properties at default values (invalid state)

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            episode.GenerateRelativePath("{TvShowName}/Season {Season}/{Episode:D2}.mkv"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateRelativePath_WithEmptyOrWhitespacePattern_ThrowsArgumentException(string pattern)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => episode.GenerateRelativePath(pattern));
    }

    [Fact]
    public void GenerateRelativePath_WithNullPattern_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => episode.GenerateRelativePath(null!));
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}.mkv", "The Office/Season 1/The Office.mkv")]
    [InlineData("{TvShowName}", "The Office")]
    [InlineData("Static/Path/File.mkv", "Static/Path/File.mkv")]
    public void GenerateRelativePath_WithPartialPatterns_ReturnsCorrectPath(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");
        var episode = new TvShowEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        // Act
        var result = episode.GenerateRelativePath(pattern);

        // Assert
        Assert.Equal(expectedPath, result);
    }
}