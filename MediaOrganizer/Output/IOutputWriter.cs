namespace MediaOrganizer.Output;

public interface IOutputWriter
{
    void WriteLine(string message);
    void WriteError(string message);
    void WriteSuccess(string message);
    void WriteWarning(string message);
}