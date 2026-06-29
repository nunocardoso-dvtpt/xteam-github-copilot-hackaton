using Tasks;

namespace Bookstore.Tests;

public sealed class ScriptedConsole : IConsole
{
    private readonly Queue<string> lines;

    public List<string> Output { get; } = [];

    public ScriptedConsole(IEnumerable<string> scriptedInput)
    {
        lines = new Queue<string>(scriptedInput);
    }

    public string ReadLine() => lines.Count > 0 ? lines.Dequeue() : "quit";

    public void Write(string format, params object[] args) =>
        Output.Add(string.Format(format, args));

    public void WriteLine(string format, params object[] args) =>
        Output.Add(string.Format(format, args));

    public void WriteLine() => Output.Add(string.Empty);
}
