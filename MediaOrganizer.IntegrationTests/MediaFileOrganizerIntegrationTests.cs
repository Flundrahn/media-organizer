using MediaOrganizer.IntegrationTests.TestHelpers;
using MediaOrganizer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaOrganizer.IntegrationTests
{
    public class MediaFileOrganizerIntegrationTests
    {

        [Fact]
        public void Organize_File_IsMoved_And_EmptyDirectories_Cleaned_WhenEnabled()
        {
            using var environment = new TempMediaTestEnvironment();

            string nestedSourceDirectoryName = "The Office (2019)";
            string relativeSourceFilePath = Path.Combine(nestedSourceDirectoryName, "The.Office.S01E01.mkv");
            string sourceFileFullPath = environment.CreateFile(relativeSourceFilePath);
            // See used TvShowPathTemplate below
            string expectedOrganizedFilePath = Path.Combine(environment.DestinationDirectory, "The Office", "Season 1", "The Office - S01E01.mkv");
            string nestedSourceDirectoryFullPath = Path.Combine(environment.SourceDirectory, nestedSourceDirectoryName);

            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "MediaOrganizer:AutoCleanupEmptyDirectories", "true" },
                { "MediaOrganizer:SourceDirectory", environment.SourceDirectory },
                { "MediaOrganizer:DestinationDirectory", environment.DestinationDirectory },
                { "MediaOrganizer:DryRun", "false" },
                { "MediaOrganizer:IncludeSubdirectories", "true" },
                { "MediaOrganizer:TvShowPathTemplate", "{TvShowName}/Season {Season}/{TvShowName} - S{Season:D2}E{Episode:D2}" },
                { "MediaOrganizer:VideoFileExtensions:0", ".mkv" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            var services = new ServiceCollection();
            var provider = services
                .AddMediaOrganizerServices(configuration)
                .BuildServiceProvider();

            var mediaFileProvider = provider.GetRequiredService<IMediaFileProvider>();
            var organizer = provider.GetRequiredService<MediaFileOrganizer>();

            // Use MediaFileProvider to get media files (same as the actual application)
            var mediaFiles = mediaFileProvider.GetMediaFiles(environment.SourceDirectory);
            organizer.Initialize(mediaFiles);

            // Act
            var result = organizer.OrganizeAllFiles();

            // Assert
            Assert.False(File.Exists(sourceFileFullPath), $"Expected organized file to be moved away from: {sourceFileFullPath}");
            Assert.True(File.Exists(expectedOrganizedFilePath), $"Expected organized file to exist at: {expectedOrganizedFilePath}");
            Assert.False(Directory.Exists(nestedSourceDirectoryFullPath), $"Source nested directory should be cleaned up after moving files: {nestedSourceDirectoryFullPath}");
        }
    }
}
