using MediaOrganizer.Configuration;
using MediaOrganizer.Validations;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Configuration;

public class MediaOrganizerSettingsTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly FileSystemValidator _validator;
    private readonly MediaOrganizerSettings _settings;

    public MediaOrganizerSettingsTests()
    {
        _mockFileSystem = new MockFileSystem();
        _validator = new FileSystemValidator(_mockFileSystem);
        _settings = new MediaOrganizerSettings();
        _settings.SetValidator(_validator);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var settings = new MediaOrganizerSettings();

        // Assert
        Assert.Equal(string.Empty, settings.SourceDirectory);
        Assert.Equal(string.Empty, settings.DestinationDirectory);
        Assert.True(settings.DryRun);
        Assert.True(settings.IncludeSubdirectories);
        Assert.Equal("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}", settings.TvShowPathTemplate);
    }

    [Fact]
    public void SectionName_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("MediaOrganizer", MediaOrganizerSettings.SectionName);
    }

    [Fact]
    public void IsValid_WithoutValidator_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => settings.IsValid());
    }

    [Fact]
    public void IsValid_WithValidSettings_ReturnsTrue()
    {
        // Arrange
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        
        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.True(result);
        Assert.Empty(_settings.GetValidationErrors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptySourceDirectory_ReturnsFalseAndAddsError(string sourceDirectory)
    {
        // Arrange
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDirectory;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("SourceDirectory is required", _settings.GetValidationErrors());
    }

    [Fact]
    public void IsValid_WithNonExistentSourceDirectory_ReturnsFalseAndAddsError()
    {
        // Arrange
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = @"C:\NonExistent";
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("SourceDirectory does not exist", _settings.GetValidationErrors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptyDestinationDirectory_ReturnsFalseAndAddsError(string destinationDirectory)
    {
        // Arrange
        var sourceDir = @"C:\Source";
        _mockFileSystem.AddDirectory(sourceDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destinationDirectory;
        _settings.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("DestinationDirectory is required", _settings.GetValidationErrors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptyTvShowPathTemplate_ReturnsFalseAndAddsError(string pattern)
    {
        // Arrange
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = pattern;

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowPathTemplate is required", _settings.GetValidationErrors());
    }

    [Fact]
    public void IsValid_WithMultipleErrors_ReturnsFalseAndAddsAllErrors()
    {
        // Arrange
        _settings.SourceDirectory = "";
        _settings.DestinationDirectory = "";
        _settings.TvShowPathTemplate = "";

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        var errors = _settings.GetValidationErrors();
        Assert.Contains("SourceDirectory is required", errors);
        Assert.Contains("DestinationDirectory is required", errors);
        Assert.Contains("TvShowPathTemplate is required", errors);
        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public void IsValid_ClearsErrorsOnEachCall()
    {
        // Arrange
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = "";
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = "{TvShowName}";

        // Act - First call with error
        var firstResult = _settings.IsValid();
        Assert.False(firstResult);
        Assert.Single(_settings.GetValidationErrors());

        // Fix the error
        var sourceDir = @"C:\Source";
        _mockFileSystem.AddDirectory(sourceDir);
        _settings.SourceDirectory = sourceDir;

        // Act - Second call should clear previous errors
        var secondResult = _settings.IsValid();

        // Assert
        Assert.True(secondResult);
        Assert.Empty(_settings.GetValidationErrors());
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}")]
    [InlineData("{TvShowName}/S{Season:D2}E{Episode:D2}")]
    [InlineData("TV Shows/{TvShowName} ({Year})/Season {Season}/{Episode:D2} - {Title}")]
    [InlineData("{TvShowName}/{TvShowName} - {Season}x{Episode:D2}")]
    [InlineData("Simple/Static/Path")]
    public void TvShowPathTemplate_AcceptsValidPatterns(string pattern)
    {
        // Arrange
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = pattern;

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.True(result);
        Assert.Equal(pattern, _settings.TvShowPathTemplate);
    }

    [Fact]
    public void SetValidator_SetsValidatorProperty()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();
        var mockFileSystem = new MockFileSystem();
        var validator = new FileSystemValidator(mockFileSystem);
        
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        mockFileSystem.AddDirectory(sourceDir);
        mockFileSystem.AddDirectory(destDir);

        // Act
        settings.SetValidator(validator);
        settings.SourceDirectory = sourceDir;
        settings.DestinationDirectory = destDir;
        settings.TvShowPathTemplate = "{TvShowName}";

        // Should not throw since validator is set
        var result = settings.IsValid();

        // Assert - Method completes without throwing and returns true for valid settings
        Assert.True(result);
    }

    [Theory]
    [InlineData("{TvShowName}/<>/Season {Season}")]  // Invalid filename characters: < >
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}|Episode")]  // Invalid filename character: |
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}*")]  // Invalid filename character: *
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}?")]  // Invalid filename character: ?
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}\"Episode\"")]  // Invalid filename character: "
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}:Episode")]  // Invalid filename character: :
    public void IsValid_WithInvalidCharactersInTemplate_ReturnsFalseAndAddsError(string invalidTemplate)
    {
        // Arrange
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = invalidTemplate;

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowPathTemplate contains invalid path characters", _settings.GetValidationErrors());
    }

    [Theory]
    [InlineData("{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}")]
    [InlineData("{TvShowName}/S{Season:D2}/{TvShowName} S{Season:D2}E{Episode:D2}")]
    [InlineData("TV Shows/{TvShowName} ({Year})/Season {Season}/{Episode:D2} - {Title}")]
    [InlineData("{TvShowName}/{TvShowName} - {Season}x{Episode:D2}")]
    [InlineData("Media/{TvShowName}/Episodes/{Episode:D3}")]
    [InlineData("Shows/{TvShowName}_S{Season:D2}E{Episode:D2}")]
    [InlineData("Series/{TvShowName} [{Year}]/Season{Season}/{TvShowName}")]
    public void IsValid_WithValidTemplateCharacters_ReturnsTrue(string validTemplate)
    {
        // Arrange
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = validTemplate;

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.True(result);
        Assert.Empty(_settings.GetValidationErrors());
    }

    [Theory]
    [InlineData(@"{TvShowName}\Season {Season}\{TvShowName}")]  // Windows-style paths with verbatim string
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}")]   // Unix-style paths
    [InlineData("{TvShowName}\\\\Season {Season}\\\\{TvShowName}")] // Double-escaped backslashes
    public void IsValid_WithDifferentPathSeparators_ReturnsTrue(string template)
    {
        // Arrange
        var sourceDir = @"C:\Source";
        var destDir = @"C:\Destination";
        _mockFileSystem.AddDirectory(sourceDir);
        _mockFileSystem.AddDirectory(destDir);
        
        _settings.SourceDirectory = sourceDir;
        _settings.DestinationDirectory = destDir;
        _settings.TvShowPathTemplate = template;

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.True(result);
        Assert.Empty(_settings.GetValidationErrors());
    }

    [Fact]
    public void IsValid_WithMultipleErrorsIncludingInvalidTemplate_ReturnsAllErrors()
    {
        // Arrange
        _settings.SourceDirectory = "";
        _settings.DestinationDirectory = "";
        _settings.TvShowPathTemplate = "{TvShowName}/<>Invalid";  // Invalid characters

        // Act
        var result = _settings.IsValid();

        // Assert
        Assert.False(result);
        var errors = _settings.GetValidationErrors();
        Assert.Contains("SourceDirectory is required", errors);
        Assert.Contains("DestinationDirectory is required", errors);
        Assert.Contains("TvShowPathTemplate contains invalid path characters", errors);
        Assert.Equal(3, errors.Count);
    }
}