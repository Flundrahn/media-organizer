using MediaOrganizer.Validations;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Validations;

public class FileSystemValidatorTests
{
    private MockFileSystem _mockFileSystem;
    private FileSystemValidator _sut;

    public FileSystemValidatorTests()
    {
        _mockFileSystem = new MockFileSystem();
        _sut = new FileSystemValidator(_mockFileSystem);
    }

    [Fact]
    public void DirectoryExists_WhenDirectoryExists_ReturnsTrue()
    {
        // Arrange
        _mockFileSystem.AddDirectory(@"C:\ExistingDirectory");

        // Act
        var result = _sut.DirectoryExists(@"C:\ExistingDirectory");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DirectoryExists_WhenDirectoryDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = _sut.DirectoryExists(@"C:\NonExistentDirectory");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_WhenFileExists_ReturnsTrue()
    {
        // Arrange
        var testFilePath = @"C:\test.txt";
        _mockFileSystem.AddFile(testFilePath, new MockFileData("content"));

        // Act
        var result = _sut.FileExists(testFilePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FileExists_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = _sut.FileExists(@"C:\nonexistent.txt");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasValidPath_WhenPathIsValid_ReturnsTrue()
    {
        // Act
        var result = _sut.HasValidPath(@"C:\ValidPath\Directory");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasValidPath_WhenPathContainsInvalidCharacters_ReturnsFalse()
    {
        // Act
        var result = _sut.HasValidPath(@"C:\Invalid|Path");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasValidPath_WhenPathIsEmpty_ReturnsTrue()
    {
        // Act
        var result = _sut.HasValidPath("");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DirectoryIsWriteable_WhenDirectoryExistsAndWriteable_ReturnsTrue()
    {
        // Arrange
        _mockFileSystem.AddDirectory(@"C:\WritableDirectory");

        // Act
        var result = _sut.DirectoryIsWriteable(@"C:\WritableDirectory");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DirectoryIsWriteable_WhenDirectoryDoesNotExistButCanBeCreated_ReturnsTrue()
    {
        // Act
        var result = _sut.DirectoryIsWriteable(@"C:\NewDirectory");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DirectoryIsWriteable_WithInvalidPath_ReturnsFalse()
    {
        // Act
        var result = _sut.DirectoryIsWriteable(@"C:\Invalid|Path");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DirectoryIsWriteable_WithEmptyPath_ReturnsFalse()
    {
        // Act
        var result = _sut.DirectoryIsWriteable("");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("ValidFolder")]
    [InlineData("Valid Folder")]
    [InlineData("Valid-Folder")]
    [InlineData("Valid_Folder")]
    [InlineData("ValidFolder123")]
    public void IsValidPathSegment_WithValidSegment_ReturnsTrue(string segment)
    {
        // Act
        var result = _sut.IsValidPathSegment(segment);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void IsValidPathSegment_WithNullOrEmpty_ReturnsFalse(string? segment)
    {
        // Act
        #pragma warning disable CS8604 // Possible null reference argument.
        var result = _sut.IsValidPathSegment(segment);
        #pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Invalid<>Folder")]
    [InlineData("Invalid|Folder")]
    [InlineData("Invalid:Folder")]
    [InlineData("Invalid\"Folder")]
    [InlineData("Invalid*Folder")]
    [InlineData("Invalid?Folder")]
    public void IsValidPathSegment_WithInvalidCharacters_ReturnsFalse(string segment)
    {
        // Act
        var result = _sut.IsValidPathSegment(segment);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AreValidPathSegments_WithAllValidSegments_ReturnsTrue()
    {
        // Arrange - All segments valid, no empty entries
        var segments = new[] { "Folder1", "Folder2", "File Name" };

        // Act
        var result = _sut.AreValidPathSegments(segments);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AreValidPathSegments_WithEmptySegment_ReturnsFalse()
    {
        // Arrange - Empty segments should be removed by caller using StringSplitOptions.RemoveEmptyEntries
        var segments = new[] { "Folder1", "", "Folder2" };

        // Act
        var result = _sut.AreValidPathSegments(segments);

        // Assert
        Assert.False(result, "Empty segments should cause validation to fail - caller should use RemoveEmptyEntries");
    }

    [Fact]
    public void AreValidPathSegments_WithInvalidSegmentAmongValid_ReturnsFalse()
    {
        // Arrange
        var segments = new[] { "Folder1", "Invalid<>Folder", "Folder3" };

        // Act
        var result = _sut.AreValidPathSegments(segments);

        // Assert
        Assert.False(result);
    }
}
