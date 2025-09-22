using System.IO.Abstractions;
using MediaDirectoryManager.Validations;

namespace MediaDirectoryManager.IntegrationTests.Validations;

/// <summary>
/// Integration tests for FileSystemValidations using real file system I/O
/// Focus: Critical real-world scenarios that mocks cannot adequately test
/// </summary>
public class FileSystemValidationsIntegrationTests : IDisposable
{
    private readonly FileSystemValidations _sut;
    private readonly string _testDirectory;

    public FileSystemValidationsIntegrationTests()
    {
        var fileSystem = new FileSystem();
        _sut = new FileSystemValidations(fileSystem);
        
        _testDirectory = Directory.CreateTempSubdirectory("MediaDirectoryManagerTests_").FullName;
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void DirectoryIsWriteable_WhenDirectoryDoesNotExist_LeavesNoTestFiles()
    {
        // Arrange
        var newDirPath = Path.Combine(_testDirectory, "NewDirectory");

        // Act
        var result = _sut.DirectoryIsWriteable(newDirPath);

        // Assert
        Assert.True(result);
        Assert.False(Directory.Exists(newDirPath)); // Should be cleaned up
    }

    [Fact]
    public void DirectoryIsWriteable_WithExistingDirectory_LeavesNoTestFiles()
    {
        // Arrange
        var existingDir = Path.Combine(_testDirectory, "ExistingDir");
        Directory.CreateDirectory(existingDir);

        // Act
        var result = _sut.DirectoryIsWriteable(existingDir);

        // Assert
        Assert.True(result);
        Assert.Empty(Directory.GetFiles(existingDir)); // No leftover test files
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in integration test that for now only needs run manually
        }
    }
}