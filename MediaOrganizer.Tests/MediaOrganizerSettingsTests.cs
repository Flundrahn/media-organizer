using MediaOrganizer.Configuration;
using MediaOrganizer.Validations;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests;

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
    public void IsValid_WhenSourceDirectoryIsEmpty_ReturnsFalseAndAddsError()
    {
        // Arrange
        _sut.SourceDirectory = "";
        _sut.DestinationDirectory = @"C:\ValidDestination";

        // Act
        var isValid = _sut.IsValid();

        // Assert
        Assert.False(isValid);
        var errors = _sut.GetValidationErrors();
        Assert.Contains("SourceDirectory is required", errors);
    }

    [Fact]
    public void IsValid_WhenDestinationDirectoryIsEmpty_ReturnsFalseAndAddsError()
    {
        // Arrange
        _sut.SourceDirectory = @"C:\ValidSource";
        _sut.DestinationDirectory = "";

        // Act
        var isValid = _sut.IsValid();

        // Assert
        Assert.False(isValid);
        var errors = _sut.GetValidationErrors();
        Assert.Contains("DestinationDirectory is required", errors);
    }

    [Fact]
    public void IsValid_WhenSourceDirectoryDoesNotExist_ReturnsFalseAndAddsError()
    {
        // Arrange
        _sut.SourceDirectory = @"C:\NonExistentDirectory";
        _sut.DestinationDirectory = @"C:\ValidDestination";

        // Act
        var isValid = _sut.IsValid();

        // Assert
        Assert.False(isValid);
        var errors = _sut.GetValidationErrors();
        Assert.Contains("SourceDirectory does not exist", errors);
    }

    [Fact]
    public void IsValid_WhenBothDirectoriesAreEmpty_ReturnsFalseAndAddsBothErrors()
    {
        // Arrange
        _sut.SourceDirectory = "";
        _sut.DestinationDirectory = "";

        // Act
        var isValid = _sut.IsValid();

        // Assert
        Assert.False(isValid);
        var errors = _sut.GetValidationErrors();
        Assert.Contains("SourceDirectory is required", errors);
        Assert.Contains("DestinationDirectory is required", errors);
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void GetValidationErrors_WhenCalledMultipleTimes_ReturnsSameErrors()
    {
        // Arrange
        _sut.SourceDirectory = "";
        _sut.DestinationDirectory = "";

        // Act
        _sut.IsValid(); // Populate errors
        var errors1 = _sut.GetValidationErrors();
        var errors2 = _sut.GetValidationErrors();

        // Assert
        Assert.Equal(errors1.Count, errors2.Count);
        Assert.All(errors1, error => Assert.Contains(error, errors2));
    }
}