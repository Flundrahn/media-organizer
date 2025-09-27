using System.Text;

namespace MediaOrganizer.Output;

public class ConsoleOutputWriter : IOutputWriter
{
    private static bool _encodingSet = false;

    public ConsoleOutputWriter()
    {
        // Set UTF-8 encoding for better emoji support
        if (!_encodingSet)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                _encodingSet = true;
            }
            catch
            {
                // Continue with default encoding
            }
        }
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteError(string message)
    {
        var prefix = CanRenderEmoji() ? "❌" : "[ERROR]";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{prefix} {message}");
        Console.ResetColor();
    }

    public void WriteSuccess(string message)
    {
        var prefix = CanRenderEmoji() ? "✅" : "[SUCCESS]";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{prefix} {message}");
        Console.ResetColor();
    }

    public void WriteWarning(string message)
    {
        var prefix = CanRenderEmoji() ? "⚠️" : "[WARNING]";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{prefix} {message}");
        Console.ResetColor();
    }

    private static bool CanRenderEmoji()
    {
        // Simple check - if we're in Windows Terminal or a modern console, emojis should work
        // Otherwise, fall back to text prefixes
        return Environment.GetEnvironmentVariable("WT_SESSION") != null ||
               Console.OutputEncoding.EncodingName.Contains("UTF-8", StringComparison.OrdinalIgnoreCase);
    }
}