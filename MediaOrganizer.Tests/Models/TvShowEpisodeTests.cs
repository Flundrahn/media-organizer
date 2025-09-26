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
}