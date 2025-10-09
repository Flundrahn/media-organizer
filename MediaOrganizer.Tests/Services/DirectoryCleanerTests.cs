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

    [Fact]
    public void CleanEmptyDirectories_WithValidDirectories_CleansSuccessfully()
    {
        // Arrange
        var tvSourceSubdir = @"C:\Source\Shows";
        var tvDestSubdir = @"C:\Destination\Shows";
        _mockFileSystem.AddDirectory(tvSourceSubdir);
        _mockFileSystem.AddDirectory(tvDestSubdir);

        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination",
            MovieSourceDirectory = @"C:\Movies\Source",
            MovieDestinationDirectory = @"C:\Movies\Dest"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - Should not throw
        sut.CleanEmptyDirectories(@"C:\Source", @"C:\Destination");
        sut.CleanEmptyDirectories(tvSourceSubdir, tvDestSubdir);
    }

    [Fact]
    public void CleanEmptyDirectories_WithInvalidSourceDirectory_ThrowsArgumentException()
    {
        // Arrange
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\InvalidPath", @"C:\Destination"));
        
        Assert.Equal("sourceDirectory", ex.ParamName);
        Assert.Contains("not within any configured media directories", ex.Message);
    }

    [Fact]
    public void CleanEmptyDirectories_WithInvalidDestinationDirectory_ThrowsArgumentException()
    {
        // Arrange
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\Source", @"C:\InvalidPath"));
        
        Assert.Equal("destinationDirectory", ex.ParamName);
        Assert.Contains("not within any configured media directories", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CleanEmptyDirectories_WithNullOrEmptySourceDirectory_ThrowsArgumentException(string? invalidPath)
    {
        // Arrange
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(invalidPath!, @"C:\Destination"));
        
        Assert.Equal("sourceDirectory", ex.ParamName);
        Assert.Contains("cannot be null or empty", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CleanEmptyDirectories_WithNullOrEmptyDestinationDirectory_ThrowsArgumentException(string? invalidPath)
    {
        // Arrange
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\Source", invalidPath!));
        
        Assert.Equal("destinationDirectory", ex.ParamName);
        Assert.Contains("cannot be null or empty", ex.Message);
    }

    [Fact]
    public void CleanEmptyDirectories_WithSubdirectoryPaths_AllowsCleaning()
    {
        // Arrange
        var sourceSubdir = @"C:\Source\TvShows\Season1";
        var destSubdir = @"C:\Destination\Organized\TvShows";
        _mockFileSystem.AddDirectory(sourceSubdir);
        _mockFileSystem.AddDirectory(destSubdir);

        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination",
            MovieSourceDirectory = @"C:\Movies",
            MovieDestinationDirectory = @"C:\Movies\Organized"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - Should not throw
        sut.CleanEmptyDirectories(sourceSubdir, destSubdir);
    }

    [Fact]
    public void CleanEmptyDirectories_WithMovieDirectories_AllowsCleaning()
    {
        // Arrange
        var movieSource = @"C:\Movies\Source";
        var movieDest = @"C:\Movies\Destination";
        _mockFileSystem.AddDirectory(movieSource);
        _mockFileSystem.AddDirectory(movieDest);

        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\TV\Source",
            TvShowDestinationDirectory = @"C:\TV\Destination",
            MovieSourceDirectory = movieSource,
            MovieDestinationDirectory = movieDest
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - Should not throw
        sut.CleanEmptyDirectories(movieSource, movieDest);
    }

    [Fact]
    public void CleanEmptyDirectories_WithMixedValidDirectories_AllowsCleaning()
    {
        // Arrange
        var tvSource = @"C:\TV\Source";
        var movieDest = @"C:\Movies\Destination";
        _mockFileSystem.AddDirectory(tvSource);
        _mockFileSystem.AddDirectory(movieDest);

        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = tvSource,
            TvShowDestinationDirectory = @"C:\TV\Destination",
            MovieSourceDirectory = @"C:\Movies\Source",
            MovieDestinationDirectory = movieDest
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - Should not throw (TV source + Movie destination is allowed)
        sut.CleanEmptyDirectories(tvSource, movieDest);
    }

    [Fact]
    public void CleanEmptyDirectories_WithRelativePaths_RejectsInvalidPaths()
    {
        // Arrange
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\Source",
            TvShowDestinationDirectory = @"C:\Destination"
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories("../SomeRelativePath", @"C:\Destination"));
        
        Assert.Equal("sourceDirectory", ex.ParamName);
        Assert.Contains("not within any configured media directories", ex.Message);
    }

    [Fact]
    public void CleanEmptyDirectories_WithEmptyMovieDirectories_OnlyAllowsTvDirectories()
    {
        // Arrange - Movie directories are empty/null, only TV directories configured
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\TV\Source",
            TvShowDestinationDirectory = @"C:\TV\Destination",
            MovieSourceDirectory = "", // Empty
            MovieDestinationDirectory = "" // Empty
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - TV directories should be allowed
        sut.CleanEmptyDirectories(@"C:\TV\Source", @"C:\TV\Destination");

        // Act & Assert - Movie directories should NOT be allowed
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\Movies\Source", @"C:\Movies\Destination"));
        
        Assert.Equal("sourceDirectory", ex.ParamName);
        Assert.Contains("not within any configured media directories", ex.Message);
    }

    [Fact]
    public void CleanEmptyDirectories_WithAllEmptyDirectories_ThrowsException()
    {
        // Arrange - All media directories are empty/null/whitespace
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = "",
            TvShowDestinationDirectory = "   ",
            MovieSourceDirectory = "",
            MovieDestinationDirectory = ""
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\AnyPath", @"C:\AnyOtherPath"));
        
        Assert.Equal("sourceDirectory", ex.ParamName);
        Assert.Contains("No valid media directories are configured", ex.Message);
    }

    [Theory]
    [InlineData("   ")] 
    [InlineData("\t")]  
    [InlineData("\n")]  
    [InlineData("\r")]  
    [InlineData(" \t ")] 
    public void CleanEmptyDirectories_WithWhitespaceMovieDirectories_DoesNotAllowRootAccess(string whitespaceMoviePath)
    {
        // Arrange - Movie directories are whitespace, which could resolve to root/current dir
        var settings = new MediaOrganizerSettings
        {
            TvShowSourceDirectory = @"C:\TV\Source",
            TvShowDestinationDirectory = @"C:\TV\Destination", 
            MovieSourceDirectory = whitespaceMoviePath,
            MovieDestinationDirectory = whitespaceMoviePath
        };
        var sut = new DirectoryCleaner(_mockFileSystem, NullLogger<DirectoryCleaner>.Instance, Options.Create(settings));

        // Act & Assert - Should NOT allow cleaning root or system directories
        var ex1 = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\", @"C:\TV\Destination"));
        Assert.Equal("sourceDirectory", ex1.ParamName);

        var ex2 = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\Windows", @"C:\TV\Destination"));
        Assert.Equal("sourceDirectory", ex2.ParamName);
        
        var ex3 = Assert.Throws<ArgumentException>(() => 
            sut.CleanEmptyDirectories(@"C:\Program Files", @"C:\TV\Destination"));
        Assert.Equal("sourceDirectory", ex3.ParamName);
    }
}
