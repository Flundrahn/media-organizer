using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMDbLib.Client;

namespace MediaOrganizer.IntegrationTests;

public class TmdbApiClientIntegrationTests
{
    [Fact]
    public async Task SearchTvShowAsync_ReturnsResults_ForValidShowName()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            // NOTE: Possibly DRY setup of integration tests
            // .AddJsonFile("appsettings.json", optional: false)
            // .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<TmdbApiClientIntegrationTests>()
            .Build();

        var services = new ServiceCollection();
        services.AddMediaOrganizerServices(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var apiClient = serviceProvider.GetRequiredService<TMDbClient>();

        // Act
        var result = await apiClient.SearchTvShowAsync("Breaking Bad");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotEmpty(result.Results);
        Assert.NotNull(result.Results[0].Name);
        Assert.Contains("Breaking Bad", result.Results[0].Name, StringComparison.OrdinalIgnoreCase);
    }
}