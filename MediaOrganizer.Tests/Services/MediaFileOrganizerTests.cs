using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Configuration;
using MediaOrganizer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.Tests.Services;

public class MediaFileOrganizerTests
{
    private const string SourceDirectory = @"C:\Source";
    private const string DestinationDirectory = @"C:\Destination";
    private const string VideoFileContent = "fake video content";
    
    private readonly MockFileSystem _mockFileSystem;
    private readonly MediaOrganizerSettings _settings;
    private readonly MediaFileOrganizer _sut;

    public MediaFileOrganizerTests()
    {
        _mockFileSystem = new MockFileSystem();
        
        // Setup test directories
        _mockFileSystem.AddDirectory(SourceDirectory);
        _mockFileSystem.AddDirectory(DestinationDirectory);
        
        _settings = new MediaOrganizerSettings
        {
            SourceDirectory = SourceDirectory,
            DestinationDirectory = DestinationDirectory,
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}",
            DryRun = false
        };

        var optionsWrapper = Options.Create(_settings);
        var logger = NullLogger<MediaFileOrganizer>.Instance;
        var parser = new TvShowEpisodeParser();
        
        _sut = new MediaFileOrganizer(
            _mockFileSystem,
            logger,
            parser,
            optionsWrapper);
    }

    [Fact]
    public void OrganizeFile_WithValidTvShowFile_MovesAndRenamesCorrectly()
    {
        // Arrange
        var sourceFilePath = Path.Combine(SourceDirectory, "The.Office.S01E01.Pilot.mkv");
        var expectedDestinationPath = Path.Combine(DestinationDirectory, "The Office", "Season 1", "The Office - S01E01.mkv");

        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        var result = _sut.OrganizeFile(fileInfo);

        // Assert
        Assert.True(result, "OrganizeFile should return true for successful operation");
        Assert.False(_mockFileSystem.File.Exists(sourceFilePath), "Source file should be moved");
        Assert.True(_mockFileSystem.File.Exists(expectedDestinationPath), $"File should exist at destination: {expectedDestinationPath}");
        Assert.Equal(VideoFileContent, _mockFileSystem.File.ReadAllText(expectedDestinationPath));
    }

    [Fact]
    public void OrganizeFile_WithUnparsableFile_ReturnsFalseAndDoesNotMoveFile()
    {
        // Arrange
        var sourceFilePath = Path.Combine(SourceDirectory, "invalid-file-name.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        var result = _sut.OrganizeFile(fileInfo);

        // Assert
        Assert.False(result, "OrganizeFile should return false for unparsable file");
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "Unparsable file should remain in source");

        var destinationFiles = _mockFileSystem.Directory.GetFiles(DestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }

    [Fact]
    public void OrganizeFile_InDryRunMode_ReturnsTrueAndDoesNotMoveFile()
    {
        // Arrange
        _settings.DryRun = true;
        var sourceFilePath = Path.Combine(SourceDirectory, "The.Office.S01E01.Pilot.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        var result = _sut.OrganizeFile(fileInfo);

        // Assert
        Assert.True(result, "OrganizeFile should return true even in dry run mode");
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "In dry run mode, file should not be moved");
        
        var destinationFiles = _mockFileSystem.Directory.GetFiles(DestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }
}