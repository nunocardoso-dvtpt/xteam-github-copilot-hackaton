using Xunit;

namespace Bookstore.Tests;

public class HelpAndErrorCommandTests
{
    [Fact]
    public void Help_Prints_Supported_Commands()
    {
        // Arrange
        var console = new ScriptedConsole([
            "help",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("show"));
        Assert.Contains(console.Output, line => line.Contains("showAuthors"));
        Assert.Contains(console.Output, line => line.Contains("add \"<title>\" <authorId> \"<category>\" \"<description>\""));
        Assert.Contains(console.Output, line => line.Contains("addAuthor <id> \"<name>\" <YYYY-MM-DD> \"<award1,award2,...>\""));
        Assert.Contains(console.Output, line => line.Contains("discontinueBook <book ID>"));
        Assert.Contains(console.Output, line => line.Contains("discontinueAuthor <author name>"));
        Assert.Contains(console.Output, line => line.Contains("help"));
        Assert.Contains(console.Output, line => line.Contains("quit"));
    }

    [Fact]
    public void Unknown_Command_Prints_Error()
    {
        // Arrange
        var console = new ScriptedConsole([
            "unknownCommand",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("I don't know what the command \"unknownCommand\" is."));
    }

    [Fact]
    public void Empty_Command_Prints_Hint_Message()
    {
        // Arrange
        var console = new ScriptedConsole([
            "",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Please provide a command. Type \"help\" to see available commands."));
    }

    [Fact]
    public void Refactored_Program_Help_And_ShowAuthors_Behavior_Remains_Stable()
    {
        // Arrange
        var console = new ScriptedConsole(["help", "showAuthors", "quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("showAuthors"));
        Assert.Contains(console.Output, line => line.Contains("[0] Unknown Author"));
    }
}
