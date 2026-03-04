using MediaOrganizer.Models;
using MediaOrganizer.Results;
using Microsoft.Extensions.Logging;

namespace MediaOrganizer.Services.MetadataEnrichers;

public class FilePathTvEpisodeEnricher : IMediaFileEnricher<TvEpisode>
{
    private readonly ILogger<FilePathTvEpisodeEnricher> _logger;
    private readonly IReadOnlyList<IExtractionRule> _rules;
    private TvEpisode? _mediaFile;

    public FilePathTvEpisodeEnricher(ILogger<FilePathTvEpisodeEnricher> logger)
    {
        _logger = logger;
        _rules = [new SxxExxSeasonEpisodeRule()];
    }

    public Task<IEnumerable<TvEpisodeEnrichmentResult>> EnrichAllAsync(IEnumerable<TvEpisode> mediaFiles)
    {
        return Task.FromResult(mediaFiles.Select(EnrichAsyncInner));
    }

    public Task<TvEpisodeEnrichmentResult> EnrichAsync(TvEpisode mediaFile)
    {
        return Task.FromResult(EnrichAsyncInner(mediaFile));
    }

    private TvEpisodeEnrichmentResult EnrichAsyncInner(TvEpisode mediaFile)
    { 
         _logger.LogDebug("Enriching file {OriginalFileRelativePath}", mediaFile.OriginalFileRelativePath);
        _mediaFile = mediaFile;

        var matches = new List<RuleMatch>();
        foreach (var rule in _rules)
        {
            if (rule.TryExtract(mediaFile.OriginalFileRelativePath, out RuleMatch match))
                matches.Add(match);
        }

        if (matches.Count == 0)
        {
            return new TvEpisodeEnrichmentResult( mediaFile, ResultBase.Failure($"No extraction rule matched."));
        }

        ApplyMatches(matches);
        return new TvEpisodeEnrichmentResult(mediaFile, ResultBase.Success());
    }

    /// <summary>
    /// Merges multiple rule matches by confidence and applies the result to the stored TV episode.
    /// For each field, the value from the highest-confidence match that provides it is used.
    /// </summary>
    private void ApplyMatches(IEnumerable<RuleMatch> matches)
    {
        string? showName = null;
        int? seasonNumber = null;
        int? episodeNumber = null;
        string? episodeTitle = null;
        int? year = null;

        foreach (var match in matches.OrderByDescending(m => m.Confidence))
        {
            showName ??= match.ShowName;
            seasonNumber ??= match.SeasonNumber;
            episodeNumber ??= match.EpisodeNumber;
            episodeTitle ??= match.EpisodeTitle;
            year ??= match.Year;
        }

        if (showName is not null)
            _mediaFile!.TvShowName = showName;
        if (seasonNumber.HasValue)
            _mediaFile!.Season = seasonNumber.Value;
        if (episodeNumber.HasValue)
            _mediaFile!.Episode = episodeNumber.Value;
        if (episodeTitle is not null)
            _mediaFile!.Title = episodeTitle;
        if (year.HasValue)
            _mediaFile!.Year = year.Value;
    }
}
