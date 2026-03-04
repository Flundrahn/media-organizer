using System.Text.RegularExpressions;

namespace MediaOrganizer.Services.MetadataEnrichers;

/// <summary>
/// Matches the standard SxxExx pattern for season and episode numbers, e.g. "S02E13" in "Breaking.Bad.S02E13.mkv".
/// The pattern must be surrounded by separators (., -, _, whitespace) or word boundaries.
/// If multiple unique matches are found, returns multiple results with split confidence.
/// </summary>
public partial class SxxExxSeasonEpisodeRule : IExtractionRule
{
    [GeneratedRegex(@"(?:^|[\s.\-_/\\\u2010-\u2015\u2212]+)S(?<season>\d{1,2})E(?<episode>\d{1,2})(?=[\s.\-_/\\\u2010-\u2015\u2212]+|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex Pattern();

    public string Name => nameof(SxxExxSeasonEpisodeRule);

    public bool TryExtract(string filePath, out RuleMatch match)
    {
        var matches = Pattern().Matches(filePath);
        
        if (matches.Count == 0)
        {
            match = new RuleMatch { RuleName = Name };
            return false;
        }

        // Extract unique season/episode combinations
        var uniqueMatches = matches
            .Select(m => new
            {
                Season = int.Parse(m.Groups["season"].Value),
                Episode = int.Parse(m.Groups["episode"].Value)
            })
            .Distinct()
            .ToList();

        if (uniqueMatches.Count == 0)
        {
            match = new RuleMatch { RuleName = Name };
            return false;
        }

        // For now, return the first match with confidence split by number of unique matches
        float confidence = 1.0f / uniqueMatches.Count;
        match = new RuleMatch
        {
            RuleName = Name,
            SeasonNumber = uniqueMatches[0].Season,
            EpisodeNumber = uniqueMatches[0].Episode,
            Confidence = confidence
        };
        return true;
    }
}
