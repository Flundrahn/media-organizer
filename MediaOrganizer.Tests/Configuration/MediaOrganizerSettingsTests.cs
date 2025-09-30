using MediaOrganizer.Configuration;
using MediaOrganizer.Validations;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Configuration;

public class MediaOrganizerSettingsTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly FileSystemValidator _validator;
    private readonly MediaOrganizerSettings _sut;

    public MediaOrganizerSettingsTests()
    {
        _mockFileSystem = new MockFileSystem();
        _validator = new FileSystemValidator(_mockFileSystem);
        _sut = new MediaOrganizerSettings();
        _sut.SetValidator(_validator);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var settings = new MediaOrganizerSettings();

        // Assert
        Assert.Equal(string.Empty, settings.TvShowSourceDirectory);
        Assert.Equal(string.Empty, settings.TvShowDestinationDirectory);
        Assert.Equal(string.Empty, settings.MovieSourceDirectory);
        Assert.Equal(string.Empty, settings.MovieDestinationDirectory);
        Assert.True(settings.DryRun);
        Assert.True(settings.IncludeSubdirectories);
        Assert.Equal(string.Empty, settings.TvShowPathTemplate);
        Assert.Equal(string.Empty, settings.MoviePathTemplate);
        Assert.Empty(settings.VideoFileExtensions);
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
        var tvSourceDir = @"C:\TvSource";
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";
        _sut.MoviePathTemplate = "{Title} ({Year})";
        _sut.VideoFileExtensions = new List<string> { ".mp4", ".avi", ".mkv" };

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.True(result);
        Assert.Empty(_sut.GetValidationErrors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptyTvShowSourceDirectory_ReturnsFalseAndAddsError(string tvShowSourceDirectory)
    {
        // Arrange
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvShowSourceDirectory;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";
        _sut.MoviePathTemplate = "{Title} ({Year})";

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowSourceDirectory is required", _sut.GetValidationErrors());
    }

    [Fact]
    public void IsValid_WithNonExistentTvShowSourceDirectory_ReturnsFalseAndAddsErrorWithNonExistentDirectoryPath()
    {
        // Arrange
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = @"C:\NonExistent";
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";
        _sut.MoviePathTemplate = "{Title} ({Year})";

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowSourceDirectory does not exist: C:\\NonExistent", _sut.GetValidationErrors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptyTvShowDestinationDirectory_ReturnsFalseAndAddsError(string tvShowDestinationDirectory)
    {
        // Arrange
        var tvSourceDir = @"C:\TvSource";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvShowDestinationDirectory;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";
        _sut.MoviePathTemplate = "{Title} ({Year})";

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowDestinationDirectory is required", _sut.GetValidationErrors());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WithEmptyTvShowPathTemplate_ReturnsFalseAndAddsError(string pattern)
    {
        // Arrange
        var tvSourceDir = @"C:\TvSource";
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = pattern;
        _sut.MoviePathTemplate = "{Title} ({Year})";

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowPathTemplate is required", _sut.GetValidationErrors());
    }

    [Fact]
    public void IsValid_WithMultipleErrors_ReturnsFalseAndAddsAllErrors()
    {
        // Arrange
        _sut.TvShowSourceDirectory = "";
        _sut.TvShowDestinationDirectory = "";
        _sut.MovieSourceDirectory = "";
        _sut.MovieDestinationDirectory = "";
        _sut.TvShowPathTemplate = "";
        _sut.MoviePathTemplate = "";
        _sut.VideoFileExtensions = new List<string>(); // Empty list

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        var errors = _sut.GetValidationErrors();
        Assert.Contains("TvShowSourceDirectory is required", errors);
        Assert.Contains("TvShowDestinationDirectory is required", errors);
        Assert.Contains("MovieSourceDirectory is required", errors);
        Assert.Contains("MovieDestinationDirectory is required", errors);
        Assert.Contains("TvShowPathTemplate is required", errors);
        Assert.Contains("MoviePathTemplate is required", errors);
        Assert.Contains("VideoFileExtensions must contain at least one extension", errors);
        Assert.Equal(7, errors.Count);
    }

    [Fact]
    public void IsValid_ClearsErrorsOnEachCall()
    {
        // Arrange - setup with missing TV show source directory only
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = ""; // This will cause the single error
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = "{TvShowName}";
        _sut.MoviePathTemplate = "{Title} ({Year})";
        _sut.VideoFileExtensions = new List<string> { ".mp4" };

        // Act - First call with error
        var firstResult = _sut.IsValid();
        Assert.False(firstResult);
        Assert.Single(_sut.GetValidationErrors());

        // Fix the error
        var tvSourceDir = @"C:\TvSource";
        _mockFileSystem.AddDirectory(tvSourceDir);
        _sut.TvShowSourceDirectory = tvSourceDir;

        // Act - Second call should clear previous errors
        var secondResult = _sut.IsValid();

        // Assert
        Assert.True(secondResult);
        Assert.Empty(_sut.GetValidationErrors());
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
        var tvSourceDir = @"C:\TvSource";
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = pattern;
        _sut.MoviePathTemplate = "{Title} ({Year})";
        _sut.VideoFileExtensions = new List<string> { ".mp4", ".avi", ".mkv" };

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.True(result);
        Assert.Equal(pattern, _sut.TvShowPathTemplate);
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
        settings.TvShowSourceDirectory = sourceDir;
        settings.TvShowDestinationDirectory = destDir;
        settings.MovieSourceDirectory = sourceDir;
        settings.MovieDestinationDirectory = destDir;
        settings.TvShowPathTemplate = "{TvShowName}";
        settings.MoviePathTemplate = "{Title} ({Year})";
        settings.VideoFileExtensions = new List<string> { ".mp4" };

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
        
        _sut.SourceDirectory = sourceDir;
        _sut.DestinationDirectory = destDir;
        _sut.TvShowPathTemplate = invalidTemplate;

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        Assert.Contains("TvShowPathTemplate contains invalid path characters", _sut.GetValidationErrors());
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
        var tvSourceDir = @"C:\TvSource";
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = validTemplate;
        _sut.MoviePathTemplate = "{Title} ({Year})";
        _sut.VideoFileExtensions = new List<string> { ".mp4", ".avi", ".mkv" };

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.True(result);
        Assert.Empty(_sut.GetValidationErrors());
    }

    [Theory]
    [InlineData(@"{TvShowName}\Season {Season}\{TvShowName}")]  // Windows-style paths with verbatim string
    [InlineData("{TvShowName}/Season {Season}/{TvShowName}")]   // Unix-style paths
    [InlineData("{TvShowName}\\\\Season {Season}\\\\{TvShowName}")] // Double-escaped backslashes
    public void IsValid_WithDifferentPathSeparators_ReturnsTrue(string template)
    {
        // Arrange
        var tvSourceDir = @"C:\TvSource";
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = template;
        _sut.MoviePathTemplate = "{Title} ({Year})";
        _sut.VideoFileExtensions = new List<string> { ".mp4", ".avi", ".mkv" };

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.True(result);
        Assert.Empty(_sut.GetValidationErrors());
    }

    [Fact]
    public void IsValid_WithMultipleErrorsIncludingInvalidTemplate_ReturnsAllErrors()
    {
        // Arrange
        _sut.TvShowSourceDirectory = "";
        _sut.TvShowDestinationDirectory = "";
        _sut.MovieSourceDirectory = "";
        _sut.MovieDestinationDirectory = "";
        _sut.TvShowPathTemplate = "{TvShowName}/<>Invalid";  // Invalid characters
        _sut.MoviePathTemplate = "";
        // VideoFileExtensions is empty by default, which will add another error

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.False(result);
        var errors = _sut.GetValidationErrors();
        Assert.Contains("TvShowSourceDirectory is required", errors);
        Assert.Contains("TvShowDestinationDirectory is required", errors);
        Assert.Contains("MovieSourceDirectory is required", errors);
        Assert.Contains("MovieDestinationDirectory is required", errors);
        Assert.Contains("TvShowPathTemplate contains invalid path characters", errors);
        Assert.Contains("MoviePathTemplate is required", errors);
        Assert.Contains("VideoFileExtensions must contain at least one extension", errors);
        Assert.Equal(7, errors.Count);
    }

    [Fact]
    public void SourceDirectory_WithRelativePath_ConvertsToAbsolutePath()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();
        var relativePath = "Source";

        // Act
        settings.SourceDirectory = relativePath;

        // Assert
        Assert.True(Path.IsPathFullyQualified(settings.SourceDirectory), "SourceDirectory should be converted to absolute path");
        Assert.EndsWith("Source", settings.SourceDirectory);
        Assert.NotEqual(relativePath, settings.SourceDirectory);
    }

    [Fact]
    public void DestinationDirectory_WithRelativePath_ConvertsToAbsolutePath()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();
        var relativePath = "Destination";

        // Act
        settings.DestinationDirectory = relativePath;

        // Assert
        Assert.True(Path.IsPathFullyQualified(settings.DestinationDirectory), "DestinationDirectory should be converted to absolute path");
        Assert.EndsWith("Destination", settings.DestinationDirectory);
        Assert.NotEqual(relativePath, settings.DestinationDirectory);
    }

    [Fact]
    public void SourceDirectory_WithAbsolutePath_RemainsUnchanged()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();
        var absolutePath = @"C:\Source";

        // Act
        settings.SourceDirectory = absolutePath;

        // Assert
        Assert.Equal(absolutePath, settings.SourceDirectory);
        Assert.True(Path.IsPathFullyQualified(settings.SourceDirectory));
    }

    [Fact]
    public void DestinationDirectory_WithAbsolutePath_RemainsUnchanged()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();
        var absolutePath = @"C:\Destination";

        // Act
        settings.DestinationDirectory = absolutePath;

        // Assert
        Assert.Equal(absolutePath, settings.DestinationDirectory);
        Assert.True(Path.IsPathFullyQualified(settings.DestinationDirectory));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SourceDirectory_WithNullOrWhiteSpace_RemainsEmpty(string? value)
    {
        // Arrange
        var settings = new MediaOrganizerSettings();

        // Act
        settings.SourceDirectory = value!;

        // Assert
        Assert.Equal(string.Empty, settings.SourceDirectory);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void DestinationDirectory_WithNullOrWhiteSpace_RemainsEmpty(string? value)
    {
        // Arrange
        var settings = new MediaOrganizerSettings();

        // Act
        settings.DestinationDirectory = value!;

        // Assert
        Assert.Equal(string.Empty, settings.DestinationDirectory);
    }

    [Fact]
    public void Directories_WithCurrentDirectoryReference_ExpandsCorrectly()
    {
        // Arrange
        var settings = new MediaOrganizerSettings();
        var currentDir = Environment.CurrentDirectory;

        // Act
        settings.SourceDirectory = ".";
        settings.DestinationDirectory = ".";

        // Assert
        Assert.Equal(currentDir, settings.SourceDirectory);
        Assert.Equal(currentDir, settings.DestinationDirectory);
    }

    [Theory]
    [InlineData(null, false, "VideoFileExtensions must contain at least one extension")] // Null list
    [InlineData(new string[0], false, "VideoFileExtensions must contain at least one extension")] // Empty list
    [InlineData(new string[] { "" }, false, "VideoFileExtensions cannot contain empty or whitespace extensions")] // Empty string extension
    [InlineData(new string[] { "   " }, false, "VideoFileExtensions cannot contain empty or whitespace extensions")] // Whitespace extension
    [InlineData(new string[] { "mp4" }, false, "VideoFileExtensions must start with a dot: 'mp4'")] // Extension without dot
    [InlineData(new string[] { "avi" }, false, "VideoFileExtensions must start with a dot: 'avi'")] // Extension without dot
    [InlineData(new string[] { "." }, false, "VideoFileExtensions must have at least one character after the dot: '.'")] // Dot only extension
    [InlineData(new string[] { ".mp*" }, false, "VideoFileExtensions contains invalid characters: '.mp*'")] // Extension with asterisk
    [InlineData(new string[] { ".avi?" }, false, "VideoFileExtensions contains invalid characters: '.avi?'")] // Extension with question mark
    [InlineData(new string[] { ".mkv<" }, false, "VideoFileExtensions contains invalid characters: '.mkv<'")] // Extension with less than
    [InlineData(new string[] { ".mp4>" }, false, "VideoFileExtensions contains invalid characters: '.mp4>'")] // Extension with greater than
    [InlineData(new string[] { ".avi|" }, false, "VideoFileExtensions contains invalid characters: '.avi|'")] // Extension with pipe
    [InlineData(new string[] { ".mkv:" }, false, "VideoFileExtensions contains invalid characters: '.mkv:'")] // Extension with colon
    [InlineData(new string[] { ".mp4\"" }, false, "VideoFileExtensions contains invalid characters: '.mp4\"'")] // Extension with quote
    [InlineData(new string[] { ".avi\\" }, false, "VideoFileExtensions contains invalid characters: '.avi\\'")] // Extension with backslash
    [InlineData(new string[] { ".mkv/" }, false, "VideoFileExtensions contains invalid characters: '.mkv/'")] // Extension with forward slash
    [InlineData(new string[] { ".mp4" }, true, null)] // Valid mp4 extension
    [InlineData(new string[] { ".avi" }, true, null)] // Valid avi extension
    [InlineData(new string[] { ".mkv" }, true, null)] // Valid mkv extension
    [InlineData(new string[] { ".mov" }, true, null)] // Valid mov extension
    [InlineData(new string[] { ".wmv" }, true, null)] // Valid wmv extension
    [InlineData(new string[] { ".flv" }, true, null)] // Valid flv extension
    [InlineData(new string[] { ".webm" }, true, null)] // Valid webm extension
    [InlineData(new string[] { ".m4v" }, true, null)] // Valid m4v extension
    [InlineData(new string[] { ".3gp" }, true, null)] // Valid 3gp extension
    [InlineData(new string[] { ".MP4" }, true, null)] // Valid uppercase extension
    [InlineData(new string[] { ".mp4v" }, true, null)] // Valid multi-character extension
    [InlineData(new string[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" }, true, null)] // Multiple valid extensions
    public void IsValid_VideoFileExtensionsValidation(string[]? extensions, bool expectedValid, string? expectedErrorSubstring)
    {
        // Arrange
        var tvSourceDir = @"C:\TvSource";
        var tvDestDir = @"C:\TvDestination";
        var movieSourceDir = @"C:\MovieSource";
        var movieDestDir = @"C:\MovieDestination";
        
        _mockFileSystem.AddDirectory(tvSourceDir);
        _mockFileSystem.AddDirectory(tvDestDir);
        _mockFileSystem.AddDirectory(movieSourceDir);
        _mockFileSystem.AddDirectory(movieDestDir);
        
        _sut.TvShowSourceDirectory = tvSourceDir;
        _sut.TvShowDestinationDirectory = tvDestDir;
        _sut.MovieSourceDirectory = movieSourceDir;
        _sut.MovieDestinationDirectory = movieDestDir;
        _sut.TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName}";
        _sut.MoviePathTemplate = "{Title} ({Year})";
        
        if (extensions == null)
        {
            _sut.VideoFileExtensions = null!;
        }
        else if (extensions.Length == 0)
        {
            _sut.VideoFileExtensions = new List<string>();
        }
        else
        {
            _sut.VideoFileExtensions = new List<string>(extensions);
        }

        // Act
        var result = _sut.IsValid();

        // Assert
        Assert.Equal(expectedValid, result);
        
        if (expectedErrorSubstring != null)
        {
            Assert.Contains(expectedErrorSubstring, _sut.GetValidationErrors());
        }
        else
        {
            Assert.Empty(_sut.GetValidationErrors());
        }
    }

    [Theory]
    [InlineData("C:\\Movies\\Source", "C:\\Movies\\Source")]
    [InlineData("..\\Movies", null)] // Relative path should be converted to absolute
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void MovieSourceDirectory_SetValue_ConvertsToAbsolutePath(string input, string? expectedPrefix)
    {
        // Act
        _sut.MovieSourceDirectory = input;

        // Assert
        if (expectedPrefix != null)
        {
            if (expectedPrefix == "")
            {
                Assert.Equal("", _sut.MovieSourceDirectory);
            }
            else
            {
                Assert.Equal(expectedPrefix, _sut.MovieSourceDirectory);
            }
        }
        else
        {
            // For relative paths, just verify it's now absolute
            Assert.True(Path.IsPathRooted(_sut.MovieSourceDirectory));
        }
    }

    [Theory]
    [InlineData("C:\\Movies\\Destination", "C:\\Movies\\Destination")]
    [InlineData("..\\MoviesOut", null)] // Relative path should be converted to absolute
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void MovieDestinationDirectory_SetValue_ConvertsToAbsolutePath(string input, string? expectedPrefix)
    {
        // Act
        _sut.MovieDestinationDirectory = input;

        // Assert
        if (expectedPrefix != null)
        {
            if (expectedPrefix == "")
            {
                Assert.Equal("", _sut.MovieDestinationDirectory);
            }
            else
            {
                Assert.Equal(expectedPrefix, _sut.MovieDestinationDirectory);
            }
        }
        else
        {
            Assert.True(Path.IsPathRooted(_sut.MovieDestinationDirectory));
        }
    }

    [Theory]
    [InlineData("C:\\TvShows\\Source", "C:\\TvShows\\Source")]
    [InlineData("..\\TvShows", null)] // Relative path should be converted to absolute
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void TvShowSourceDirectory_SetValue_ConvertsToAbsolutePath(string input, string? expectedPrefix)
    {
        // Act
        _sut.TvShowSourceDirectory = input;

        // Assert
        if (expectedPrefix != null)
        {
            if (expectedPrefix == "")
            {
                Assert.Equal("", _sut.TvShowSourceDirectory);
            }
            else
            {
                Assert.Equal(expectedPrefix, _sut.TvShowSourceDirectory);
            }
        }
        else
        {
            Assert.True(Path.IsPathRooted(_sut.TvShowSourceDirectory));
        }
    }

    [Theory]
    [InlineData("C:\\TvShows\\Destination", "C:\\TvShows\\Destination")]
    [InlineData("..\\TvShowsOut", null)] // Relative path should be converted to absolute
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void TvShowDestinationDirectory_SetValue_ConvertsToAbsolutePath(string input, string? expectedPrefix)
    {
        // Act
        _sut.TvShowDestinationDirectory = input;

        // Assert
        if (expectedPrefix != null)
        {
            if (expectedPrefix == "")
            {
                Assert.Equal("", _sut.TvShowDestinationDirectory);
            }
            else
            {
                Assert.Equal(expectedPrefix, _sut.TvShowDestinationDirectory);
            }
        }
        else
        {
            Assert.True(Path.IsPathRooted(_sut.TvShowDestinationDirectory));
        }
    }
}