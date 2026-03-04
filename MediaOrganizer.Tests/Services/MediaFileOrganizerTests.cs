using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Configuration;
using MediaOrganizer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Tests.Services;

public class MediaFileOrganizerTests
{
    private const string VideoFileContent = "fake video content";
    private readonly MockFileSystem _mockFileSystem;
    private readonly MediaOrganizerSettings _settings;

    public MediaFileOrganizerTests()
    {
        _mockFileSystem = new MockFileSystem();

        string tvShowSourceDirectory = @"C:\TvSource";
        string tvShowDestinationDirectory = @"C:\TvDestination";

        // Setup test directories
        _mockFileSystem.AddDirectory(tvShowSourceDirectory);
        _mockFileSystem.AddDirectory(tvShowDestinationDirectory);
        _settings = new MediaOrganizerSettings
        {
            // Does not bother to set movie directories, the tested logic of base class should be identical
            TvShowSourceDirectory = tvShowSourceDirectory,
            TvShowDestinationDirectory = tvShowDestinationDirectory,
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
        var options = Options.Create(_settings);

        return new MediaFileOrganizer(
            _mockFileSystem,
            NullLogger<MediaFileOrganizer>.Instance,
            new TvEpisodeParser(options),
            options,
            fileInfos);
    }

    [Fact]
    public void OrganizeFile_WithValidTvShowFile_MovesAndRenamesCorrectly()
    {
        // Arrange
        var sourceFilePath = $"{_settings.TvShowSourceDirectory}\\The.Office.S01E01.Pilot.mkv";
        var expectedDestinationPath = $"{_settings.TvShowDestinationDirectory}\\The Office\\Season 1\\The Office - S01E01.mkv";

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
        var sourceFilePath = $"{_settings.TvShowSourceDirectory}\\invalid-file-name.mkv";
        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.Null(result);
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "Unparsable file should remain in source");

        var destinationFiles = _mockFileSystem.Directory.GetFiles(_settings.TvShowDestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }

    [Fact]
    public void OrganizeFile_InDryRunMode_ReturnsTrueAndDoesNotMoveFile()
    {
        // Arrange
        _settings.DryRun = true;
        var sourceFilePath = $"{_settings.TvShowSourceDirectory}\\The.Office.S01E01.Pilot.mkv";
        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid in dry run mode");
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "In dry run mode, file should not be moved");

        var destinationFiles = _mockFileSystem.Directory.GetFiles(_settings.TvShowDestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }

    [Fact]
    public void OrganizeFile_WithValidTvShowFile_UpdatesCurrentFileInfoAfterMove()
    {
        // Arrange
        var sourceFilePath = $"{_settings.TvShowSourceDirectory}\\Breaking.Bad.S01E01.720p.HDTV.x264-CTU.mkv";
        var sut = CreateOrganizerWithFiles(sourceFilePath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");
        Assert.Equal(sourceFilePath, result.OriginalFilePath);
        Assert.NotEqual(result.OriginalFilePath, result.CurrentFilePath);
    }

    [Fact]
    public void OrganizeFile_WithFileAlreadyOrganized_SkipsAndReturnsSuccess()
    {
        // Arrange
        _settings.TvShowDestinationDirectory = _settings.TvShowSourceDirectory;
        var correctDestinationPath = $"{_settings.TvShowSourceDirectory}\\The Office\\Season 1\\The Office - S01E01.mkv";
        var sut = CreateOrganizerWithFiles(correctDestinationPath);

        // Act
        var result = sut.OrganizeFile();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");

        var allFiles = _mockFileSystem.Directory.GetFiles(_settings.TvShowDestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Single(allFiles);
        Assert.Equal(correctDestinationPath, allFiles[0]);
    }
}
