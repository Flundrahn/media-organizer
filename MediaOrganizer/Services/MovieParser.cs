using System.IO.Abstractions;
using System.Text.RegularExpressions;
using MediaOrganizer.Models;

namespace MediaOrganizer.Services;

public class MovieParser : IMediaFileParser
{
    // Example: "Grandmas Boy 2006 UNRATED 1080p BluRay HEVC x265 5.1 BONE.mkv"
    private static readonly Regex MovieWithYearAndQualityPattern = new(
        @"^(?<title>.+?)\s+(?<year>\d{4})\s+(?:UNRATED\s+)?(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\s+.+?)?$",
        RegexOptions.IgnoreCase);

    // Example: "A Brilliant Young Mind (2014) (1080p BluRay x265 10bit Tigole).mkv"
    private static readonly Regex MovieWithYearInParenthesesPattern = new(
        @"^(?<title>.+?)\s+\((?<year>\d{4})\)\s+\((?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\s+[^)]*)?",
        RegexOptions.IgnoreCase);

    // Example: "Clash.Of.The.Titans.1981.1080p.BluRay.x264-[YTS.AM].mp4"
    private static readonly Regex MovieWithDotsPattern = new(
        @"^(?<title>(?:[A-Za-z0-9]+\.)*[A-Za-z0-9]+)\.(?<year>\d{4})\.(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\..*)?$",
        RegexOptions.IgnoreCase);

    // Example: "Interstellar 1080p.mkv"
    private static readonly Regex MovieSimpleWithQualityPattern = new(
        @"^(?<title>.+?)\s+(?<quality>480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD)(?:\.|$)",
        RegexOptions.IgnoreCase);

    // Example: "Superman 2025"
    private static readonly Regex MovieWithYearPattern = new(
        @"^(?<title>.+?)\s+(?<year>\d{4})(?:\s|$)",
        RegexOptions.IgnoreCase);

    // Example: "Samsara 1080p.mkv"
    private static readonly Regex MovieSimpleTitlePattern = new(
        @"^(?<title>[A-Za-z][A-Za-z\s]+[A-Za-z])(?:\s+\d{3,4}p|\s+4K|\s+UHD|\s+HDR|$)",
        RegexOptions.IgnoreCase);

    // Patterns to exclude files that are clearly not movies (bonus content, etc.)
    private static readonly Regex[] ExclusionPatterns = [
        new Regex(@"^(?:deleted|bonus|behind|making|trailer|teaser|concept|editing|legacy|internet|sample)", RegexOptions.IgnoreCase),
        new Regex(@"\b(?:deleted\s+scenes|behind\s+the\s+score|bonus\s+features|making\s+of)\b", RegexOptions.IgnoreCase)
    ];

    private static readonly Regex[] AllPatterns = [
        MovieWithYearAndQualityPattern,
        MovieWithYearInParenthesesPattern,
        MovieWithDotsPattern,
        MovieSimpleWithQualityPattern,
        MovieWithYearPattern,
        MovieSimpleTitlePattern
    ];

    public bool CanParse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        foreach (var exclusionPattern in ExclusionPatterns)
        {
            if (exclusionPattern.IsMatch(nameWithoutExtension))
                return false;
        }

        foreach (var pattern in AllPatterns)
        {
            if (pattern.IsMatch(nameWithoutExtension))
                return true;
        }

        return false;
    }

    public IMediaFile Parse(IFileInfo fileInfo)
    {
        var movie = new Movie(fileInfo);
        var filename = fileInfo.Name;
        
        // Remove file extension for parsing
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        bool matched = false;
        foreach (var pattern in AllPatterns)
        {
            var match = pattern.Match(nameWithoutExtension);
            if (match.Success)
            {
                // Extract title and clean it up
                var title = ExtractAndCleanTitle(match.Groups["title"].Value);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    movie.Title = title;
                }

                // Extract year if present
                if (match.Groups["year"].Success && int.TryParse(match.Groups["year"].Value, out int year))
                {
                    movie.Year = year;
                }

                // Extract quality if present
                if (match.Groups["quality"].Success)
                {
                    movie.Quality = match.Groups["quality"].Value;
                }

                matched = true;
                break; // Use the first successful match
            }
        }
        // NOTE: is this the type of behavior we want?a or better to throw considering we have canparse?
        //  just need to return anything with invalid media file tbh

        // Fallback for unparseable files
        if (!matched)
        {
            movie.Title = filename; // Use full filename including extension
        }

        return movie;
    }

    private static string ExtractAndCleanTitle(string rawTitle)
    {
        if (string.IsNullOrWhiteSpace(rawTitle))
            return string.Empty;

        // Replace dots with spaces for dotted titles
        var title = rawTitle.Replace('.', ' ');
        
        // Clean up extra whitespace
        title = Regex.Replace(title, @"\s+", " ").Trim();
        
        // Capitalize properly (basic title case)
        return ToTitleCase(title);
    }

    private static string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCaseWords = new List<string>();

        foreach (var word in words)
        {
            if (word.Length == 1)
            {
                titleCaseWords.Add(word.ToUpper());
            }
            else if (IsSmallWord(word.ToLower()))
            {
                titleCaseWords.Add(word.ToLower());
            }
            else
            {
                titleCaseWords.Add(char.ToUpper(word[0]) + word.Substring(1).ToLower());
            }
        }

        // Always capitalize the first word
        if (titleCaseWords.Count > 0)
        {
            titleCaseWords[0] = CapitalizeFirstWord(titleCaseWords[0]);
        }

        return string.Join(' ', titleCaseWords);
    }

    private static bool IsSmallWord(string word)
    {
        var smallWords = new[] { "a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        return smallWords.Contains(word);
    }

    private static string CapitalizeFirstWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;
        
        return char.ToUpper(word[0]) + word.Substring(1);
    }
}