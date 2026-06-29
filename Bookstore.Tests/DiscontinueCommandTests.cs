using Xunit;

namespace Bookstore.Tests;

public class DiscontinueCommandTests
{
    [Fact]
    public void DiscontinueBook_Invalid_Id_Prints_Validation_Message()
    {
        // Arrange
        var console = new ScriptedConsole([
            "discontinueBook abc",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Invalid book ID"));
    }

    [Fact]
    public void DiscontinueBook_Missing_Id_Prints_Validation_Message()
    {
        // Arrange
        var console = new ScriptedConsole([
            "discontinueBook",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Invalid book ID"));
    }

    [Fact]
    public void DiscontinueBook_NonExisting_Id_Prints_NotFound()
    {
        // Arrange
        var console = new ScriptedConsole([
            "discontinueBook 99",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Book with ID 99 was not found."));
    }

    [Fact]
    public void DiscontinueBook_Existing_Id_Marks_Book_Discontinued()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Author A\" 1980-01-01 \"Award\"",
            "add \"Book 1\" 1 \"Fiction\" \"Desc\"",
            "discontinueBook 1",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Discontinued: 'Book 1' (ID: 1)"));
        Assert.Contains(console.Output, line => line.Contains("Book 1") && line.Contains("(discontinued)"));
    }

    [Fact]
    public void DiscontinueAuthor_Marks_All_Active_Books_For_Author()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Author A\" 1980-01-01 \"Award\"",
            "addAuthor 2 \"Author B\" 1985-01-01 \"Award\"",
            "add \"Book 1\" 1 \"Fiction\" \"Desc\"",
            "add \"Book 2\" 1 \"Fantasy\" \"Desc\"",
            "add \"Book 3\" 2 \"Romance\" \"Desc\"",
            "discontinueAuthor Author A",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        var discontinuedCount = console.Output.Count(line => line.Contains("(discontinued)"));
        Assert.Equal(2, discontinuedCount);
        Assert.Contains(console.Output, line => line.Contains("Book 3") && !line.Contains("(discontinued)"));
    }

    [Fact]
    public void DiscontinueAuthor_Empty_Author_Prints_Validation_Message()
    {
        // Arrange
        var console = new ScriptedConsole([
            "discontinueAuthor",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Please provide an author name."));
    }

    [Fact]
    public void DiscontinueAuthor_NotFound_Prints_Message()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Author A\" 1980-01-01 \"Award\"",
            "add \"Book 1\" 1 \"Fiction\" \"Desc\"",
            "discontinueAuthor Author B",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Could not find any active books by author: Author B"));
    }
}
