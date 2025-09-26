using System.IO.Abstractions;

namespace MediaOrganizer.Validations;

public class FileSystemValidator
{
    private readonly IFileSystem _fileSystem;
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public FileSystemValidator(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public bool DirectoryExists(string path)
    {
        return _fileSystem.Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return _fileSystem.File.Exists(path);
    }

    public bool HasValidPath(string path)
    {
        return path.IndexOfAny(InvalidPathChars) < 0;
    }

    /// <summary>
    /// Validates that a path segment (directory or filename) contains only valid characters
    /// </summary>
    /// <param name="pathSegment">The path segment to validate</param>
    /// <returns>True if the path segment is valid</returns>
    public bool IsValidPathSegment(string pathSegment)
    {
        if (string.IsNullOrEmpty(pathSegment))
            return false;

        // Check for invalid path characters
        if (pathSegment.IndexOfAny(InvalidPathChars) >= 0)
            return false;

        // ALL segments (both directory names and filenames) must follow filename character restrictions
        // because directory names also cannot contain filename invalid characters in Windows
        if (pathSegment.IndexOfAny(InvalidFileNameChars) >= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Validates multiple path segments at once
    /// </summary>
    /// <param name="pathSegments">The path segments to validate</param>
    /// <returns>True if all path segments are valid</returns>
    public bool AreValidPathSegments(IEnumerable<string> pathSegments)
    {
        return pathSegments.All(segment => 
            string.IsNullOrEmpty(segment) || IsValidPathSegment(segment));
    }

    public bool DirectoryIsWriteable(string path)
    {
        bool wasPathDirectoryCreated = false;
        bool wasTestFileCreated = false;
        string testFilePath = string.Empty;

        try
        {
            if (!_fileSystem.Directory.Exists(path))
            {
                _fileSystem.Directory.CreateDirectory(path);
                wasPathDirectoryCreated = true;
            }

            testFilePath = Path.Combine(path, Path.GetRandomFileName());
            _fileSystem.File.WriteAllText(testFilePath, "test");
            wasTestFileCreated = true;

            return true;
        }
        catch
        {
            // Do nothing
            return false;
        }
        finally
        {
            if (wasTestFileCreated)
            {
                _fileSystem.File.Delete(testFilePath);
            }

            if (wasPathDirectoryCreated)
            {
                if (!_fileSystem.Directory.EnumerateFileSystemEntries(path).Any())
                {
                    _fileSystem.Directory.Delete(path);
                }
            }
        }
    }
}