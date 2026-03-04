using System.IO.Abstractions;
using MediaOrganizer.Infrastructure.ApiClients;
using MediaOrganizer.Models;
using MediaOrganizer.Services.MetadataEnrichers;
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
        private readonly Queue<SearchContainer<SearchTv>?> _searchResponses = new();
        private readonly Queue<TmdbTvEpisode?> _episodeResponses = new();

        public void EnqueueSearchResponse(SearchContainer<SearchTv>? resp) => _searchResponses.Enqueue(resp);
        public void EnqueueEpisodeResponse(TmdbTvEpisode? resp) => _episodeResponses.Enqueue(resp);

        public Task<SearchContainer<SearchTv>> SearchTvShowAsync(string query, int page = 1, bool includeAdult = false, int year = 0, CancellationToken cancellationToken = default)
        {
            if (_searchResponses.Count > 0)
            {
                var resp = _searchResponses.Dequeue();
                if (resp is null) throw new InvalidOperationException("search failed");
                return Task.FromResult(resp);
            }

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

            if (_episodeResponses.Count > 0)
            {
                return Task.FromResult(_episodeResponses.Dequeue());
            }

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

    private static TvEpisode CreateTvEpisode(string showName = "ShowName", int season = 1, int episode = 1, int year = 0)
    {
        var fileInfoMock = new Mock<IFileInfo>();
        fileInfoMock.SetupGet(f => f.FullName)
                    .Returns("SourceDirectory/ShowName.S01E01.mkv");

        return new TvEpisode(fileInfoMock.Object, "SourceDirectory")
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
        Assert.False(result.Result.IsSuccess);
        Assert.Contains("failed", result.Result.Error, StringComparison.OrdinalIgnoreCase);
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
        Assert.False(result.Result.IsSuccess);
        Assert.Contains("no results", result.Result.Error, StringComparison.OrdinalIgnoreCase);
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
        Assert.True(result.Result.IsSuccess);
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
        Assert.False(result.Result.IsSuccess);
        Assert.Contains("failed", result.Result.Error, StringComparison.OrdinalIgnoreCase);
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
        Assert.False(result.Result.IsSuccess);
        Assert.Contains("not found", result.Result.Error, StringComparison.OrdinalIgnoreCase);
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
        Assert.True(result.Result.IsSuccess);
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
        Assert.True(result.Result.IsSuccess);
        Assert.Equal(expectedTitle, episode.Title);
        Assert.Equal(yearBeforeEnriching, episode.Year);
    }

    [Fact]
    public async Task EnrichAllAsync_WhenAllSucceed_ReturnsSuccessForAll()
    {
        // Arrange - two files
        var firstTv = new SearchTv
        {
            Id = 11,
            Name = "ShowOne",
            FirstAirDate = new DateTime(2001, 1, 1)
        };
        var secondTv = new SearchTv
        {
            Id = 22,
            Name = "ShowTwo",
            FirstAirDate = new DateTime(2002, 1, 1)
        };

        _fakeTmdbClient.EnqueueSearchResponse(new SearchContainer<SearchTv> { Results = [firstTv] });
        _fakeTmdbClient.EnqueueSearchResponse(new SearchContainer<SearchTv> { Results = [secondTv] });

        var ep1 = new TmdbTvEpisode
        {
            Id = 1001,
            Name = "Ep1",
            AirDate = new DateTime(2001, 2, 3)
        };
        var ep2 = new TmdbTvEpisode
        {
            Id = 2002,
            Name = "Ep2",
            AirDate = new DateTime(2002, 3, 4)
        };

        _fakeTmdbClient.EnqueueEpisodeResponse(ep1);
        _fakeTmdbClient.EnqueueEpisodeResponse(ep2);

        var file1 = CreateTvEpisode("ShowOne", 1, 1);
        var file2 = CreateTvEpisode("ShowTwo", 1, 1);

        // Act
        var results = (await _sut.EnrichAllAsync(new[] { file1, file2 })).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results[0].Result.IsSuccess);
        Assert.True(results[1].Result.IsSuccess);
        Assert.Equal("Ep1", file1.Title);
        Assert.Equal(2001, file1.Year);
        Assert.Equal("Ep2", file2.Title);
        Assert.Equal(2002, file2.Year);
    }

    [Fact]
    public async Task EnrichAllAsync_WhenOneSearchFails_ReturnsFailureForThatFile()
    {
        // Arrange - first search will fail due to null response
        var secondTv = new SearchTv
        {
            Id = 44,
            Name = "GoodShow"
        };

        _fakeTmdbClient.EnqueueSearchResponse(null); 
        _fakeTmdbClient.EnqueueSearchResponse(new SearchContainer<SearchTv> { Results = [secondTv] });

        var ep2 = new TmdbTvEpisode
        {
            Id = 4004,
            Name = "GoodEp",
            AirDate = new DateTime(2010, 5, 6)
        };
        _fakeTmdbClient.EnqueueEpisodeResponse(ep2);

        var file1 = CreateTvEpisode("BadShow", 1, 1);
        var file2 = CreateTvEpisode("GoodShow", 1, 1);

        // Act
        var results = (await _sut.EnrichAllAsync(new[] { file1, file2 })).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.False(results[0].Result.IsSuccess);
        Assert.True(results[1].Result.IsSuccess);
        Assert.Equal("GoodEp", file2.Title);
    }
}
