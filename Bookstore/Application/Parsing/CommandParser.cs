using System.Text.RegularExpressions;

namespace Bookstore.Application.Parsing;

public class CommandParser : ICommandParser
{
    public IReadOnlyList<string> ParseArguments(string commandLine)
    {
        return Regex.Matches(commandLine ?? string.Empty, "\"([^\"]*)\"|(\\S+)")
            .Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value)
            .ToList();
    }
}
