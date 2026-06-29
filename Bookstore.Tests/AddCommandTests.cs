using Xunit;

namespace Bookstore.Tests;

public class AddCommandTests
{
    [Fact]
    public void Add_With_Quoted_Multiword_Arguments_Shows_Book()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Frank Herbert\" 1920-10-08 \"Hugo,Nebula\"",
            "add \"Dune Messiah\" 1 \"Science Fiction\" \"Classic sequel\"",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Dune Messiah"));
        Assert.Contains(console.Output, line => line.Contains("Frank Herbert"));
    }

    [Fact]
    public void Add_Duplicate_Title_Is_Rejected_Case_Insensitive()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Frank Herbert\" 1920-10-08 \"Hugo\"",
            "add \"Dune\" 1 \"Science Fiction\" \"One\"",
            "add \"dune\" 1 \"Science Fiction\" \"Two\"",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Duplicate book with the name \"dune\"."));
    }

    [Fact]
    public void Add_With_Invalid_Category_Prints_Validation()
    {
        // Arrange
        var console = new ScriptedConsole([
            "add \"Dune\" 0 \"Unknown\" \"Desc\"",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Invalid category."));
    }

    [Fact]
    public void Add_With_Missing_Arguments_Prints_Usage()
    {
        // Arrange
        var console = new ScriptedConsole([
            "add \"Dune\" 1",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Usage: add"));
    }

    [Fact]
    public void Show_With_No_Books_Prints_Empty_State()
    {
        // Arrange
        var console = new ScriptedConsole([
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line == "No books registered.");
    }

    [Fact]
    public void Show_Prints_Each_Book_Once()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Author A\" 1980-01-01 \"Award\"",
            "addAuthor 2 \"Author B\" 1985-01-01 \"Award\"",
            "add \"Book One\" 1 \"Fiction\" \"Desc\"",
            "add \"Book Two\" 2 \"Fantasy\" \"Desc\"",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        var bookOneLines = console.Output.Count(line => line.Contains("Book One"));
        var bookTwoLines = console.Output.Count(line => line.Contains("Book Two"));
        Assert.Equal(1, bookOneLines);
        Assert.Equal(1, bookTwoLines);
    }

    [Fact]
    public void Add_With_Known_AuthorId_Shows_Resolved_Author_Name()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 10 \"Frank Herbert\" 1920-10-08 \"Hugo\"",
            "add \"Dune\" 10 \"Science Fiction\" \"Classic\"",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Dune") && line.Contains("Frank Herbert"));
    }

    [Fact]
    public void Add_With_Unknown_AuthorId_Falls_Back_To_Unknown_Author()
    {
        // Arrange
        var console = new ScriptedConsole([
            "add \"Book X\" 999 \"Fiction\" \"Desc\"",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Book X") && line.Contains("Unknown Author"));
    }
}
