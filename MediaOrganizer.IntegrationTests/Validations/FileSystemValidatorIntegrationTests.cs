using System.IO.Abstractions;
using MediaOrganizer.IntegrationTests.TestHelpers;
using MediaOrganizer.Validations;

namespace MediaOrganizer.IntegrationTests.Validations;

/// <summary>
/// Integration tests for FileSystemValidations using real file system I/O
/// Focus: Critical real-world scenarios that mocks cannot adequately test
/// </summary>
public class FileSystemValidatorIntegrationTests
{
    private readonly FileSystemValidator _sut;

    public FileSystemValidatorIntegrationTests()
    {
        var fileSystem = new FileSystem();
        _sut = new FileSystemValidator(fileSystem);
    }

    [Fact]
    public void DirectoryIsWriteable_WhenDirectoryDoesNotExist_LeavesNoTestFiles()
    {
        using var environment = new TempMediaTestEnvironment();
        // Arrange
        var newDirPath = Path.Combine(environment.DestinationDirectory, "NewDirectory");

        // Act
        var result = _sut.DirectoryIsWriteable(newDirPath);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(newDirPath)); // Should be cleaned up
    }

    [Fact]
    public void DirectoryIsWriteable_WithExistingDirectory_LeavesNoTestFiles()
    {
        using var environment = new TempMediaTestEnvironment();
        // Arrange
        var existingDir = Path.Combine(environment.DestinationDirectory, "ExistingDir");
        Directory.CreateDirectory(existingDir);

        // Act
        var result = _sut.DirectoryIsWriteable(existingDir);

        // Assert
        Assert.True(result);
        Assert.Empty(Directory.GetFiles(existingDir)); // No leftover test files
    }
}