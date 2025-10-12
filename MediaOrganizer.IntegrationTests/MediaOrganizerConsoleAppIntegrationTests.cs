using MediaOrganizer.Configuration;
using MediaOrganizer.IntegrationTests.TestHelpers;
using MediaOrganizer.IO;
using MediaOrganizer.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.IntegrationTests;

public class MediaOrganizerConsoleAppIntegrationTests
{
    private static MediaOrganizerSettings CreateSettingsForIntegrationTest(TempMediaTestEnvironment environment)
    {
        return new MediaOrganizerSettings
        {
            AutoCleanupEmptyDirectories = true,
            TvShowSourceDirectory = environment.MediaSourceDirectory,
            TvShowDestinationDirectory = environment.MediaDestinationDirectory,
            MovieSourceDirectory = environment.MediaSourceDirectory,
            MovieDestinationDirectory = environment.MediaDestinationDirectory,
            DryRun = false,
            TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}",
            MoviePathTemplate = "{Title} ({Year})",
            VideoFileExtensions = new List<string> { ".mkv", ".mp4", ".avi" }
        };
    }

    [Fact]
    public void ConsoleApp_OrganizeTvShow_WithAutoCleanup_MovesFileAndCleansDirectories()
    {
        // Arrange
        using var environment = new TempMediaTestEnvironment();
        var mockConsole = new MockConsoleIO();

        string relativeSourceFilePath = Path.Combine("The Office (2019)", "The.Office.S01E01.mkv");
        string sourceFileFullPath = environment.CreateFile(relativeSourceFilePath);
        string nestedSourceDirectoryFullPath = Path.Combine(environment.MediaSourceDirectory, "The Office (2019)");
        string expectedOrganizedFilePath = Path.Combine(environment.MediaDestinationDirectory, "The Office", "Season 1", "The Office - S01E01.mkv");

        MediaOrganizerSettings settings = CreateSettingsForIntegrationTest(environment);

        var services = new ServiceCollection();
        var provider = services
            .AddMediaOrganizerServices(new ConfigurationBuilder().Build())
            .AddSingleton(Options.Create(settings))
            .AddSingleton<IConsoleIO>(mockConsole)
            .BuildServiceProvider();

        var consoleApp = provider.GetRequiredService<MediaOrganizerConsoleApp>();

        // Doing integration test like this is a bit brittle,
        // however it would also enforce to not change user experience without failing test,
        // which would help maintaining backwards compatibility of console app
        // Keep these tests for now.

        mockConsole.QueueKeyInput(ConsoleKey.D1, '1'); // 1. Select TV Shows (option 1)
        mockConsole.QueueKeyInput(ConsoleKey.D2, '2'); // 2. Organize files (option 2) 
        mockConsole.QueueKeyInput(ConsoleKey.A, 'A');  // 3. Organize all files (A)
        mockConsole.QueueKeyInput(ConsoleKey.Enter);   // Continue prompt
        mockConsole.QueueKeyInput(ConsoleKey.B, 'B');  // 4. Back to main menu (B)
        mockConsole.QueueKeyInput(ConsoleKey.Q, 'Q');  // 5. Quit (Q)

        // Act
        var exitCode = consoleApp.Run();

        // Assert
        Assert.Equal(0, exitCode);
        Assert.False(File.Exists(sourceFileFullPath), $"Expected organized file to be moved away from: {sourceFileFullPath}");
        Assert.True(File.Exists(expectedOrganizedFilePath), $"Expected organized file to exist at: {expectedOrganizedFilePath}");
        Assert.False(Directory.Exists(nestedSourceDirectoryFullPath), $"Source nested directory should be cleaned up after moving files: {nestedSourceDirectoryFullPath}");

        // Verify success message in console output
        Assert.Contains(mockConsole.Output, output => output.Contains("[SUCCESS]") && output.Contains("Organization complete"));
        Assert.Contains(mockConsole.Output, output => output.Contains("[SUCCESS]") && output.Contains("Empty directory cleanup completed"));
    }

    [Fact]
    public void ConsoleApp_OrganizeMovie_WithAutoCleanup_MovesFileAndCleansDirectories()
    {
        // Arrange
        using var environment = new TempMediaTestEnvironment();
        var mockConsole = new MockConsoleIO();

        string relativeSourceFilePath = Path.Combine("Movies", "The.Matrix.1999.1080p.BluRay.mkv");
        string sourceFileFullPath = environment.CreateFile(relativeSourceFilePath);
        string nestedSourceDirectoryFullPath = Path.Combine(environment.MediaSourceDirectory, "Movies");
        string expectedOrganizedFilePath = Path.Combine(environment.MediaDestinationDirectory, "The Matrix (1999).mkv");

        MediaOrganizerSettings settings = CreateSettingsForIntegrationTest(environment);

        var services = new ServiceCollection();
        var provider = services
            .AddMediaOrganizerServices(new ConfigurationBuilder().Build())
            .AddSingleton(Options.Create(settings))
            .AddSingleton<IConsoleIO>(mockConsole)
            .BuildServiceProvider();

        var consoleApp = provider.GetRequiredService<MediaOrganizerConsoleApp>();

        mockConsole.QueueKeyInput(ConsoleKey.D2, '2'); // 1. Select Movies (option 2)
        mockConsole.QueueKeyInput(ConsoleKey.D2, '2'); // 2. Organize files (option 2)
        mockConsole.QueueKeyInput(ConsoleKey.A, 'A');  // 3. Organize all files (A)
        mockConsole.QueueKeyInput(ConsoleKey.Enter);   // Continue prompt
        mockConsole.QueueKeyInput(ConsoleKey.B, 'B');  // 4. Back to main menu (B)
        mockConsole.QueueKeyInput(ConsoleKey.Q, 'Q');  // 5. Quit (Q)

        // Act
        var exitCode = consoleApp.Run();

        // Assert
        Assert.Equal(0, exitCode);
        Assert.False(File.Exists(sourceFileFullPath), $"Expected organized file to be moved away from: {sourceFileFullPath}");
        Assert.True(File.Exists(expectedOrganizedFilePath), $"Expected organized file to exist at: {expectedOrganizedFilePath}");
        Assert.False(Directory.Exists(nestedSourceDirectoryFullPath), $"Source nested directory should be cleaned up after moving files: {nestedSourceDirectoryFullPath}");

        // Verify success message in console output
        Assert.Contains(mockConsole.Output, output => output.Contains("[SUCCESS]") && output.Contains("Organization complete"));
        Assert.Contains(mockConsole.Output, output => output.Contains("[SUCCESS]") && output.Contains("Empty directory cleanup completed"));
    }

    [Fact]
    public void ConsoleApp_ManualCleanupTvShow_CleansOnlyTvShowDirectories()
    {
        // Arrange
        using var environment = new TempMediaTestEnvironment();
        var mockConsole = new MockConsoleIO();

        // Create empty directories in both TV and Movie areas
        var emptyTvDir = Path.Combine(environment.MediaDestinationDirectory, "Empty TV Show");
        var emptyMovieDir = Path.Combine(environment.MediaDestinationDirectory, "Empty Movie");
        Directory.CreateDirectory(emptyTvDir);
        Directory.CreateDirectory(emptyMovieDir);

        MediaOrganizerSettings settings = CreateSettingsForIntegrationTest(environment);
        settings.AutoCleanupEmptyDirectories = false; // Manual cleanup only
        settings.MovieDestinationDirectory = Path.Combine(environment.TempDirectoryRoot, "movies"); // Different destination

        // Create movie destination directory and empty dir within it
        Directory.CreateDirectory(settings.MovieDestinationDirectory);
        var emptyMovieDestDir = Path.Combine(settings.MovieDestinationDirectory, "Empty Movie Dest");
        Directory.CreateDirectory(emptyMovieDestDir);

        var services = new ServiceCollection();
        var provider = services
            .AddMediaOrganizerServices(new ConfigurationBuilder().Build())
            .AddSingleton(Options.Create(settings))
            .AddSingleton<IConsoleIO>(mockConsole)
            .BuildServiceProvider();

        var consoleApp = provider.GetRequiredService<MediaOrganizerConsoleApp>();

        mockConsole.QueueKeyInput(ConsoleKey.D1, '1'); // 1. Select TV Shows (option 1)
        mockConsole.QueueKeyInput(ConsoleKey.D3, '3'); // 2. Clean directories (option 3)
        mockConsole.QueueKeyInput(ConsoleKey.Enter);   // 3. Continue prompt
        mockConsole.QueueKeyInput(ConsoleKey.B, 'B');  // 4. Back to main menu (B)
        mockConsole.QueueKeyInput(ConsoleKey.Q, 'Q');  // 5. Quit (Q)

        // Act
        var exitCode = consoleApp.Run();

        // Assert
        Assert.Equal(0, exitCode);
        Assert.False(Directory.Exists(emptyTvDir), $"Empty TV directory should be cleaned up: {emptyTvDir}");
        Assert.True(Directory.Exists(emptyMovieDestDir), $"Movie destination directory should NOT be cleaned up by TV cleanup: {emptyMovieDestDir}");

        // Verify success message in console output
        Assert.Contains(mockConsole.Output, output => output.Contains("[SUCCESS]") && output.Contains("TV Show directory cleanup completed"));
    }
}