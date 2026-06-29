using Xunit;

namespace Bookstore.Tests;

public class BookstoreRunTests
{
    [Fact]
    public void Run_Stops_On_Quit_Command()
    {
        // Arrange
        var console = new ScriptedConsole(["quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains("> ", console.Output);
    }
}
