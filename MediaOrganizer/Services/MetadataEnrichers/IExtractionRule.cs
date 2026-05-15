using System.Diagnostics.CodeAnalysis;

namespace MediaOrganizer.Services.MetadataEnrichers;

/// <summary>
/// A rule that attempts to extract media file information from file paths.
/// </summary>
public interface IExtractionRule
{
    string Name { get; }

    bool TryExtract(string filePath,[NotNullWhen(true)] out RuleMatch? match);
}
