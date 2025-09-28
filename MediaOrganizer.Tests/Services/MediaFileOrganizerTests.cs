using System.IO.Abstractions.TestingHelpers;
using MediaOrganizer.Configuration;
using MediaOrganizer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

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
        var mockDirectoryCleaner = new Mock<IDirectoryCleaner>();
        
        _sut = new MediaFileOrganizer(
            _mockFileSystem,
            logger,
            parser,
            optionsWrapper,
            mockDirectoryCleaner.Object);
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
        _sut.Initialize([fileInfo]);
        var result = _sut.OrganizeFile();

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
        var sourceFilePath = Path.Combine(SourceDirectory, "invalid-file-name.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        _sut.Initialize([fileInfo]);
        var result = _sut.OrganizeFile();

        // Assert
        Assert.Null(result);
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
        _sut.Initialize([fileInfo]);
        var result = _sut.OrganizeFile();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid in dry run mode");
        Assert.True(_mockFileSystem.File.Exists(sourceFilePath), "In dry run mode, file should not be moved");
        
        var destinationFiles = _mockFileSystem.Directory.GetFiles(DestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Empty(destinationFiles);
    }

    [Fact]
    public void OrganizeFile_WithValidTvShowFile_UpdatesCurrentFileInfoAfterMove()
    {
        // Arrange
        var sourceFilePath = Path.Combine(SourceDirectory, "Breaking.Bad.S01E01.720p.HDTV.x264-CTU.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);
        var expectedDestinationPath = Path.Combine(DestinationDirectory, "Breaking Bad", "Season 1", "Breaking Bad - S01E01.mkv");

        // Act
        _sut.Initialize([fileInfo]);
        var result = _sut.OrganizeFile();

        // Assert - verify the operation succeeded and returned a valid episode
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");
        
        // Assert - verify file was actually moved
        Assert.False(_mockFileSystem.File.Exists(sourceFilePath), "Source file should be moved");
        Assert.True(_mockFileSystem.File.Exists(expectedDestinationPath), $"File should exist at destination: {expectedDestinationPath}");
        
        // Assert - verify CurrentFile property points to the new location
        Assert.Equal(expectedDestinationPath, result.CurrentFile.FullName);
        Assert.Equal(sourceFilePath, result.OriginalFile.FullName);
        Assert.NotEqual(result.OriginalFile.FullName, result.CurrentFile.FullName);
    }

    [Fact]
    public void OrganizeFile_WithFileAlreadyOrganized_SkipsAndReturnsSuccess()
    {
        // Arrange - file is already in the correct organized location
        var correctDestinationPath = Path.Combine(DestinationDirectory, "Breaking Bad", "Season 1", "Breaking Bad - S01E01.mkv");
        _mockFileSystem.AddFile(correctDestinationPath, new MockFileData(VideoFileContent));
        var fileInfo = _mockFileSystem.FileInfo.New(correctDestinationPath);

        // Act
        _sut.Initialize([fileInfo]);
        var result = _sut.OrganizeFile();

        // Assert - verify the operation succeeded without moving the file
        Assert.NotNull(result);
        Assert.True(result.IsValid, "Returned episode should be valid");
        Assert.True(_mockFileSystem.File.Exists(correctDestinationPath), "File should remain in correct location");
        
        // Assert - verify no additional files were created or moved
        var allFiles = _mockFileSystem.Directory.GetFiles(DestinationDirectory, "*", SearchOption.AllDirectories);
        Assert.Single(allFiles); // Only the original file should exist
        Assert.Equal(correctDestinationPath, allFiles[0]);
    }

    [Fact]
    public void OrganizeFile_WithCleanupEnabled_CallsDirectoryCleaner()
    {
        // Arrange
        var sourceFilePath = Path.Combine(SourceDirectory, "The.Office.S01E01.Pilot.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        
        _settings.AutoCleanupEmptyDirectories = true;

        var mockDirectoryCleaner = new Mock<IDirectoryCleaner>();
        var organizer = new MediaFileOrganizer(
            _mockFileSystem,
            NullLogger<MediaFileOrganizer>.Instance,
            new TvShowEpisodeParser(),
            Options.Create(_settings),
            mockDirectoryCleaner.Object);

        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        organizer.Initialize([fileInfo]);
        organizer.OrganizeFile();

        // Assert
        mockDirectoryCleaner.Verify(x => x.CleanEmptyDirectories(), Times.Once);
    }

    [Fact]
    public void OrganizeFile_WithCleanupDisabled_DoesNotCallDirectoryCleaner()
    {
        // Arrange
        var sourceFilePath = Path.Combine(SourceDirectory, "The.Office.S01E01.Pilot.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        
        _settings.AutoCleanupEmptyDirectories = false;

        var mockDirectoryCleaner = new Mock<IDirectoryCleaner>();
        var organizer = new MediaFileOrganizer(
            _mockFileSystem,
            NullLogger<MediaFileOrganizer>.Instance,
            new TvShowEpisodeParser(),
            Options.Create(_settings),
            mockDirectoryCleaner.Object);

        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        organizer.Initialize([fileInfo]);
        organizer.OrganizeFile();

        // Assert
        mockDirectoryCleaner.Verify(x => x.CleanEmptyDirectories(), Times.Never);
    }

    [Fact]
    public void OrganizeFile_WithCleanupEnabled_CallsDirectoryCleanerRegardlessOfDirectoryState()
    {
        // Arrange
        var sourceFilePath = Path.Combine(SourceDirectory, "The.Office.S01E01.Pilot.mkv");
        _mockFileSystem.AddFile(sourceFilePath, new MockFileData(VideoFileContent));
        
        _settings.AutoCleanupEmptyDirectories = true;

        var mockDirectoryCleaner = new Mock<IDirectoryCleaner>();
        var organizer = new MediaFileOrganizer(
            _mockFileSystem,
            NullLogger<MediaFileOrganizer>.Instance,
            new TvShowEpisodeParser(),
            Options.Create(_settings),
            mockDirectoryCleaner.Object);

        var fileInfo = _mockFileSystem.FileInfo.New(sourceFilePath);

        // Act
        organizer.Initialize([fileInfo]);
        organizer.OrganizeFile();

        // Assert
        mockDirectoryCleaner.Verify(x => x.CleanEmptyDirectories(), Times.Once);
    }
}