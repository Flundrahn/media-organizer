using MediaOrganizer.Models;
using MediaOrganizer.Services;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Services;

public class MovieParserTests
{
    [Theory]
    [InlineData("The Matrix 1999 1080p BluRay.mkv", true)]
    [InlineData("Inception (2010) (1080p BluRay).mkv", true)]
    [InlineData("Interstellar.2014.1080p.BluRay.x264.YIFY.mp4", true)]
    [InlineData("Breaking Bad S01E01.mkv", false)] // TV show format should not be parsed as movie
    [InlineData("Game.of.Thrones.S08E06.mkv", false)] // TV show format should not be parsed as movie
    [InlineData("Sample.mkv", false)]
    public void CanParse_ShouldReturnExpectedResult(string filename, bool expected)
    {
        // Arrange
        var parser = new MovieParser();

        // Act
        var result = parser.CanParse(filename);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Grandmas Boy 2006 UNRATED 1080p BluRay HEVC x265 5.1 BONE.mkv", "Grandmas Boy", 2006, "1080p")]
    [InlineData("Superman 2025 1080p WEB-DL HEVC x265 5.1 BONE.mkv", "Superman", 2025, "1080p")]
    [InlineData("Red One 2024 1080p WEB-DL HEVC x265 5.1 BONE.mkv", "Red One", 2024, "1080p")]
    [InlineData("Subservience 2024 1080p WEB-DL HEVC x265 5.1 BONE.mkv", "Subservience", 2024, "1080p")]
    [InlineData("The Wild Robot 2024 1080p WEBRip x264 AAC5.1-[YTS.MX].mp4", "The Wild Robot", 2024, "1080p")]
    [InlineData("Speak No Evil 2024 1080p WEBRip x264 AAC5.1-[YTS.MX].mp4", "Speak No Evil", 2024, "1080p")]
    public void Parse_WithYearAndQuality_ShouldReturnCorrectMovie(string filename, string expectedTitle, int expectedYear, string expectedQuality)
    {
        // Arrange
        var parser = new MovieParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Equal(expectedYear, movie.Year);
        Assert.Contains(expectedQuality, movie.Quality);
    }

    [Theory]
    [InlineData("A Brilliant Young Mind (2014) (1080p BluRay x265 10bit Tigole).mkv", "A Brilliant Young Mind", 2014, "1080p")]
    [InlineData("Super 8 (2011) (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole).mkv", "Super 8", 2011, "1080p")]
    [InlineData("The Simpsons Movie (2007) (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole).mkv", "The Simpsons Movie", 2007, "1080p")]
    [InlineData("Young Sherlock Holmes (1985) (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole).mkv", "Young Sherlock Holmes", 1985, "1080p")]
    [InlineData("Cowboys & Aliens (2011) (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole).mkv", "Cowboys & Aliens", 2011, "1080p")]
    [InlineData("Free Guy (2021) (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole).mkv", "Free Guy", 2021, "1080p")]
    public void Parse_WithParenthesesFormat_ShouldReturnCorrectMovie(string filename, string expectedTitle, int expectedYear, string expectedQuality)
    {
        // Arrange
        var parser = new MovieParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Equal(expectedYear, movie.Year);
        Assert.Contains(expectedQuality, movie.Quality);
    }

    [Theory]
    [InlineData("Clash.Of.The.Titans.1981.1080p.BluRay.x264-[YTS.AM].mp4", "Clash of the Titans", 1981, "1080p")]
    [InlineData("Home.Alone.3.1997.1080p.WEBRip.x264.AAC-[YTS.MX].mp4", "Home Alone 3", 1997, "1080p")]
    [InlineData("Teenage.Mutant.Ninja.Turtles.2014.1080p.BluRay.x264.YIFY.mp4", "Teenage Mutant Ninja Turtles", 2014, "1080p")]
    [InlineData("Interstellar.2014.1080p.BluRay.x264.YIFY.mp4", "Interstellar", 2014, "1080p")]
    public void Parse_WithDotsFormat_ShouldReturnCorrectMovie(string filename, string expectedTitle, int expectedYear, string expectedQuality)
    {
        // Arrange
        var parser = new MovieParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Equal(expectedYear, movie.Year);
        Assert.Contains(expectedQuality, movie.Quality);
    }

    [Theory]
    [InlineData("Sample.mkv")]
    [InlineData("Making-of-documentary.mp4")]
    [InlineData("Behind-the-scenes-featurette.mkv")]
    [InlineData("Director-commentary-track.mp4")]
    public void Parse_WithExcludedContent_ShouldReturnInvalidResult(string filename)
    {
        // Arrange
        var parser = new MovieParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Parse_WithUnparseableFilename_ShouldReturnInvalidMovie()
    {
        // Arrange
        var parser = new MovieParser();
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\movies\unparseable-filename.mkv");

        // Act
        var result = parser.Parse(fileInfo);

        // Assert
        Assert.False(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal("unparseable-filename.mkv", movie.Title);
        Assert.Null(movie.Year);
        Assert.Equal(string.Empty, movie.Quality);
    }
}