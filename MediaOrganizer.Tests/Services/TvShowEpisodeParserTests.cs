using MediaOrganizer.Models;
using MediaOrganizer.Services;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Services;

public class TvShowEpisodeParserTests
{
    [Theory]
    [InlineData("Alien.Earth.S01E07.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Alien Earth", 1, 7, "")]
    [InlineData("Gen.V.S02E01.REPACK.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Gen V", 2, 1, "")]
    [InlineData("foundation.s03e08.1080p.web.h264-successfulcrab[EZTVx.to].mkv", "Foundation", 3, 8, "")]
    [InlineData("Foundation 2021 S03E09 The Paths That Choose Us 1080p ATVP WEB-DL DD 5 1 Atmos H 264-playWEB[EZTVx.to].mkv", "Foundation", 3, 9, "The Paths That Choose Us")]
    public void Parse_WithStandardSxxExxPattern_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode, string expectedTitle)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\source\{filename}");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal(expectedShow, tvShow.TvShowName);
        Assert.Equal(expectedSeason, tvShow.Season);
        Assert.Equal(expectedEpisode, tvShow.Episode);
        Assert.Equal(expectedTitle, tvShow.Title);
    }

    [Theory]
    [InlineData("The Office 1x01 Pilot.mkv", "The Office", 1, 1, "Pilot")]
    [InlineData("Breaking Bad 2x13 ABQ.mp4", "Breaking Bad", 2, 13, "ABQ")]
    [InlineData("Friends 10x18 The Last One.avi", "Friends", 10, 18, "The Last One")]
    public void Parse_WithSeasonXEpisodePattern_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode, string expectedTitle)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\source\{filename}"); 
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal(expectedShow, tvShow.TvShowName);
        Assert.Equal(expectedSeason, tvShow.Season);
        Assert.Equal(expectedEpisode, tvShow.Episode);
        Assert.Equal(expectedTitle, tvShow.Title);
    }

    [Theory]
    [InlineData("Peacemaker.2022.S02E05.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Peacemaker", 2, 5)]
    [InlineData("Its.Always.Sunny.In.Philadelphia.S17E07.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Its Always Sunny in Philadelphia", 17, 7)]
    [InlineData("Gen.V.S02E02.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Gen V", 2, 2)]
    [InlineData("Gen.V.S02E03.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Gen V", 2, 3)]
    public void Parse_WithNoEpisodeTitle_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\source\{filename}"); 
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal(expectedShow, tvShow.TvShowName);
        Assert.Equal(expectedSeason, tvShow.Season);
        Assert.Equal(expectedEpisode, tvShow.Episode);
        Assert.Equal("", tvShow.Title);
    }

    [Theory]
    [InlineData("The Office (2005) S01E01.mkv", "The Office", 1, 1, 2005)]
    [InlineData("Doctor Who (2005) S01E01 Rose.mp4", "Doctor Who", 1, 1, 2005)]
    public void Parse_WithYearInParenthesesPattern_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode, int expectedYear)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\source\{filename}"); 
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal(expectedShow, tvShow.TvShowName);
        Assert.Equal(expectedSeason, tvShow.Season);
        Assert.Equal(expectedEpisode, tvShow.Episode);
        Assert.Equal(expectedYear, tvShow.Year);
    }

    [Theory]
    [InlineData("Some Movie 2023.mkv")]
    [InlineData("Random.File.Name.txt")]
    [InlineData("")]
    [InlineData("No_Pattern_Here.mp4")]
    public void Parse_WithInvalidFormat_ShouldReturnInvalidTvShowEpisode(string filename)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\source\{filename}"); 
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("The.Office.S01E01.Pilot.mkv")]
    [InlineData("Breaking Bad 2x13 ABQ.mp4")]
    [InlineData("Game of Thrones (2011) S01E01.avi")]
    public void CanParse_WithValidTvShowFormat_ShouldReturnTrue(string filename)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var canParse = parser.CanParse(filename);

        // Assert
        Assert.True(canParse);
    }

    [Theory]
    [InlineData("Some Movie 2023.mkv")]
    [InlineData("Random.File.Name.txt")]
    [InlineData("")]
    public void CanParse_WithInvalidFormat_ShouldReturnFalse(string filename)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var canParse = parser.CanParse(filename);

        // Assert
        Assert.False(canParse);
    }

    [Fact]
    public void Parse_WithSpacedSxxExxWithTitlePattern_ShouldParseCorrectly()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var realFilePath = @"C:\Videos\TV Programmes\Its Always Sunny in Philadelphia S17E08 The Golden Bachelor Live 1080p AMZN WEB-DL DDP5 1 H 264-FLUX[EZTVx.to].mkv";

        // Act - Use mock file system for testing, but with the real filename
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(realFilePath);
        var result = parser.Parse(fileInfo);

        // Debug output to understand what's happening
        var filename = fileInfo.Name;
        var canParse = parser.CanParse(filename);
        
        // Output debug information
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        if (!result.IsValid)
        {
            Assert.Fail(
                $"Parser failed to parse: {filename}\n" +
                $"CanParse: {canParse}\n" +
                $"IsValid: {result.IsValid}\n" +
                $"TvShowName: '{tvShow.TvShowName}'\n" +
                $"Season: {tvShow.Season}\n" +
                $"Episode: {tvShow.Episode}\n" +
                $"Title: '{tvShow.Title}'");
        }

        // Assert - Using the SpacedSxxExxWithTitlePattern
        Assert.True(result.IsValid, "Should be able to parse episode with SpacedSxxExxWithTitlePattern");
        Assert.Equal("Its Always Sunny in Philadelphia", tvShow.TvShowName);
        Assert.Equal(17, tvShow.Season);
        Assert.Equal(8, tvShow.Episode);
        Assert.Equal("The Golden Bachelor Live", tvShow.Title);
    }

    [Fact]
    public void CanParse_WithSpacedSxxExxWithTitlePattern_ShouldReturnTrue()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var filename = "Its Always Sunny in Philadelphia S17E08 The Golden Bachelor Live 1080p AMZN WEB-DL DDP5 1 H 264-FLUX[EZTVx.to].mkv";

        // Act
        var canParse = parser.CanParse(filename);

        // Assert - Using the SpacedSxxExxWithTitlePattern
        Assert.True(canParse, "Should be able to identify episode format with SpacedSxxExxWithTitlePattern");
    }

    [Fact]
    public void Parse_WithDashedSxxExxWithTitlePattern_ShouldParseCorrectly()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\source\The Mandalorian - S02E02 - Chapter 10 The Passenger.mkv");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid, "Should be able to parse dashed SxxExx format with title");
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal("The Mandalorian", tvShow.TvShowName);
        Assert.Equal(2, tvShow.Season);
        Assert.Equal(2, tvShow.Episode);
        Assert.Equal("Chapter 10 The Passenger", tvShow.Title);
    }

    [Fact]
    public void CanParse_WithDashedSxxExxWithTitlePattern_ShouldReturnTrue()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var filename = "The Mandalorian - S02E02 - Chapter 10 The Passenger.mkv";

        // Act
        var canParse = parser.CanParse(filename);

        // Assert
        Assert.True(canParse, "Should be able to identify dashed SxxExx format with title");
    }

    [Fact]
    public void Parse_WithSpacedSxxExxWithQualityPattern_ShouldParseCorrectly()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var realFilePath = @"C:\Videos\TV Programmes\The Sandman\Season 2\The Sandman S02E07 1080p.mkv";

        // Act - Use mock file system for testing, but with the real filename
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(realFilePath);
        var result = parser.Parse(fileInfo);

        // Debug output to understand what's happening
        var filename = fileInfo.Name;
        var canParse = parser.CanParse(filename);
        
        // Output debug information
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        if (!result.IsValid)
        {
            Assert.Fail(
                $"Parser failed to parse: {filename}\n" +
                $"CanParse: {canParse}\n" +
                $"IsValid: {result.IsValid}\n" +
                $"TvShowName: '{tvShow.TvShowName}'\n" +
                $"Season: {tvShow.Season}\n" +
                $"Episode: {tvShow.Episode}\n" +
                $"Title: '{tvShow.Title}'");
        }

        // Assert - Expected values for SpacedSxxExxWithQualityPattern
        Assert.True(result.IsValid, "Should be able to parse SpacedSxxExxWithQualityPattern");
        Assert.Equal("The Sandman", tvShow.TvShowName);
        Assert.Equal(2, tvShow.Season);
        Assert.Equal(7, tvShow.Episode);
        Assert.Equal("", tvShow.Title); // No episode title in this format
    }

    [Fact]
    public void CanParse_WithSpacedSxxExxWithQualityPattern_ShouldReturnTrue()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var filename = "The Sandman S02E07 1080p.mkv";

        // Act
        var canParse = parser.CanParse(filename);

        // Assert
        Assert.True(canParse, "Should be able to identify SpacedSxxExxWithQualityPattern format");
    }

    [Fact]
    public void CanParse_WithDashedSxxExxPattern_ShouldReturnTrue()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var filename = "Breaking Bad - S01E01.mkv";

        // Act
        var canParse = parser.CanParse(filename);

        // Assert
        Assert.True(canParse, "Should be able to identify DashedSxxExxPattern format");
    }

    [Fact]
    public void Parse_WithDashedSxxExxPattern_ShouldParseCorrectly()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\Videos\TV Programmes\Breaking Bad\Season 1\Breaking Bad - S01E01.mkv");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid, "Should be able to parse DashedSxxExxPattern");
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal("Breaking Bad", tvShow.TvShowName);
        Assert.Equal(1, tvShow.Season);
        Assert.Equal(1, tvShow.Episode);
        Assert.Equal("", tvShow.Title); // No episode title in this format
    }

    [Fact]
    public void Parse_WithRepeatedSxxExxPattern_ShouldParseCorrectly()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var testFile = mockFileSystem.FileInfo.New(@"C:\Test\The Lord of the Rings The Rings of Power - S01E01 S01E01 - A Shadow of the Past.avi");
        var parser = new TvShowEpisodeParser();

        // Act
        var result = parser.Parse(testFile);

        // Assert
        Assert.True(result.IsValid, "Should be able to parse repeated SxxExx pattern");
        var tvShow = Assert.IsType<TvShowEpisode>(result);
        Assert.Equal("The Lord of the Rings the Rings of Power", tvShow.TvShowName);
        Assert.Equal(1, tvShow.Season);
        Assert.Equal(1, tvShow.Episode);
        Assert.Equal("A Shadow of the Past", tvShow.Title);
    }

    [Fact]
    public void CanParse_WithRepeatedSxxExxPattern_ShouldReturnTrue()
    {
        // Arrange
        var parser = new TvShowEpisodeParser();
        var filename = "The Lord of the Rings The Rings of Power - S01E01 S01E01 - A Shadow of the Past.avi";

        // Act
        var canParse = parser.CanParse(filename);

        // Assert
        Assert.True(canParse, "Should be able to identify repeated SxxExx pattern format");
    }
}
