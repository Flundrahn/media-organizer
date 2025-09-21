namespace MediaDirectoryManager.Configuration;

/// <summary>
/// Configuration settings for the Media Organizer application
/// </summary>
public class MediaOrganizerSettings
{
    public const string SectionName = "MediaOrganizer";

    /// <summary>
    /// The source directory to scan for media files
    /// </summary>
    public string SourceDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The destination directory where organized files will be placed
    /// </summary>
    public string DestinationDirectory { get; set; } = string.Empty;

    /// <summary>
    /// When true, only shows what would be done without actually moving files
    /// </summary>
    public bool DryRun { get; set; } = true;

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SourceDirectory) && 
               !string.IsNullOrWhiteSpace(DestinationDirectory);
    }

    /// <summary>
    /// Gets validation error messages
    /// </summary>
    /// <returns>List of validation errors</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SourceDirectory))
            errors.Add("SourceDirectory is required");

        if (string.IsNullOrWhiteSpace(DestinationDirectory))
            errors.Add("DestinationDirectory is required");

        return errors;
    }
}