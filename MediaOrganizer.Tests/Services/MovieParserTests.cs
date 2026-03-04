using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Models;
using MediaOrganizer.Services;

namespace MediaOrganizer.Tests.Services;

public class MovieParserTests
{
    private readonly MovieParser _sut = new();

    [Theory]
    [InlineData("The Matrix 1999 1080p BluRay.mkv", true)]
    [InlineData("Inception (2010) (1080p BluRay).mkv", true)]
    [InlineData("Interstellar.2014.1080p.BluRay.x264.YIFY.mp4", true)]
    [InlineData("Tolkien 1080p.mp4", true)]
    [InlineData("Thor Ragnarok 1080p.mkv", true)]
    [InlineData("Solo A Star Wars Story 2160p.mkv", true)]
    [InlineData("Bram.Stokers.Dracula.1992.RM4k.1080p.BluRay.x265.hevc.10bit.AAC.7.1.commentary-HeVK.mkv", true)]
    [InlineData("Thunderbolts.2025.Proper.1080p.WEB-DL.DDP5.1.x265-NeoNoir.mkv", true)]
    [InlineData("Breaking Bad S01E01.mkv", false)] // TV show format should not be parsed as movie
    [InlineData("Game.of.Thrones.S08E06.mkv", false)] // TV show format should not be parsed as movie
    public void CanParse_ShouldReturnExpectedResult(string filename, bool expected)
    {
        // Act
        var result = _sut.CanParse(filename);

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
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = _sut.Parse(fileInfo);

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
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = _sut.Parse(fileInfo);

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
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = _sut.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Equal(expectedYear, movie.Year);
        Assert.Contains(expectedQuality, movie.Quality);
    }


    [Theory]
    [InlineData("Tolkien 1080p.mp4", "Tolkien", "1080p")]
    [InlineData("Thor Ragnarok 1080p.mkv", "Thor Ragnarok", "1080p")]
    [InlineData("The Princess Bride 1080p.mp4", "The Princess Bride", "1080p")]
    [InlineData("The Batman 1080p.mkv", "The Batman", "1080p")]
    [InlineData("Interstellar 1080p.mkv", "Interstellar", "1080p")]
    [InlineData("Soul 1080p.mkv", "Soul", "1080p")]
    [InlineData("Free Guy 1080p.mp4", "Free Guy", "1080p")]
    [InlineData("Moneyball 1080p.mkv", "Moneyball", "1080p")]
    public void Parse_SimpleMovieWithQuality_ShouldReturnCorrectMovie(string filename, string expectedTitle, string expectedQuality)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = _sut.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Contains(expectedQuality, movie.Quality);
        Assert.Null(movie.Year);
    }

    [Theory]
    [InlineData("Solo A Star Wars Story 2160p.mkv", "Solo A Star Wars Story", "2160p")]
    [InlineData("Shang Chi And The Legend Of The Ten Rings 1080p.mp4", "Shang Chi and the Legend of the Ten Rings", "1080p")]
    [InlineData("Dungeons and Dragons Honor Among Thieves 2160p.mkv", "Dungeons and Dragons Honor Among Thieves", "2160p")]
    [InlineData("Sorry To Bother You 1080p.mp4", "Sorry to Bother You", "1080p")]
    [InlineData("This Is Where I Leave You 1080p.mp4", "This Is Where I Leave You", "1080p")]
    public void Parse_LongTitleMovieWithQuality_ShouldReturnCorrectMovie(string filename, string expectedTitle, string expectedQuality)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = _sut.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Contains(expectedQuality, movie.Quality);
        Assert.Null(movie.Year);
    }

    [Theory]
    // [InlineData("Bram.Stokers.Dracula.1992.RM4k.1080p.BluRay.x265.hevc.10bit.AAC.7.1.commentary-HeVK.mkv", "Bram Stokers Dracula", 1992, "1080p")]
    [InlineData("Thunderbolts.2025.Proper.1080p.WEB-DL.DDP5.1.x265-NeoNoir.mkv", "Thunderbolts", 2025, "1080p")]
    public void Parse_ComplexDotsReleaseFormat_ShouldReturnCorrectMovie(string filename, string expectedTitle, int expectedYear, string expectedQuality)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New($@"C:\movies\{filename}");

        // Act
        var result = _sut.Parse(fileInfo);

        // Assert
        Assert.True(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(expectedTitle, movie.Title);
        Assert.Equal(expectedYear, movie.Year);
        Assert.Equal(expectedQuality, movie.Quality);
    }

    [Fact]
    public void Parse_WithUnparseableFilename_ShouldReturnInvalidMovie()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var fileInfo = mockFileSystem.FileInfo.New(@"C:\movies\unparseable-filename.mkv");

        // Act
        var result = _sut.Parse(fileInfo);

        // Assert
        Assert.False(result.IsValid);
        var movie = Assert.IsType<Movie>(result);
        Assert.Equal(string.Empty, movie.Title);
        Assert.Null(movie.Year);
        Assert.Equal(string.Empty, movie.Quality);
        Assert.Equal(fileInfo.FullName, movie.CurrentFilePath);
        Assert.Equal(fileInfo.FullName, movie.OriginalFilePath);
    }
}
