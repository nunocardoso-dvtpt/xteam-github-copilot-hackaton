using Xunit;

namespace Bookstore.Tests;

public class RepositoryTests
{
    [Fact]
    public void InMemoryAuthorRepository_Seeds_Unknown_Author()
    {
        // Arrange
        var repo = new global::Bookstore.Infrastructure.InMemory.InMemoryAuthorRepository();

        // Act
        var unknown = repo.GetById(0);

        // Assert
        Assert.NotNull(unknown);
        Assert.Equal("Unknown Author", unknown!.Name);
    }

    [Fact]
    public void InMemoryBookRepository_Add_Stores_Book()
    {
        // Arrange
        var repo = new global::Bookstore.Infrastructure.InMemory.InMemoryBookRepository();
        var book = new global::Bookstore.Book
        {
            Id = 1,
            Title = "Dune",
            Author = "Frank Herbert",
            CategoryId = 3,
            Description = "Classic",
            IsDiscontinued = false
        };

        // Act
        repo.Add(book);
        var stored = repo.GetById(1);

        // Assert
        Assert.NotNull(stored);
        Assert.Equal("Dune", stored!.Title);
    }
}
