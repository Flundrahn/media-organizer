using MediaOrganizer.IO;

namespace MediaOrganizer.IntegrationTests.TestHelpers;

/// <summary>
/// Mock console implementation for testing that captures output and provides simulated input
/// </summary>
public class MockConsoleIO : IConsoleIO
{
    private readonly Queue<ConsoleKeyInfo> _keyInputs = new();
    private readonly Queue<string> _lineInputs = new();
    private readonly List<string> _output = new();

    /// <summary>
    /// All output written to the console
    /// </summary>
    public IReadOnlyList<string> Output => _output.AsReadOnly();

    /// <summary>
    /// Queue up key presses to be returned by ReadKey calls
    /// </summary>
    public void QueueKeyInput(ConsoleKey key, char keyChar = '\0', bool shift = false, bool alt = false, bool control = false)
    {
        _keyInputs.Enqueue(new ConsoleKeyInfo(keyChar, key, shift, alt, control));
    }

    /// <summary>
    /// Queue up line input to be returned by ReadLine calls
    /// </summary>
    public void QueueLineInput(string input)
    {
        _lineInputs.Enqueue(input);
    }

    public void WriteInformation(string message)
    {
        _output.Add($"[INFO] {message}");
    }

    public void WriteLine(string message = "")
    {
        _output.Add(message);
    }

    public void WriteSuccess(string message)
    {
        _output.Add($"[SUCCESS] {message}");
    }

    public void WriteError(string message)
    {
        _output.Add($"[ERROR] {message}");
    }

    public void WriteWarning(string message)
    {
        _output.Add($"[WARNING] {message}");
    }

    public void Write(string message)
    {
        _output.Add(message);
    }

    public string? ReadLine()
    {
        return _lineInputs.Count > 0 ? _lineInputs.Dequeue() : null;
    }

    public ConsoleKeyInfo ReadKey(bool intercept = false)
    {
        if (_keyInputs.Count > 0)
        {
            return _keyInputs.Dequeue();
        }
        
        // Default to 'Q' (quit) if no more inputs are queued
        return new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false);
    }
}