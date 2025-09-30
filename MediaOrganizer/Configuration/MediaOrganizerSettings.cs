using System.Text.RegularExpressions;
using MediaOrganizer.Models;
using MediaOrganizer.Validations;

namespace MediaOrganizer.Configuration;

/// <summary>
/// Configuration settings for the Media Organizer application
/// </summary>
public class MediaOrganizerSettings
{
    private List<string> _validationErrors = [];
    private FileSystemValidator? _validator;
    private string _sourceDirectory = string.Empty;
    private string _destinationDirectory = string.Empty;
    private string _tvShowSourceDirectory = string.Empty;
    private string _tvShowDestinationDirectory = string.Empty;
    private string _movieSourceDirectory = string.Empty;
    private string _movieDestinationDirectory = string.Empty;

    public const string SectionName = "MediaOrganizer";

    // TODO: clean up "legacy" directories

    /// <summary>
    /// The source directory to scan for media files
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string SourceDirectory
    {
        get => _sourceDirectory;
        set => _sourceDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The destination directory where organized files will be placed
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string DestinationDirectory 
    { 
        get => _destinationDirectory;
        set => _destinationDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The source directory to scan for TV show files
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string TvShowSourceDirectory 
    { 
        get => _tvShowSourceDirectory;
        set => _tvShowSourceDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The destination directory where organized TV show files will be placed
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string TvShowDestinationDirectory 
    { 
        get => _tvShowDestinationDirectory;
        set => _tvShowDestinationDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The source directory to scan for movie files
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string MovieSourceDirectory 
    { 
        get => _movieSourceDirectory;
        set => _movieSourceDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// The destination directory where organized movie files will be placed
    /// Automatically converts relative paths to absolute paths
    /// </summary>
    public string MovieDestinationDirectory 
    { 
        get => _movieDestinationDirectory;
        set => _movieDestinationDirectory = string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
    }

    /// <summary>
    /// When true, only shows what would be done without actually moving files
    /// </summary>
    public bool DryRun { get; set; } = true;

    /// <summary>
    /// When true, includes all subdirectories when searching for media files
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;

    /// <summary>
    /// The template used to generate relative paths for TV show episodes.
    /// Supports placeholders: {TvShowName}, {Season}, {Episode}, {Title}, {Year}
    /// The file extension from the original file is automatically appended.
    /// </summary>
    public string TvShowPathTemplate { get; set; } = string.Empty;

    /// <summary>
    /// The template used to generate relative paths for movies.
    /// Supports placeholders: {Title}, {Year}, {Quality}
    /// The file extension from the original file is automatically appended.
    /// </summary>
    public string MoviePathTemplate { get; set; } = string.Empty;

    /// <summary>
    /// List of video file extensions to include when scanning for media files.
    /// Extensions should include the dot (e.g., ".mp4", ".avi").
    /// Case-insensitive matching is used.
    /// </summary>
    public List<string> VideoFileExtensions { get; set; } = new List<string>();

    /// <summary>
    /// When true, automatically removes empty directories after finishing OrganizeFiles operation.
    /// Only removes directories that become empty as a result of the file organization process.
    /// Note: Directory cleanup can also be run manually through the application menu.
    /// </summary>
    public bool AutoCleanupEmptyDirectories { get; set; } = false;

    public ICollection<string> GetValidationErrors() => _validationErrors;

    // Set manually since options need empty ctor
    public void SetValidator(FileSystemValidator validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    
    // NOTE: Possibly extract validation to own class
    public bool IsValid()
    {
        if (_validator is null)
        {
            throw new InvalidOperationException("Validator must be set.");
        }

        _validationErrors.Clear();
        var isValid = true;

        if (string.IsNullOrWhiteSpace(TvShowSourceDirectory))
        {
            _validationErrors.Add("TvShowSourceDirectory is required");
            isValid = false;
        }
        else if (!_validator.DirectoryExists(TvShowSourceDirectory))
        {
            _validationErrors.Add($"TvShowSourceDirectory does not exist: {TvShowSourceDirectory}");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(TvShowDestinationDirectory))
        {
            _validationErrors.Add("TvShowDestinationDirectory is required");
            isValid = false;
        }
        else if (!_validator.DirectoryIsWriteable(TvShowDestinationDirectory))
        {
            _validationErrors.Add("TvShowDestinationDirectory is not writable or cannot be created");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(MovieSourceDirectory))
        {
            _validationErrors.Add("MovieSourceDirectory is required");
            isValid = false;
        }
        else if (!_validator.DirectoryExists(MovieSourceDirectory))
        {
            _validationErrors.Add($"MovieSourceDirectory does not exist: {MovieSourceDirectory}");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(MovieDestinationDirectory))
        {
            _validationErrors.Add("MovieDestinationDirectory is required");
            isValid = false;
        }
        else if (!_validator.DirectoryIsWriteable(MovieDestinationDirectory))
        {
            _validationErrors.Add("MovieDestinationDirectory is not writable or cannot be created");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(TvShowPathTemplate))
        {
            _validationErrors.Add("TvShowPathTemplate is required");
            isValid = false;
        }
        else
        {
            // Validate placeholders in the template using the valid placeholders from TvShowEpisode
            var placeholderMatches = Regex.Matches(TvShowPathTemplate, @"\{([^}:]*)[^}]*\}");

            foreach (Match match in placeholderMatches)
            {
                var placeholderName = match.Groups[1].Value;
                if (!TvShowEpisode.ValidPlaceholders.Contains(placeholderName))
                {
                    _validationErrors.Add($"TvShowPathTemplate contains invalid placeholder: {{{placeholderName}}}");
                    isValid = false;
                }
            }

            // Validate path characters by removing placeholders and checking segments
            var templateWithoutPlaceholders = Regex.Replace(
                TvShowPathTemplate,
                @"\{[^}]*\}",
                "X"); // Replace placeholders with a safe character

            var pathParts = templateWithoutPlaceholders.Split('/', '\\');
            if (_validator != null && !_validator.AreValidPathSegments(pathParts))
            {
                _validationErrors.Add("TvShowPathTemplate contains invalid path characters");
                isValid = false;
            }
        }

        if (string.IsNullOrWhiteSpace(MoviePathTemplate))
        {
            _validationErrors.Add("MoviePathTemplate is required");
            isValid = false;
        }
        else
        {
            // Validate placeholders in the template using the valid placeholders from Movie
            var placeholderMatches = Regex.Matches(MoviePathTemplate, @"\{([^}:]*)[^}]*\}");

            foreach (Match match in placeholderMatches)
            {
                var placeholderName = match.Groups[1].Value;
                if (!Movie.ValidPlaceholders.Contains(placeholderName))
                {
                    _validationErrors.Add($"MoviePathTemplate contains invalid placeholder: {{{placeholderName}}}");
                    isValid = false;
                }
            }

            // Validate path characters by removing placeholders and checking segments
            var movieTemplateWithoutPlaceholders = Regex.Replace(
                MoviePathTemplate,
                @"\{[^}]*\}",
                "X"); // Replace placeholders with a safe character

            var moviePathParts = movieTemplateWithoutPlaceholders.Split('/', '\\');
            if (_validator != null && !_validator.AreValidPathSegments(moviePathParts))
            {
                _validationErrors.Add("MoviePathTemplate contains invalid path characters");
                isValid = false;
            }
        }

        // Validate video file extensions
        if (VideoFileExtensions == null || VideoFileExtensions.Count == 0)
        {
            _validationErrors.Add("VideoFileExtensions must contain at least one extension");
            isValid = false;
        }
        else
        {
            foreach (var extension in VideoFileExtensions)
            {
                if (string.IsNullOrWhiteSpace(extension))
                {
                    _validationErrors.Add("VideoFileExtensions cannot contain empty or whitespace extensions");
                    isValid = false;
                    break;
                }

                if (!extension.StartsWith('.'))
                {
                    _validationErrors.Add($"VideoFileExtensions must start with a dot: '{extension}'");
                    isValid = false;
                }

                if (extension.Length < 2)
                {
                    _validationErrors.Add($"VideoFileExtensions must have at least one character after the dot: '{extension}'");
                    isValid = false;
                }

                if (_validator != null && !_validator.IsValidPathSegment(extension))
                {
                    _validationErrors.Add($"VideoFileExtensions contains invalid characters: '{extension}'");
                    isValid = false;
                }
            }
        }

        return isValid;
    }
}