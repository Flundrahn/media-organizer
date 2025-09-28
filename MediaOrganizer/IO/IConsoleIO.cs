namespace MediaOrganizer.IO;

public interface IConsoleIO
{
    void WriteInformation(string message);
    void WriteLine(string message = "");
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteWarning(string message);
    void Write(string message);
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept = false);
}