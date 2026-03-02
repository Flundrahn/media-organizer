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

    public bool IsValidPathSegment(string pathSegment)
    {
        if (string.IsNullOrEmpty(pathSegment))
            return false;

        if (pathSegment.IndexOfAny(InvalidPathChars) > -1)
            return false;

        // ALL segments (both directory names and filenames) must follow filename character restrictions
        // because directory names also cannot contain filename invalid characters in Windows
        if (pathSegment.IndexOfAny(InvalidFileNameChars) > -1)
            return false;

        return true;
    }

    public bool AreValidPathSegments(IEnumerable<string> pathSegments)
    {
        return pathSegments.All(IsValidPathSegment);
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
