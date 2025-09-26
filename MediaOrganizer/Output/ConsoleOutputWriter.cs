namespace MediaOrganizer.Output;

public class ConsoleOutputWriter : IOutputWriter
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteError(string message)
    {
        Console.WriteLine($"❌ {message}");
    }

    public void WriteSuccess(string message)
    {
        Console.WriteLine($"✅ {message}");
    }

    public void WriteWarning(string message)
    {
        Console.WriteLine($"⚠️ {message}");
    }
}