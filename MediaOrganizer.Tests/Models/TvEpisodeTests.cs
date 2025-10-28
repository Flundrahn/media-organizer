using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Configuration;
using MediaOrganizer.Models;

namespace MediaOrganizer.Tests.Models;

public class TvEpisodeTests
{
    [Fact]
    public void Constructor_WithFileInfo_SetsOriginalFileProperty()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");

        // Act
        var episode = new TvEpisode(fileInfo);

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
        var episode = new TvEpisode(fileInfo);

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
        var episode = new TvEpisode(fileInfo);
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
        var episode = new TvEpisode(fileInfo);
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
        var episode = new TvEpisode(fileInfo);
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
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2} - {Title}", 
                "The Office/Season 1/The Office - S01E01 - Pilot.mkv")]
    [InlineData("{TvShowName}/S{Season:D2}/{TvShowName} S{Season:D2}E{Episode:D2}", 
                "The Office/S01/The Office S01E01.mkv")]
    [InlineData("TV Shows/{TvShowName} ({Year})/Season {Season}/{Episode:D2} - {Title}", 
                "TV Shows/The Office (2005)/Season 1/01 - Pilot.mkv")]
    [InlineData("{TvShowName}/{TvShowName} - {Season}x{Episode:D2}", 
                "The Office/The Office - 1x01.mkv")]
    public void GenerateRelativePath_WithValidPattern_ReturnsFormattedPath(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.Pilot.mkv");
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;
        episode.Title = "Pilot";
        episode.Year = 2005;

        var settings = new MediaOrganizerSettings
        {
            TvShowPathTemplate = pattern
        };

        // Act
        var result = episode.GenerateRelativePath(settings);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}", 
                "Breaking Bad/Season 2/Breaking Bad - S02E13.mkv")]
    [InlineData("Series/{TvShowName}/S{Season:D2}E{Episode:D2} - {TvShowName}", 
                "Series/Breaking Bad/S02E13 - Breaking Bad.mkv")]
    public void GenerateRelativePath_WithoutTitle_HandlesEmptyTitle(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\Breaking.Bad.S02E13.mkv");
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "Breaking Bad";
        episode.Season = 2;
        episode.Episode = 13;
        episode.Title = "";

        var settings = new MediaOrganizerSettings
        {
            TvShowPathTemplate = pattern
        };

        // Act
        var result = episode.GenerateRelativePath(settings);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}", 
                "Game Of Thrones/Season 1/Game Of Thrones - S01E01.avi")]
    [InlineData("{TvShowName} ({Year})/S{Season:D2}/{Episode:D2}", 
                "Game Of Thrones/S01/01.avi")]
    public void GenerateRelativePath_WithoutYear_HandlesNullYear(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\Game.Of.Thrones.S01E01.avi");
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "Game Of Thrones";
        episode.Season = 1;
        episode.Episode = 1;
        episode.Title = "";
        episode.Year = null;

        var settings = new MediaOrganizerSettings
        {
            TvShowPathTemplate = pattern
        };

        // Act
        var result = episode.GenerateRelativePath(settings);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void GenerateRelativePath_WithInvalidEpisode_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\invalid.mkv");
        var episode = new TvEpisode(fileInfo);
        // Leave properties at default values (invalid state)

        var settings = new MediaOrganizerSettings
        {
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{Episode:D2}"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            episode.GenerateRelativePath(settings));
    }

    [Fact]
    public void GenerateRelativePath_WithNullSettings_ThrowsNullReferenceException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => episode.GenerateRelativePath(null!));
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}", "The Office/Season 1/The Office.mkv")]
    [InlineData("{TvShowName}", "The Office.mkv")]
    [InlineData("Static/Path/File", "Static/Path/File.mkv")]
    public void GenerateRelativePath_WithPartialPatterns_ReturnsCorrectPath(string pattern, string expectedPath)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The.Office.S01E01.mkv");
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        var settings = new MediaOrganizerSettings
        {
            TvShowPathTemplate = pattern
        };

        // Act
        var result = episode.GenerateRelativePath(settings);

        // Assert
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void IsOrganized_WithFileInCorrectLocation_ReturnsTrue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var correctPath = @"C:\destination\The Office\Season 1\The Office - S01E01.mkv";
        var fileInfo = mockFileSystem.FileInfo.New(correctPath);
        
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        var settings = new MediaOrganizerSettings
        {
            TvShowDestinationDirectory = @"C:\destination",
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}"
        };

        // Act
        var result = episode.IsOrganized(settings);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOrganized_WithFileInWrongLocation_ReturnsFalse()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var wrongPath = @"C:\source\The.Office.S01E01.mkv";
        var fileInfo = mockFileSystem.FileInfo.New(wrongPath);
        
        var episode = new TvEpisode(fileInfo);
        episode.TvShowName = "The Office";
        episode.Season = 1;
        episode.Episode = 1;

        var settings = new MediaOrganizerSettings
        {
            TvShowDestinationDirectory = @"C:\destination",
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}"
        };

        // Act
        var result = episode.IsOrganized(settings);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOrganized_WithInvalidEpisode_ReturnsFalse()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\invalid.mkv");
        var episode = new TvEpisode(fileInfo); // Invalid episode (no properties set)

        var settings = new MediaOrganizerSettings
        {
            TvShowDestinationDirectory = @"C:\destination",
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}"
        };

        // Act
        var result = episode.IsOrganized(settings);

        // Assert
        Assert.False(result);
    }
}