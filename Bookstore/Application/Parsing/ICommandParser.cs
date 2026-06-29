namespace Bookstore.Application.Parsing;

public interface ICommandParser
{
    IReadOnlyList<string> ParseArguments(string commandLine);
}
