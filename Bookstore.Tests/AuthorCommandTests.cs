using Xunit;

namespace Bookstore.Tests;

public class AuthorCommandTests
{
    [Fact]
    public void Author_Model_Holds_All_Required_Fields()
    {
        // Arrange
        var sut = new global::Bookstore.Author
        {
            Id = 1,
            Name = "Frank Herbert",
            BornDate = new DateOnly(1920, 10, 8),
            Awards = ["Hugo", "Nebula"]
        };

        // Act
        var awardsCount = sut.Awards.Count;

        // Assert
        Assert.Equal(1, sut.Id);
        Assert.Equal("Frank Herbert", sut.Name);
        Assert.Equal(new DateOnly(1920, 10, 8), sut.BornDate);
        Assert.Equal(2, awardsCount);
    }

    [Fact]
    public void AddAuthor_Registers_Author_And_ShowAuthors_Displays_It()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 1 \"Frank Herbert\" 1920-10-08 \"Hugo,Nebula\"",
            "showAuthors",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Frank Herbert"));
        Assert.Contains(console.Output, line => line.Contains("1920-10-08"));
        Assert.Contains(console.Output, line => line.Contains("Hugo") && line.Contains("Nebula"));
    }

    [Fact]
    public void ShowAuthors_Includes_Unknown_Author()
    {
        // Arrange
        var console = new ScriptedConsole(["showAuthors", "quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("[0] Unknown Author"));
    }

    [Fact]
    public void AddAuthor_Rejects_Invalid_Date_Format()
    {
        // Arrange
        var console = new ScriptedConsole([
            "addAuthor 3 \"Author\" 1920/10/08 \"Award\"",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Invalid born date"));
    }
}
