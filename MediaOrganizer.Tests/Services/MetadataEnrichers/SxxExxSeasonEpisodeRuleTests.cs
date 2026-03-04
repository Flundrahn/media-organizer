using MediaOrganizer.Services.MetadataEnrichers;

namespace MediaOrganizer.Tests.Services.MetadataEnrichers;

public class SxxExxSeasonEpisodeRuleTests
{
    [Theory]
    [InlineData(@"Breaking.Bad.S02E13.mkv", 2, 13)] // Dot-separated
    [InlineData(@"Breaking_Bad_S02E13.mkv", 2, 13)] // Underscore-separated 
    [InlineData(@"Breaking Bad S02E13.mkv", 2, 13)] // Space-separated 
    [InlineData(@"Breaking-Bad-S02E13.mkv", 2, 13)] // Dash-separated 
    [InlineData(@"Breaking-Bad - S02E13 - Some episode title.mkv", 2, 13)] // With episode title
    [InlineData(@"Breaking Bad\Episode Folder S02E13\Some episode title.mkv", 2, 13)] // SxxExx in folder path
    [InlineData(@"S02E13\Some episode title.mkv", 2, 13)] // SxxExx as folder name
    [InlineData(@"Breaking Bad\S02E13.mkv", 2, 13)] // Path separator before pattern
    [InlineData(@"/Breaking.Bad/S02E13/", 2, 13)] // Pattern between path separators
    [InlineData(@"Breaking.Bad/S02E13.mkv", 2, 13)] // Unix-style path separators
    [InlineData(@"Breaking.Bad.s1e5.720p.mkv", 1, 5)] // Single-digit season and episode
    [InlineData(@"S02E13", 2, 13)] // Pattern only, no extension
    [InlineData(@"Breaking‐Bad‐S02E13.mkv", 2, 13)] // U+2010 Hyphen
    [InlineData(@"Breaking‑Bad‑S02E13.mkv", 2, 13)] // U+2011 Non-breaking hyphen
    [InlineData(@"Breaking‒Bad‒S02E13.mkv", 2, 13)] // U+2012 Figure dash
    [InlineData(@"Breaking–Bad–S02E13.mkv", 2, 13)] // U+2013 En dash
    [InlineData(@"Breaking—Bad—S02E13.mkv", 2, 13)] // U+2014 Em dash
    [InlineData(@"Breaking―Bad―S02E13.mkv", 2, 13)] // U+2015 Horizontal bar
    [InlineData(@"Breaking−Bad−S02E13.mkv", 2, 13)] // U+2212 Minus sign
    public void TryExtract_IfFilePathContainsSxxExx_ShouldReturnValidRuleMatch(string filePath, int expectedSeason, int expectedEpisode)
    {
        // Arrange
        var sut = new SxxExxSeasonEpisodeRule();

        // Act
        var result = sut.TryExtract(filePath, out RuleMatch match);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedSeason, match.SeasonNumber); 
        Assert.Equal(expectedEpisode, match.EpisodeNumber); 
    }

    [Theory]
    // Some of these cases maybe should be handled and data extracted
    // leave here for now to document current behavior and will update if encounter cases in production data
    [InlineData(@"Breaking.Bad\NEWS02E13.mkv")] // Pattern embedded in word (prefix)
    [InlineData(@"Breaking.Bad\S02E13XYZ.mkv")] // Pattern embedded in word (suffix)
    [InlineData(@"Breaking.Bad\SEASONS02EPISODE13.mkv")] // Pattern embedded in different format
    [InlineData(@"Breaking.Bad\Season02Episode13.mkv")] // Full words without SxxExx pattern
    [InlineData(@"Breaking.Bad\episode.mkv")] // No pattern at all
    [InlineData(@"Breaking.Bad\S2.mkv")] // Incomplete pattern (missing episode)
    [InlineData(@"Breaking.Bad\E13.mkv")] // Incomplete pattern (missing season)
    [InlineData(@"")] // Empty string
    [InlineData(@"Breaking.Bad\S123E45.mkv")] // Too many digits in season
    [InlineData(@"Breaking.Bad\S12E456.mkv")] // Too many digits in episode
    public void TryExtract_IfFilePathDoesNotContainValidSxxExx_ShouldReturnFalse(string filePath)
    {
        // Arrange
        var sut = new SxxExxSeasonEpisodeRule();

        // Act
        var result = sut.TryExtract(filePath, out RuleMatch match);

        // Assert
        Assert.False(result);
        Assert.Null(match.SeasonNumber);
        Assert.Null(match.EpisodeNumber);
    }

    [Theory]
    [InlineData(@"Breaking.Bad\Show.S02E13.S03E14.mkv", 2, 13, 0.5f)]
    [InlineData(@"Breaking.Bad\S01E01\S01E01.mkv", 1, 1, 1.0f)]
    public void TryExtract_WithMultipleMatches_ShouldSplitConfidenceEqually(string filePath,
                                                                            int expectedSeason,
                                                                            int expectedEpisode,
                                                                            float expectedConfidence)
    {
        // Arrange
        var sut = new SxxExxSeasonEpisodeRule();

        // Act
        var result = sut.TryExtract(filePath, out RuleMatch match);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedSeason, match.SeasonNumber);
        Assert.Equal(expectedEpisode, match.EpisodeNumber);
        Assert.Equal(expectedConfidence, match.Confidence);
    }

    [Fact]
    public void TryExtract_ShouldBeCaseInsensitive()
    {
        // Arrange
        var sut = new SxxExxSeasonEpisodeRule();

        // Act & Assert - all should match
        Assert.True(sut.TryExtract("show.s02e13.mkv", out var match1));
        Assert.Equal(2, match1.SeasonNumber);
        Assert.Equal(13, match1.EpisodeNumber);

        Assert.True(sut.TryExtract("show.S02E13.mkv", out var match2));
        Assert.Equal(2, match2.SeasonNumber);
        Assert.Equal(13, match2.EpisodeNumber);

        Assert.True(sut.TryExtract("show.S02e13.mkv", out var match3));
        Assert.Equal(2, match3.SeasonNumber);
        Assert.Equal(13, match3.EpisodeNumber);
    }

    [Fact]
    public void TryExtract_ShouldSetRuleName()
    {
        // Arrange
        var sut = new SxxExxSeasonEpisodeRule();

        // Act
        sut.TryExtract("show.S02E13.mkv", out var match);

        // Assert
        Assert.Equal(nameof(SxxExxSeasonEpisodeRule), match.RuleName);
    }
}
