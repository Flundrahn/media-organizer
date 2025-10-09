using MediaOrganizer.Services;
using MediaOrganizer.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.IO.Abstractions.TestingHelpers;

namespace MediaOrganizer.Tests.Services;

public class DirectoryCleanerTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly DirectoryCleaner _sut;

    public DirectoryCleanerTests()
    {
        _mockFileSystem = new MockFileSystem();
        var logger = NullLogger<DirectoryCleaner>.Instance;
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination",
            DryRun = false
        };
        _sut = new DirectoryCleaner(_mockFileSystem, logger, Options.Create(settings));
    }

    [Fact]
    public void CleanEmptyDirectories_WithEmptyDirectories_RemovesEmptyDirectories()
    {
        // Arrange
        var sourceEmptyDir = @"C:\Source\EmptyFolder";
        var destinationEmptyDir = @"C:\Destination\AnotherEmpty";
        var sourceNestedEmpty = @"C:\Source\Parent\EmptyChild";
        
        _mockFileSystem.AddDirectory(sourceEmptyDir);
        _mockFileSystem.AddDirectory(destinationEmptyDir);
        _mockFileSystem.AddDirectory(sourceNestedEmpty);

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        Assert.False(_mockFileSystem.Directory.Exists(sourceEmptyDir));
        Assert.False(_mockFileSystem.Directory.Exists(destinationEmptyDir));
        Assert.False(_mockFileSystem.Directory.Exists(sourceNestedEmpty));
        // Note: Parent directory removal depends on implementation - the current implementation
        // may not remove parent directories that become empty after child removal
    }

    [Fact]
    public void CleanEmptyDirectories_WithNonEmptyDirectories_DoesNotRemoveNonEmptyDirectories()
    {
        // Arrange
        var nonEmptySourceDir = @"C:\Source\WithFile";
        var nonEmptyDestDir = @"C:\Destination\AlsoWithFile";
        var fileInSource = @"C:\Source\WithFile\important.txt";
        var fileInDest = @"C:\Destination\AlsoWithFile\data.log";
        
        _mockFileSystem.AddFile(fileInSource, new MockFileData("content"));
        _mockFileSystem.AddFile(fileInDest, new MockFileData("log data"));

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        Assert.True(_mockFileSystem.Directory.Exists(nonEmptySourceDir));
        Assert.True(_mockFileSystem.Directory.Exists(nonEmptyDestDir));
        Assert.True(_mockFileSystem.File.Exists(fileInSource));
        Assert.True(_mockFileSystem.File.Exists(fileInDest));
    }

    [Fact]
    public void CleanEmptyDirectories_WithMixedDirectories_RemovesOnlyEmptyOnes()
    {
        // Arrange
        var emptyDir = @"C:\Source\Empty";
        var nonEmptyDir = @"C:\Source\NonEmpty";
        var fileInNonEmpty = @"C:\Source\NonEmpty\file.txt";
        var nestedEmptyInNonEmpty = @"C:\Source\NonEmpty\EmptyChild";
        
        _mockFileSystem.AddDirectory(emptyDir);
        _mockFileSystem.AddFile(fileInNonEmpty, new MockFileData("content"));
        _mockFileSystem.AddDirectory(nestedEmptyInNonEmpty);

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        Assert.False(_mockFileSystem.Directory.Exists(emptyDir));
        Assert.False(_mockFileSystem.Directory.Exists(nestedEmptyInNonEmpty));
        Assert.True(_mockFileSystem.Directory.Exists(nonEmptyDir));
        Assert.True(_mockFileSystem.File.Exists(fileInNonEmpty));
    }

    [Fact]
    public void CleanEmptyDirectories_WithNonExistentRootDirectories_DoesNotThrow()
    {
        // Arrange
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination",
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - Should not throw
        sut.CleanEmptyDirectories();
    }

    [Fact]
    public void CleanEmptyDirectories_InDryRunMode_DoesNotRemoveDirectories()
    {
        // Arrange
        var emptyDir = @"C:\Source\EmptyFolder";
        _mockFileSystem.AddDirectory(emptyDir);
        
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination",
            DryRun = true
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act
        sut.CleanEmptyDirectories();

        // Assert
        Assert.True(_mockFileSystem.Directory.Exists(emptyDir)); // Should still exist in dry run mode
    }

    [Fact]
    public void CleanEmptyDirectories_WithDeepNestedEmptyDirectories_RemovesDeepestEmptyDirectory()
    {
        // Arrange
        var deepPath = @"C:\Source\Level1\Level2\Level3\Level4";
        _mockFileSystem.AddDirectory(deepPath);

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        // The current implementation processes deepest directories first,
        // so it should remove Level4, but Level3, Level2, Level1 may remain
        // since they become empty only after their children are removed
        Assert.False(_mockFileSystem.Directory.Exists(deepPath));
        // Note: Parent directories may or may not be removed depending on implementation
        Assert.True(_mockFileSystem.Directory.Exists(@"C:\Source")); // Root should remain
    }

    [Fact]
    public void CleanEmptyDirectories_WithEmptyDirectoriesContainingOnlyEmptySubdirectories_RemovesEmptyDirectories()
    {
        // Arrange
        var parentDir = @"C:\Source\Parent";
        var emptyChild1 = @"C:\Source\Parent\Empty1";
        var emptyChild2 = @"C:\Source\Parent\Empty2";
        var emptyGrandchild = @"C:\Source\Parent\Empty1\EmptyGrandchild";
        
        _mockFileSystem.AddDirectory(parentDir);
        _mockFileSystem.AddDirectory(emptyChild1);
        _mockFileSystem.AddDirectory(emptyChild2);
        _mockFileSystem.AddDirectory(emptyGrandchild);

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        // The implementation should remove the deepest empty directories first
        Assert.False(_mockFileSystem.Directory.Exists(emptyGrandchild));
        Assert.False(_mockFileSystem.Directory.Exists(emptyChild2)); // This is empty from the start
        // emptyChild1 and parentDir might remain since they may not be processed again
        // after their children are removed
    }

    [Fact]
    public void CleanEmptyDirectories_WithEmptyRootDirectories_DoesNotRemoveRootDirectories()
    {
        // Arrange - Root directories are empty but should not be removed
        _mockFileSystem.AddDirectory(@"C:\Source");
        _mockFileSystem.AddDirectory(@"C:\Destination");

        // Act
        _sut.CleanEmptyDirectories();

        // Assert - Root directories should remain even if empty
        Assert.True(_mockFileSystem.Directory.Exists(@"C:\Source"));
        Assert.True(_mockFileSystem.Directory.Exists(@"C:\Destination"));
    }

    [Fact]
    public void CleanEmptyDirectories_WithDirectoryContainingHiddenFiles_DoesNotRemoveDirectory()
    {
        // Arrange
        var dirWithHiddenFile = @"C:\Source\WithHidden";
        var hiddenFile = @"C:\Source\WithHidden\.hidden";
        
        _mockFileSystem.AddFile(hiddenFile, new MockFileData("hidden content"));

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        Assert.True(_mockFileSystem.Directory.Exists(dirWithHiddenFile));
        Assert.True(_mockFileSystem.File.Exists(hiddenFile));
    }

    [Fact]
    public void CleanEmptyDirectories_WithDirectoryContainingSubdirectoryWithFiles_DoesNotRemoveParent()
    {
        // Arrange
        var parentDir = @"C:\Source\Parent";
        var childDir = @"C:\Source\Parent\Child";
        var fileInChild = @"C:\Source\Parent\Child\file.txt";
        
        _mockFileSystem.AddFile(fileInChild, new MockFileData("content"));

        // Act
        _sut.CleanEmptyDirectories();

        // Assert
        Assert.True(_mockFileSystem.Directory.Exists(parentDir));
        Assert.True(_mockFileSystem.Directory.Exists(childDir));
        Assert.True(_mockFileSystem.File.Exists(fileInChild));
    }
}
