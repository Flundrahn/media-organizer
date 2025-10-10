using System.Text.RegularExpressions;
using System.IO.Abstractions;
using MediaOrganizer.Models;
using System.Globalization;
using MediaOrganizer.Utils;

namespace MediaOrganizer.Services;

public partial class TvShowEpisodeParser : IMediaFileParser
{
    [GeneratedRegex(@"^(?<showName>.+?)(?:\.(?<year>\d{4}))?\.S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\.(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z]))?", RegexOptions.IgnoreCase)]
    private static partial Regex ShowYearSeasonEpisodeTitleWithDotsPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+(?<year>\d{4})\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])(?:\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD))?", RegexOptions.IgnoreCase)]
    private static partial Regex ShowYearSeasonEpisodeTitleQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+\((?<year>\d{4})\)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z]))?", RegexOptions.IgnoreCase)]
    private static partial Regex ShowParenthesesYearSeasonEpisodeTitleWithSpacesPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+(?<season>\d{1,2})x(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])", RegexOptions.IgnoreCase)]
    private static partial Regex ShowSeasonXEpisodeTitleWithSpacesPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?<episodeTitle>[A-Za-z][A-Za-z\s]*[A-Za-z])(?:\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD))?", RegexOptions.IgnoreCase)]
    private static partial Regex ShowSeasonEpisodeTitleQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+-\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})(?:\s+S(?<season>\d{1,2})E(?<episode>\d{1,2}))?\s+-\s+(?<episodeTitle>.+)", RegexOptions.IgnoreCase)]
    private static partial Regex ShowSeasonEpisodeTitleWithSpacedDashesPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex ShowSeasonEpisodeQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<showName>.+?)\s+-\s+S(?<season>\d{1,2})E(?<episode>\d{1,2})", RegexOptions.IgnoreCase)]
    private static partial Regex ShowSpaceDashSpaceSeasonEpisodePattern();

    private static readonly Regex[] AllPatterns = [
        ShowYearSeasonEpisodeTitleWithDotsPattern(),
        ShowYearSeasonEpisodeTitleQualityWithSpacesPattern(),
        ShowParenthesesYearSeasonEpisodeTitleWithSpacesPattern(),
        ShowSeasonXEpisodeTitleWithSpacesPattern(),
        ShowSeasonEpisodeTitleQualityWithSpacesPattern(),
        ShowSeasonEpisodeTitleWithSpacedDashesPattern(),
        ShowSeasonEpisodeQualityWithSpacesPattern(),
        ShowSpaceDashSpaceSeasonEpisodePattern()
    ];

    public bool CanParse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        return AllPatterns.Any(pattern => pattern.IsMatch(nameWithoutExtension));
    }

    public IMediaFile Parse(IFileInfo fileInfo)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);

        foreach (var pattern in AllPatterns)
        {
            var match = pattern.Match(nameWithoutExtension);
            if (!match.Success)
            {
                continue;
            }

            string showName = CleanShowName(match.Groups["showName"].Value);
            int season = int.Parse(match.Groups["season"].Value);
            int episode = int.Parse(match.Groups["episode"].Value);
            string episodeTitle = CleanEpisodeTitle(match.Groups["episodeTitle"].Value);
            int? year = match.Groups["year"].Success
                ? int.Parse(match.Groups["year"].Value)
                : null;
            string quality = match.Groups["quality"].Success
                ? match.Groups["quality"].Value
                : string.Empty;

            return new TvShowEpisode(fileInfo)
            {
                TvShowName = showName,
                Season = season,
                Episode = episode,
                Title = episodeTitle,
                Year = year,
                Quality = quality
            };
        }

        return new TvShowEpisode(fileInfo);
    }

    private static string CleanShowName(string showName)
    {
        if (string.IsNullOrWhiteSpace(showName))
            return "";

        // Replace dots with spaces and clean up
        var cleaned = showName.Replace(".", " ")
                              .Replace("_", " ")
                              .Trim();

        // Remove extra spaces using source-generated regex
        cleaned = RegexUtils.WhitespacePattern().Replace(cleaned, " ");

        // TODO: use own title case method, put in stringUtils class
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleaned);
    }

    private static string CleanEpisodeTitle(string episodeTitle)
    {
        if (string.IsNullOrWhiteSpace(episodeTitle))
            return "";

        // Clean up episode title
        var cleaned = episodeTitle.Replace(".", " ")
                                 .Replace("_", " ")
                                 .Trim();

        // Remove extra spaces using source-generated regex
        cleaned = RegexUtils.WhitespacePattern().Replace(cleaned, " ");

        // Return empty string if it looks like quality info or metadata
        if (string.IsNullOrWhiteSpace(cleaned) ||
            cleaned.Length <= 2 ||
            RegexUtils.NumbersOnlyPattern().IsMatch(cleaned) ||
            RegexUtils.SingleLetterPattern().IsMatch(cleaned) || 
            RegexUtils.QualityTermsPattern().IsMatch(cleaned))  
            return "";

        return cleaned;
    }
}