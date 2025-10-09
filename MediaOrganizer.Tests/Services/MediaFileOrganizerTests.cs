using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Configuration;
using MediaOrganizer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace MediaOrganizer.Tests.Services;

public class MediaFileOrganizerTests
{
    private const string TvShowSourceDirectory = @"C:\TvSource";
    private const string TvShowDestinationDirectory = @"C:\TvDestination";
    private const string VideoFileContent = "fake video content";

    private readonly MockFileSystem _mockFileSystem;
    private readonly MediaOrganizerSettings _settings;
    private readonly Mock<IDirectoryCleaner> _mockDirectoryCleaner;

    public MediaFileOrganizerTests()
    {
        _mockFileSystem = new MockFileSystem();
        _mockDirectoryCleaner = new Mock<IDirectoryCleaner>();

        // Setup test directories
        _mockFileSystem.AddDirectory(TvShowSourceDirectory);
        _mockFileSystem.AddDirectory(TvShowDestinationDirectory);

        _settings = new MediaOrganizerSettings
        {
            // Does not bother to set movie directories, the tested logic of base class should be identical
            TvShowSourceDirectory = TvShowSourceDirectory,
            TvShowDestinationDirectory = TvShowDestinationDirectory,
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}",
            MoviePathTemplate = "{Title} ({Year})",
            DryRun = false
        };
    }

    private MediaFileOrganizer CreateOrganizerWithFiles(params string[] sourceFilesToAddFullPaths)
    {
        foreach (var path in sourceFilesToAddFullPaths)
        {
            _mockFileSystem.AddFile(path, new MockFileData(VideoFileContent));
        }

        var fileInfos = sourceFilesToAddFullPaths.Select(path => _mockFileSystem.FileInfo.New(path));

        return new MediaFileOrganizer(
            _mockFileSystem,
            NullLogger<MediaFileOrganizer>.Instance,
            new TvShowEpisodeParser(),
            Options.Create(_settings),
            fileInfos);
    }

    [Fact]
    public void OrganizeFile_WithValidTvShowFile_MovesAndRenamesCorrectly()
    {
        // Arrange
        var sourceFilePath = Path.Combine(TvShowSourceDirectory, "The.Office.S01E01.Pilot.mkv");
        var expectedDestinationPath = Path.Combine(TvShowDestinationDirectory, "The Office", "Season 1", "The Office - S01E01.mkv");

        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");
        Assert.False(_mockFileSystem.File.Exists(sourceFilePath), "Source file should be moved");
        Assert.True(_mockFileSystem.File.Exists(expectedDestinationPath), $"File should exist at destination: {expectedDestinationPath}");
        Assert.Equal(VideoFileContent, _mockFileSystem.File.ReadAllText(expectedDestinationPath));
    }

    [Fact]
    public void OrganizeFile_WithUnparsableFile_ReturnsFalseAndDoesNotMoveFile()
    {
        // Arrange
        var sourceFilePath = Path.Combine(TvShowSourceDirectory, "invalid-file-name.mkv");
        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.Null(result);
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "Unparsable file should remain in source");

        var destinationFiles = _mockFileSystem.Directory.GetFiles(TvShowDestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }

    [Fact]
    public void OrganizeFile_InDryRunMode_ReturnsTrueAndDoesNotMoveFile()
    {
        // Arrange
        _settings.DryRun = true;
        var sourceFilePath = Path.Combine(TvShowSourceDirectory, "The.Office.S01E01.Pilot.mkv");
        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid in dry run mode");
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "In dry run mode, file should not be moved");

        var destinationFiles = _mockFileSystem.Directory.GetFiles(TvShowDestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }

    [Fact]
    public void OrganizeFile_WithValidTvShowFile_UpdatesCurrentFileInfoAfterMove()
    {
        // Arrange
        var sourceFilePath = Path.Combine(TvShowSourceDirectory, "Breaking.Bad.S01E01.720p.HDTV.x264-CTU.mkv");
        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert - verify the operation succeeded and returned a valid episode
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");

        // Assert - verify the original and current file paths are tracked correctly
        Assert.Equal(sourceFilePath, result.OriginalFile.FullName);
        Assert.NotEqual(result.OriginalFile.FullName, result.CurrentFile.FullName);
    }

    [Fact]
    public void OrganizeFile_WithFileAlreadyOrganized_SkipsAndReturnsSuccess()
    {
        // Arrange - use a parseable filename in the location where it should be organized to
        // The parser can parse "The.Office.S01E01.mkv" and it should organize to "The Office/Season 1/The Office - S01E01.mkv"
        var correctDestinationPath = Path.Combine(TvShowDestinationDirectory, "The Office", "Season 1", "The Office - S01E01.mkv");
        var sut = CreateOrganizerWithFiles(correctDestinationPath);

        // Act
        var result = sut.OrganizeFile();

        // Assert - verify the operation succeeded without moving the file
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");

        var allFiles = _mockFileSystem.Directory.GetFiles(TvShowDestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Single(allFiles);
        Assert.Equal(correctDestinationPath, allFiles[0]);
    }
}