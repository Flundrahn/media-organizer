using Microsoft.Extensions.Logging;
using MediaOrganizer.Infrastructure.ApiClients;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;
using MediaOrganizer.Models;
using MediaOrganizer.Utils;

namespace MediaOrganizer.Services;

public class TmdbApiTvShowEpisodeEnricher : IMediaFileEnricher<TvShowEpisode>
{
    private readonly ILogger<TmdbApiTvShowEpisodeEnricher> _logger;
    private readonly ITmdbApiClient _tmdbApi;

    public TmdbApiTvShowEpisodeEnricher(ILogger<TmdbApiTvShowEpisodeEnricher> logger, ITmdbApiClient tmdbApi)
    {
        _logger = logger;
        _tmdbApi = tmdbApi;
    }

    public async Task<ResultBase> EnrichAsync(TvShowEpisode mediaFile)
    {
        // NOTE:
        //1. get tvshow from api
        //2. use tvshow to get episode from api
        //3. use result to add to entity properties
        //4. later use separation to allow proper batching

        Result<SearchTv> tvShowSearchResult = await SearchTvShowAsync(mediaFile);
        if (!tvShowSearchResult.IsSuccess)
        {
            return tvShowSearchResult;
        }
        SearchTv tvShow = tvShowSearchResult.Value;

        Result<TvEpisode> tvEpisodeResult = await GetTvEpisodeAsync(mediaFile, tvShow.Id);
        if (!tvEpisodeResult.IsSuccess)
        {
            return tvEpisodeResult;
        }
        TvEpisode tvEpisode = tvEpisodeResult.Value;

        mediaFile.Title = tvEpisode.Name;
        if (tvEpisode.AirDate.HasValue)
        {
            mediaFile.Year = tvEpisode.AirDate.Value.Year;
        }

        _logger.LogInformation("Enriched {ShowName} S{Season}E{Episode} -> {Year} / {Title}",
                               mediaFile.TvShowName,
                               mediaFile.Season,
                               mediaFile.Episode,
                               mediaFile.Year,
                               mediaFile.Title);

        return ResultBase.Success();
    }

    private async Task<Result<SearchTv>> SearchTvShowAsync(TvShowEpisode mediaFile)
    {
        SearchContainer<SearchTv> search;
        try
        {
            search = await _tmdbApi.SearchTvShowAsync(mediaFile.TvShowName);
        }
        catch (Exception ex)
        {
            string message = $"Searching TMDB for TV show {mediaFile.TvShowName} failed with unexpected error.";
            _logger.LogError(ex, message);
            return Result<SearchTv>.Failure(message);
        }

        if (search.Results.Count == 0)
        {
            return Result<SearchTv>.Failure($"Searching TMDB for TV show {mediaFile.TvShowName} yielded no results");
        }

        if (search.Results.Count > 1)
        {
            _logger.LogWarning("Searching TMDB for TV show {TvShowName} yielded multiple results and is inconclusive. Using first result in response.", mediaFile.TvShowName);
        }

        return Result<SearchTv>.Success(search.Results[0]);
    }

    private async Task<Result<TvEpisode>> GetTvEpisodeAsync(TvShowEpisode mediaFile, int tmdbTvShowId)
    {
        TvEpisode? result;
        try
        {
            result = await _tmdbApi.GetTvEpisodeAsync(tmdbTvShowId, mediaFile.Season, mediaFile.Episode);
        }
        catch (Exception ex)
        {

            string message = $"Getting TV episode {mediaFile.TvShowName} S{mediaFile.Season}E{mediaFile.Episode} failed with unexpected error.";
            _logger.LogError(ex, message);
            return Result<TvEpisode>.Failure(message);
        }

        if (result is null)
        {
            return Result<TvEpisode>.Failure("Episode not found in TMDB.");
        }

        return Result<TvEpisode>.Success(result);
    }

    public Task EnrichAllAsync(IEnumerable<TvShowEpisode> mediaFiles)
    {
        throw new NotImplementedException();
    }
}
