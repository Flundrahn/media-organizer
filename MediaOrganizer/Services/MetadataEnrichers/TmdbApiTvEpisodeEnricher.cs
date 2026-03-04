using MediaOrganizer.Infrastructure.ApiClients;
using MediaOrganizer.Models;
using MediaOrganizer.Results;
using Microsoft.Extensions.Logging;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TmdbTvEpisode = TMDbLib.Objects.TvShows.TvEpisode;

namespace MediaOrganizer.Services.MetadataEnrichers;

public class TmdbApiTvEpisodeEnricher : IMediaFileEnricher<TvEpisode>
{
    private readonly ILogger<TmdbApiTvEpisodeEnricher> _logger;
    private readonly ITmdbApiClient _tmdbApi;

    public TmdbApiTvEpisodeEnricher(ILogger<TmdbApiTvEpisodeEnricher> logger, ITmdbApiClient tmdbApi)
    {
        _logger = logger;
        _tmdbApi = tmdbApi;
    }

    public async Task<TvEpisodeEnrichmentResult> EnrichAsync(TvEpisode mediaFile)
    {
        Result<SearchTv> tvShowSearchResult = await SearchTvShowAsync(mediaFile);
        if (!tvShowSearchResult.IsSuccess)
        {
            return new TvEpisodeEnrichmentResult(mediaFile, tvShowSearchResult);
        }

        Result<TmdbTvEpisode> tvEpisodeResult = await GetTvEpisodeAsync(mediaFile, tvShowSearchResult.Value.Id);
        if (!tvEpisodeResult.IsSuccess)
        {
            return new TvEpisodeEnrichmentResult(mediaFile, tvEpisodeResult);
        }

        EnrichMediaFile(mediaFile, tvEpisodeResult.Value);
        return new TvEpisodeEnrichmentResult(mediaFile, ResultBase.Success());
    }

    private void EnrichMediaFile(TvEpisode mediaFile, TmdbTvEpisode tmdbTvEpisode)
    {
        mediaFile.Title = tmdbTvEpisode.Name;
        if (tmdbTvEpisode.AirDate.HasValue)
        {
            mediaFile.Year = tmdbTvEpisode.AirDate.Value.Year;
        }
        _logger.LogInformation("Enriched {ShowName} S{Season}E{Episode} -> {Year} / {Title}",
                               mediaFile.TvShowName,
                               mediaFile.Season,
                               mediaFile.Episode,
                               mediaFile.Year,
                               mediaFile.Title);
    }

    private async Task<Result<SearchTv>> SearchTvShowAsync(TvEpisode mediaFile)
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

    private async Task<Result<TmdbTvEpisode>> GetTvEpisodeAsync(TvEpisode mediaFile, int tmdbTvShowId)
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

    public async Task<IEnumerable<TvEpisodeEnrichmentResult>> EnrichAllAsync(IEnumerable<TvEpisode> mediaFiles)
    {
        var searchTvShowTasks = new List<Task<Result<SearchTv>>>();
        var searchTvShowTaskToFiles = new Dictionary<Task<Result<SearchTv>>, TvEpisode>();

        // start search show tasks for all files
        foreach (TvEpisode file in mediaFiles)
        {
            var task = SearchTvShowAsync(file);
            searchTvShowTasks.Add(task);
            searchTvShowTaskToFiles[task] = file;
        }

        int fileCount = searchTvShowTasks.Count;
        var getEpisodeTasks = new List<Task<Result<TmdbTvEpisode>>>(fileCount);
        var getEpisodeTaskToFiles = new Dictionary<Task<Result<TmdbTvEpisode>>, TvEpisode>(fileCount);

        // as searches complete, start get episode fetches 
        while (searchTvShowTasks.Count > 0)
        {
            var finishedSearch = await Task.WhenAny(searchTvShowTasks);
            var searchTvShowResult = await finishedSearch;
            // note we don't bother removing from task-2-file mapping dictionary
            searchTvShowTasks.Remove(finishedSearch);

            TvEpisode file = searchTvShowTaskToFiles[finishedSearch];
            Task<Result<TmdbTvEpisode>> task;

            if (searchTvShowResult.IsSuccess)
            {
                // start get episode 
                task = GetTvEpisodeAsync(file, searchTvShowResult.Value.Id);
            }
            else
            {
                task = Task.FromResult(Result<TmdbTvEpisode>.Failure(searchTvShowResult.Error));
            }

            getEpisodeTasks.Add(task);
            getEpisodeTaskToFiles[task] = file;
        }

        var results = new List<TvEpisodeEnrichmentResult>(fileCount);

        // as getEpisodeTasks complete, enrich each media files with metadata
        while (getEpisodeTasks.Count > 0)
        {
            var finishedGetEpisodeTask = await Task.WhenAny(getEpisodeTasks);
            var getEpisodeResult = await finishedGetEpisodeTask;
            // note we don't bother removing from task-2-file mapping dictionary
            getEpisodeTasks.Remove(finishedGetEpisodeTask);

            TvEpisode file = getEpisodeTaskToFiles[finishedGetEpisodeTask];
            TvEpisodeEnrichmentResult result;

            if (getEpisodeResult.IsSuccess)
            {
                EnrichMediaFile(file, getEpisodeResult.Value);
                result = new TvEpisodeEnrichmentResult(file, ResultBase.Success());
            }
            else
            {
                result = new TvEpisodeEnrichmentResult(file, ResultBase.Failure(getEpisodeResult.Error));
            }

            results.Add(result);
        }

        return results;
    }
}
