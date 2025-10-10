using System.Text.RegularExpressions;

namespace MediaOrganizer.Utils;

/// <summary>
/// Utility class containing commonly used source-generated regex patterns
/// </summary>
public static partial class RegexUtils
{
    [GeneratedRegex(@"\s+", RegexOptions.None)]
    public static partial Regex WhitespacePattern();

    [GeneratedRegex(@"^\d+$", RegexOptions.None)]
    public static partial Regex NumbersOnlyPattern();

    [GeneratedRegex(@"^[A-Z]$", RegexOptions.None)]
    public static partial Regex SingleLetterPattern();

    [GeneratedRegex(@"^(480p|720p|1080p|1440p|2160p|4320p|4K|8K|UHD|REPACK)$", RegexOptions.IgnoreCase)]
    public static partial Regex QualityTermsPattern();
}