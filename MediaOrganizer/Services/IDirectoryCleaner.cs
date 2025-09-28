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
}