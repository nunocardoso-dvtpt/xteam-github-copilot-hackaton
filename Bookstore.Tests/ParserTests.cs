using Xunit;

namespace Bookstore.Tests;

public class ParserTests
{
    [Fact]
    public void CommandParser_Parses_Quoted_And_Unquoted_Tokens()
    {
        // Arrange
        var parser = new global::Bookstore.Application.Parsing.CommandParser();

        // Act
        var tokens = parser.ParseArguments("\"Dune Messiah\" 10 \"Science Fiction\" \"Classic sequel\"");

        // Assert
        Assert.Equal(4, tokens.Count);
        Assert.Equal("Dune Messiah", tokens[0]);
        Assert.Equal("10", tokens[1]);
    }
}
