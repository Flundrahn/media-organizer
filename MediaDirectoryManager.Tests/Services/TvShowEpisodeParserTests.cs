using MediaOrganizer.Services;

namespace MediaDirectoryManager.Tests.Services;

public class TvShowEpisodeParserTests
{
    [Theory]
    [InlineData("Alien.Earth.S01E07.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Alien Earth", 1, 7, "")]
    [InlineData("Gen.V.S02E01.REPACK.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Gen V", 2, 1, "")]
    [InlineData("foundation.s03e08.1080p.web.h264-successfulcrab[EZTVx.to].mkv", "Foundation", 3, 8, "")]
    [InlineData("Foundation 2021 S03E09 The Paths That Choose Us 1080p ATVP WEB-DL DD 5 1 Atmos H 264-playWEB[EZTVx.to].mkv", "Foundation", 3, 9, "The Paths That Choose Us")]
    public void Parse_WithStandardSxxExxFormat_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode, string expectedTitle)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var result = parser.Parse(filename);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(expectedShow, result.ShowName);
        Assert.Equal(expectedSeason, result.Season);
        Assert.Equal(expectedEpisode, result.Episode);
        Assert.Equal(expectedTitle, result.Title);
    }

    [Theory]
    [InlineData("The Office 1x01 Pilot.mkv", "The Office", 1, 1, "Pilot")]
    [InlineData("Breaking Bad 2x13 ABQ.mp4", "Breaking Bad", 2, 13, "ABQ")]
    [InlineData("Friends 10x18 The Last One.avi", "Friends", 10, 18, "The Last One")]
    public void Parse_WithSeasonXEpisodeFormat_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode, string expectedTitle)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var result = parser.Parse(filename);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(expectedShow, result.ShowName);
        Assert.Equal(expectedSeason, result.Season);
        Assert.Equal(expectedEpisode, result.Episode);
        Assert.Equal(expectedTitle, result.Title);
    }

    [Theory]
    [InlineData("Peacemaker.2022.S02E05.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Peacemaker", 2, 5)]
    [InlineData("Its.Always.Sunny.In.Philadelphia.S17E07.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Its Always Sunny In Philadelphia", 17, 7)]
    [InlineData("Gen.V.S02E02.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Gen V", 2, 2)]
    [InlineData("Gen.V.S02E03.1080p.WEB.h264-ETHEL[EZTVx.to].mkv", "Gen V", 2, 3)]
    public void Parse_WithNoEpisodeTitle_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var result = parser.Parse(filename);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(expectedShow, result.ShowName);
        Assert.Equal(expectedSeason, result.Season);
        Assert.Equal(expectedEpisode, result.Episode);
        Assert.Equal("", result.Title);
    }

    [Theory]
    [InlineData("The Office (2005) S01E01.mkv", "The Office", 1, 1, 2005)]
    [InlineData("Doctor Who (2005) S01E01 Rose.mp4", "Doctor Who", 1, 1, 2005)]
    public void Parse_WithYearInParentheses_ShouldReturnCorrectTvShowEpisode(string filename, string expectedShow, int expectedSeason, int expectedEpisode, int expectedYear)
    {
        // Arrange
        var parser = new TvShowEpisodeParser();

        // Act
        var result = parser.Parse(filename);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(expectedShow, result.ShowName);
        Assert.Equal(expectedSeason, result.Season);
        Assert.Equal(expectedEpisode, result.Episode);
        Assert.Equal(expectedYear, result.Year);
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
        var result = parser.Parse(filename);

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
}