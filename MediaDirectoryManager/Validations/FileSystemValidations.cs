using System.IO.Abstractions;

namespace MediaDirectoryManager.Validations;

public class FileSystemValidator
{
    private readonly IFileSystem _fileSystem;

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
        return path.IndexOfAny(_fileSystem.Path.GetInvalidPathChars()) < 0;
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