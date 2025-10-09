namespace MediaOrganizer.Services;

/// <summary>
/// Interface for cleaning up empty directories in source and destination paths
/// </summary>
public interface IDirectoryCleaner
{
    /// <summary>
    /// Cleans up empty directories in the source and destination directories and their subdirectories
    /// </summary>
    void CleanEmptyDirectories();

    /// <summary>
    /// Cleans up empty directories in specific source and destination directories
    /// </summary>
    /// <param name="sourceDirectory">The source directory to clean</param>
    /// <param name="destinationDirectory">The destination directory to clean</param>
    /// <exception cref="ArgumentException">Thrown when directories are not within configured media directories</exception>
    void CleanEmptyDirectories(string sourceDirectory, string destinationDirectory);
}