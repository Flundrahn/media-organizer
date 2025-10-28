using MediaOrganizer.Infrastructure.ApiClients;
using MediaOrganizer.Utils;
using Microsoft.Extensions.Logging;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TmdbTvEpisode = TMDbLib.Objects.TvShows.TvEpisode;

namespace MediaOrganizer.Services;

public class TmdbApiTvEpisodeEnricher : IMediaFileEnricher<Models.TvEpisode>
{
    private readonly ILogger<TmdbApiTvEpisodeEnricher> _logger;
    private readonly ITmdbApiClient _tmdbApi;

    public TmdbApiTvEpisodeEnricher(ILogger<TmdbApiTvEpisodeEnricher> logger, ITmdbApiClient tmdbApi)
    {
        _logger = logger;
        _tmdbApi = tmdbApi;
    }

    public async Task<ResultBase> EnrichAsync(Models.TvEpisode mediaFile)
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

        Result<TmdbTvEpisode> tvEpisodeResult = await GetTvEpisodeAsync(mediaFile, tvShow.Id);
        if (!tvEpisodeResult.IsSuccess)
        {
            return tvEpisodeResult;
        }
        TmdbTvEpisode tvEpisode = tvEpisodeResult.Value;

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

    private async Task<Result<SearchTv>> SearchTvShowAsync(Models.TvEpisode mediaFile)
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

    private async Task<Result<TmdbTvEpisode>> GetTvEpisodeAsync(Models.TvEpisode mediaFile, int tmdbTvShowId)
    {
        TmdbTvEpisode? result;
        try
        {
            result = await _tmdbApi.GetTvEpisodeAsync(tmdbTvShowId, mediaFile.Season, mediaFile.Episode);
        }
        catch (Exception ex)
        {

            string message = $"Getting TV episode {mediaFile.TvShowName} S{mediaFile.Season}E{mediaFile.Episode} failed with unexpected error.";
            _logger.LogError(ex, message);
            return Result<TmdbTvEpisode>.Failure(message);
        }

        if (result is null)
        {
            return Result<TmdbTvEpisode>.Failure("Episode not found in TMDB.");
        }

        return Result<TmdbTvEpisode>.Success(result);
    }

    public Task EnrichAllAsync(IEnumerable<Models.TvEpisode> mediaFiles)
    {
        throw new NotImplementedException();
    }
}
