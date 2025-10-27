using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMDbLib.Client;
using TMDbLib.Objects.TvShows;

namespace MediaOrganizer.IntegrationTests;

// TODO: Structure integration tests to reflect main project
public class TmdbApiClientIntegrationTests
{
    private readonly TMDbClient _apiClient;

    public TmdbApiClientIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            // NOTE: Possibly DRY configuration of other integration test classes
            // .AddJsonFile("appsettings.json", optional: false)
            // .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<TmdbApiClientIntegrationTests>()
            .Build();

        var services = new ServiceCollection();
        services.AddMediaOrganizerServices(configuration);

        var serviceProvider = services.BuildServiceProvider();
        _apiClient = serviceProvider.GetRequiredService<TMDbClient>();
    }

    [Fact]
    public async Task SearchTvShowAsync_ForValidShowName_ReturnsResults()
    {
        // Act
        var result = await _apiClient.SearchTvShowAsync("Breaking Bad");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotEmpty(result.Results);
        Assert.NotNull(result.Results[0].Name);
        Assert.Contains("Breaking Bad", result.Results[0].Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTvEpisodeAsync_ForKnownShow_ReturnsYearAndTitle()
    {
        // Arrange
        const int breakingBadTvShowId = 1396;
        const int seasonNumber = 1;
        const int episodeNumber = 1;

        // Act
        TvEpisode episode = await _apiClient.GetTvEpisodeAsync(breakingBadTvShowId, seasonNumber, episodeNumber);

        // Assert
        Assert.NotNull(episode);
        Assert.Equal("Pilot", episode.Name);
    }

    [Fact]
    public async Task GetTvEpisodeAsync_ForNonExistentEpisode_ReturnsNull()
    {
        // Arrange
        const int breakingBadTvShowId = 1396;
        const int seasonNumber = 99;
        const int episodeNumber = 99;

        // Act
        TvEpisode episode = await _apiClient.GetTvEpisodeAsync(breakingBadTvShowId, seasonNumber, episodeNumber);

        // Assert
        Assert.Null(episode);
    }
}