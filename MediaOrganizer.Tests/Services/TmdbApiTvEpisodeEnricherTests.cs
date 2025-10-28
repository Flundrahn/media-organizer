using System.IO.Abstractions;
using MediaOrganizer.Infrastructure.ApiClients;
using MediaOrganizer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TmdbTvEpisode = TMDbLib.Objects.TvShows.TvEpisode;

namespace MediaOrganizer.Tests.Services;

public class TmdbApiTvEpisodeEnricherTests
{
    private class FakeTmdbClient : ITmdbApiClient
    {
        public bool ThrowOnSearchTvShow { get; set; }
        public bool ThrowOnGetTvEpisode { get; set; }
        public SearchContainer<SearchTv>? SearchResponse { get; set; }
        public TmdbTvEpisode? EpisodeResponse { get; set; }
        public int? LastRequestedTvShowId { get; private set; }

        public Task<SearchContainer<SearchTv>> SearchTvShowAsync(string query, int page = 1, bool includeAdult = false, int year = 0, CancellationToken cancellationToken = default)
        {
            if (ThrowOnSearchTvShow)
            {
                throw new InvalidOperationException("search failed");
            }

            return Task.FromResult(SearchResponse ?? new SearchContainer<SearchTv> { Results = [] });
        }

        public Task<TmdbTvEpisode?> GetTvEpisodeAsync(int tvShowId, int seasonNumber, int episodeNumber)
        {
            // To verify client called with expected TMDB show id
            LastRequestedTvShowId = tvShowId;

            if (ThrowOnGetTvEpisode)
            {
                throw new InvalidOperationException("get episode failed");
            }

            return Task.FromResult(EpisodeResponse);
        }
    }

    private readonly FakeTmdbClient _fakeTmdbClient;
    private readonly TmdbApiTvEpisodeEnricher _sut;

    public TmdbApiTvEpisodeEnricherTests()
    {
        _fakeTmdbClient = new FakeTmdbClient();
        _sut = new TmdbApiTvEpisodeEnricher(NullLogger<TmdbApiTvEpisodeEnricher>.Instance, _fakeTmdbClient);
    }

    private static MediaOrganizer.Models.TvEpisode CreateTvEpisode(string showName = "ShowName", int season = 1, int episode = 1, int year = 0)
    {
        var fileInfoMock = new Mock<IFileInfo>();
        return new MediaOrganizer.Models.TvEpisode(fileInfoMock.Object)
        {
            TvShowName = showName,
            Season = season,
            Episode = episode,
            Title = string.Empty,
            Year = year
        };
    }

    [Fact]
    public async Task EnrichAsync_IfSearchTvShowThrows_ShouldReturnFailure()
    {
        // Arrange
        _fakeTmdbClient.ThrowOnSearchTvShow = true;
        var episode = CreateTvEpisode();

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("failed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrichAsync_IfSearchTvShowYieldsNoResults_ShouldReturnFailure()
    {
        // Arrange
        _fakeTmdbClient.SearchResponse = new SearchContainer<SearchTv>
        {
            Results = []
        };
        var episode = CreateTvEpisode();

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("no results", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrichAsync_IfSearchTvShowYieldsMultipleResults_ShouldUseFirstAndReturnSuccess()
    {
        // Arrange
        var firstTv = new SearchTv
        {
            Id = 1,
            Name = "Breaking Bad",
            FirstAirDate = new DateTime(2008, 1, 20)
        };
        var secondTv = new SearchTv
        {
            Id = 2,
            Name = "Other Show"
        };
        _fakeTmdbClient.SearchResponse = new SearchContainer<SearchTv>
        {
            Results = [firstTv, secondTv]
        };

        string expectedTitle = "Pilot";
        int expectedYear = 2008;
        _fakeTmdbClient.EpisodeResponse = new TmdbTvEpisode
        {
            Id = 101,
            Name = expectedTitle,
            AirDate = new DateTime(expectedYear, 1, 20)
        };

        var episode = CreateTvEpisode();

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTitle, episode.Title);
        Assert.Equal(expectedYear, episode.Year);
        Assert.Equal(firstTv.Id, _fakeTmdbClient.LastRequestedTvShowId);
    }

    [Fact]
    public async Task EnrichAsync_IfGetTvEpisodeThrows_ShouldReturnFailure()
    {
        // Arrange
        var tv = new SearchTv
        {
            Id = 42,
            Name = "ShowName"
        };
        var search = new SearchContainer<SearchTv>
        {
            Results = [tv]
        };
        _fakeTmdbClient.SearchResponse = search;
        _fakeTmdbClient.ThrowOnGetTvEpisode = true;
        var episode = CreateTvEpisode();

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("failed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrichAsync_IfTmdbEpisodeNotFound_ShouldReturnFailure()
    {
        // Arrange
        var tv = new SearchTv
        {
            Id = 100,
            Name = "ShowName"
        };
        var search = new SearchContainer<SearchTv>
        {
            Results = [tv]
        };
        _fakeTmdbClient.SearchResponse = search;
        _fakeTmdbClient.EpisodeResponse = null;
        var episode = CreateTvEpisode();

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnrichAsync_IfTmdbEpisodeFound_ShouldSetTitleAndYearAndReturnsSuccess()
    {
        // Arrange
        var tv = new SearchTv
        {
            Id = 7,
            Name = "ShowName",
            FirstAirDate = new DateTime(2010, 1, 1)
        };
        _fakeTmdbClient.SearchResponse = new SearchContainer<SearchTv>
        {
            Results = [tv]
        };

        int expectedYear = 2010;
        string expectedTitle = "Pilot";
        _fakeTmdbClient.EpisodeResponse = new TmdbTvEpisode
        {
            Id = 555,
            Name = expectedTitle,
            AirDate = new DateTime(expectedYear, 2, 3)
        };

        var episode = CreateTvEpisode("EpisodeTitleBeforeEnriching", 1, 1);

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTitle, episode.Title);
        Assert.Equal(expectedYear, episode.Year);
    }

    [Fact]
    public async Task EnrichAsync_IfTmdbEpisodeHasNoAirDate_ShouldNotSetYearAndReturnsSuccess()
    {
        // Arrange
        var tv = new SearchTv
        {
            Id = 8,
            Name = "ShowName"
        };
        _fakeTmdbClient.SearchResponse = new SearchContainer<SearchTv>
        {
            Results = [tv]
        };

        string expectedTitle = "Pilot";
        _fakeTmdbClient.EpisodeResponse = new TmdbTvEpisode
        {
            Id = 999,
            Name = expectedTitle,
            AirDate = null
        };

        int yearBeforeEnriching = 2010;
        var episode = CreateTvEpisode("ShowName", 2, 3, yearBeforeEnriching);

        // Act
        var result = await _sut.EnrichAsync(episode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTitle, episode.Title);
        Assert.Equal(yearBeforeEnriching, episode.Year);
    }
}
