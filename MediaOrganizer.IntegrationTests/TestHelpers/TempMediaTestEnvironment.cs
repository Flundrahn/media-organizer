namespace MediaOrganizer.IntegrationTests.TestHelpers;

/// <summary>
/// Helper that creates a temporary media test environment with Source and Destination folders
/// and cleans it up when disposed.
/// </summary>
public sealed class TempMediaTestEnvironment : IDisposable
{
    private bool _disposed;

    public string TempDirectoryRoot { get; }
    public string SourceDirectory { get; }
    public string DestinationDirectory { get; }

    public TempMediaTestEnvironment()
    {
        TempDirectoryRoot = Path.Combine(Path.GetTempPath(), $"media-organizer-integration-{Guid.NewGuid():N}");
        SourceDirectory = Path.Combine(TempDirectoryRoot, "source");
        DestinationDirectory = Path.Combine(TempDirectoryRoot, "media");

        Directory.CreateDirectory(TempDirectoryRoot);
        Directory.CreateDirectory(SourceDirectory);
        Directory.CreateDirectory(DestinationDirectory);
    }

    /// <summary>
    /// Creates a test file at the specified path relative to the SourceDirectory.
    /// Creates any necessary parent directories.
    /// </summary>
    /// <param name="relativePath">Path relative to SourceDirectory where the file should be created (e.g., "nested/episode1.mkv")</param>
    /// <param name="content">Content to write to the file</param>
    /// <returns>The absolute path to the created file</returns>
    public string CreateFile(string relativePath, string content = "dummy")
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TempMediaTestEnvironment));
        }

        var absoluteFilePath = Path.Combine(SourceDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var parentDirectoryPath = Path.GetDirectoryName(absoluteFilePath) ?? SourceDirectory;
        if (!Directory.Exists(parentDirectoryPath))
        {
            Directory.CreateDirectory(parentDirectoryPath);
        }
        File.WriteAllText(absoluteFilePath, content);
        return absoluteFilePath;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            try
            {
                if (Directory.Exists(TempDirectoryRoot))
                {
                    Directory.Delete(TempDirectoryRoot, true);
                }
            }
            catch
            {
                // best-effort cleanup of temp directory
            }
        }

        _disposed = true;
    }
}
