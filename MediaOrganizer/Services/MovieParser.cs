using System.IO.Abstractions;
using System.Text.RegularExpressions;
using MediaOrganizer.Models;
using MediaOrganizer.Utils;

namespace MediaOrganizer.Services;

public partial class MovieParser : IMediaFileParser
{
    [GeneratedRegex(@"^(?<title>.+?)\s+(?<year>\d{4})\s+(?:UNRATED\s+)?(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex TitleYearQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<title>.+?)\s+\((?<year>\d{4})\)\s+\((?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex TitleParenthesesYearParenthesesQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<title>(?:[A-Za-z0-9]+\.)*[A-Za-z0-9]+)\.(?<year>\d{4})\.(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex TitleYearQualityWithDotsPattern();

    [GeneratedRegex(@"^(?<title>.+?)\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex TitleQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<title>.+?)\s+(?<year>\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex TitleYearWithSpacesPattern();

    [GeneratedRegex(@"^(?<title>(?:[A-Za-z0-9]+\.)*[A-Za-z0-9]+)\.(?<year>\d{4})\.(?:[A-Za-z0-9\-\.]+\.)*(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex TitleYearExtrasQualityWithDotsPattern();

    [GeneratedRegex(@"^(?<title>(?:[A-Za-z]+\s+){2,}[A-Za-z]+)\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)", RegexOptions.IgnoreCase)]
    private static partial Regex LongTitleQualityWithSpacesPattern();

    [GeneratedRegex(@"^(?<title>[A-Za-z][A-ZaZ\s]*[A-ZaZe])(?:\s+\d{3,4}p|\s+4K|\s+UHD|\s+HDR|$)", RegexOptions.IgnoreCase)]
    private static partial Regex TitleOnlyPattern();

    private static readonly Regex[] AllPatterns = [
        TitleYearQualityWithSpacesPattern(),
        TitleParenthesesYearParenthesesQualityWithSpacesPattern(),
        TitleYearQualityWithDotsPattern(),
        TitleYearExtrasQualityWithDotsPattern(),
        LongTitleQualityWithSpacesPattern(),
        TitleQualityWithSpacesPattern(),
        TitleYearWithSpacesPattern(),
        TitleOnlyPattern()
    ];

    public bool CanParse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        foreach (var pattern in AllPatterns)
        {
            if (pattern.IsMatch(nameWithoutExtension))
                return true;
        }

        return false;
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

            string title = ExtractAndCleanTitle(match.Groups["title"].Value);
            int? year = match.Groups["year"].Success
                ? int.Parse(match.Groups["year"].Value)
                : null;
            string quality = match.Groups["quality"].Success
                ? match.Groups["quality"].Value
                : string.Empty;

            return new Movie(fileInfo)
            {
                Title = title,
                Year = year,
                Quality = quality
            };
        }

        return new Movie(fileInfo);
    }

    private static string ExtractAndCleanTitle(string rawTitle)
    {
        if (string.IsNullOrWhiteSpace(rawTitle))
            return string.Empty;

        // Replace dots with spaces for dotted titles
        var title = rawTitle.Replace('.', ' ');

        // Clean up extra whitespace using source-generated regex
        title = RegexUtils.WhitespacePattern().Replace(title, " ").Trim();

        // Capitalize properly (basic title case)
        // TODO: make this field
        return new StringUtils().ToTitleCase(title);
    }
}