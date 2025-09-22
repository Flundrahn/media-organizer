using MediaDirectoryManager.Validations;
using System.IO.Abstractions.TestingHelpers;

namespace MediaDirectoryManager.Tests.Validations;

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
}