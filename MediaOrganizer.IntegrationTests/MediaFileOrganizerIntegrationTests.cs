using MediaOrganizer.Configuration;
using MediaOrganizer.IntegrationTests.TestHelpers;
using MediaOrganizer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MediaOrganizer.IntegrationTests
{
    public class MediaFileOrganizerIntegrationTests
    {
        [Fact]
        public void Organize_File_IsMoved_And_EmptyDirectories_Cleaned_WhenEnabled()
        {
            using var environment = new TempMediaTestEnvironment();

            string relativeSourceFilePath = Path.Combine("The Office (2019)", "The.Office.S01E01.mkv");
            string sourceFileFullPath = environment.CreateFile(relativeSourceFilePath);
            string nestedSourceDirectoryFullPath = Path.Combine(environment.MediaSourceDirectory, "The Office (2019)");
            // See used TvShowPathTemplate below
            string expectedOrganizedFilePath = Path.Combine(environment.MediaDestinationDirectory, "The Office", "Season 1", "The Office - S01E01.mkv");

            var settings = new MediaOrganizerSettings
            {
                AutoCleanupEmptyDirectories = true,
                TvShowSourceDirectory = environment.MediaSourceDirectory,
                TvShowDestinationDirectory = environment.MediaDestinationDirectory,
                MovieSourceDirectory = environment.MediaSourceDirectory,
                MovieDestinationDirectory = environment.MediaDestinationDirectory,
                DryRun = false,
                IncludeSubdirectories = true,
                TvShowPathTemplate = "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}",
                VideoFileExtensions = [".mkv"]
            };

            var services = new ServiceCollection();
            var provider = services
                // Passing in empty configuration, then overriding actual settings object by injecting singleton
                .AddMediaOrganizerServices(new ConfigurationBuilder().Build())
                .AddSingleton(Options.Create(settings))
                .BuildServiceProvider();

            var organizerFactory = provider.GetRequiredService<MediaFileOrganizerFactory>();
            var organizer = organizerFactory.CreateTvShowOrganizer();

            // Act
            var result = organizer.OrganizeAllFiles();

            // Assert
            Assert.False(File.Exists(sourceFileFullPath), $"Expected organized file to be moved away from: {sourceFileFullPath}");
            Assert.True(File.Exists(expectedOrganizedFilePath), $"Expected organized file to exist at: {expectedOrganizedFilePath}");
            Assert.False(Directory.Exists(nestedSourceDirectoryFullPath), $"Source nested directory should be cleaned up after moving files: {nestedSourceDirectoryFullPath}");
        }

        [Fact]
        public void Organize_Movie_File_IsMoved_And_EmptyDirectories_Cleaned_WhenEnabled()
        {
            using var environment = new TempMediaTestEnvironment();

            string relativeSourceFilePath = Path.Combine("Movies", "The.Matrix.1999.1080p.BluRay.mkv");
            string sourceFileFullPath = environment.CreateFile(relativeSourceFilePath);
            string nestedSourceDirectoryFullPath = Path.Combine(environment.MediaSourceDirectory, "Movies");
            // See used MoviePathTemplate below
            string expectedOrganizedFilePath = Path.Combine(environment.MediaDestinationDirectory, "Movies", "The Matrix (1999).mkv");

            var settings = new MediaOrganizerSettings
            {
                AutoCleanupEmptyDirectories = true,
                TvShowSourceDirectory = environment.MediaSourceDirectory,
                TvShowDestinationDirectory = environment.MediaDestinationDirectory,
                MovieSourceDirectory = environment.MediaSourceDirectory,
                MovieDestinationDirectory = environment.MediaDestinationDirectory,
                DryRun = false,
                IncludeSubdirectories = true,
                MoviePathTemplate = "Movies/{Title} ({Year})",
                VideoFileExtensions = [".mkv"]
            };

            var services = new ServiceCollection();
            var provider = services
                // Passing in empty configuration, then overriding actual settings object by injecting singleton
                .AddMediaOrganizerServices(new ConfigurationBuilder().Build())
                .AddSingleton(Options.Create(settings))
                .BuildServiceProvider();

            var organizerFactory = provider.GetRequiredService<MediaFileOrganizerFactory>();
            var organizer = organizerFactory.CreateMovieOrganizer();

            // Act
            var result = organizer.OrganizeAllFiles();

            // Assert
            Assert.False(File.Exists(sourceFileFullPath), $"Expected organized file to be moved away from: {sourceFileFullPath}");
            Assert.True(File.Exists(expectedOrganizedFilePath), $"Expected organized file to exist at: {expectedOrganizedFilePath}");
            Assert.False(Directory.Exists(nestedSourceDirectoryFullPath), $"Source nested directory should be cleaned up after moving files: {nestedSourceDirectoryFullPath}");
        }
    }
}
