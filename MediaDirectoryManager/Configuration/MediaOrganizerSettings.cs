using MediaDirectoryManager.Validations;

namespace MediaDirectoryManager.Configuration;

/// <summary>
/// Configuration settings for the Media Organizer application
/// </summary>
public class MediaOrganizerSettings
{
    private List<string> _validationErrors = [];
    private FileSystemValidations? _validator;

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

    public ICollection<string> GetValidationErrors() => _validationErrors;

    public void SetValidator(FileSystemValidations validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    public bool IsValid()
    {
        if (_validator is null)
        {
            throw new InvalidOperationException("Validator must be set.");
        }

        _validationErrors.Clear();
        var isValid = true;

        if (string.IsNullOrWhiteSpace(SourceDirectory))
        {
            _validationErrors.Add("SourceDirectory is required");
            isValid = false;
        }
        else if (!_validator.DirectoryExists(SourceDirectory))
        {
            _validationErrors.Add("SourceDirectory does not exist");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(DestinationDirectory))
        {
            _validationErrors.Add("DestinationDirectory is required");
            isValid = false;
        }
        else if (!_validator.DirectoryIsWriteable(DestinationDirectory))
        {
            _validationErrors.Add("DestinationDirectory is not writable or cannot be created");
            isValid = false;
        }

        return isValid;
    }
}