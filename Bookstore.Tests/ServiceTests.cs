using Xunit;

namespace Bookstore.Tests;

public class ServiceTests
{
    [Fact]
    public void BookCatalogService_Uses_Unknown_Author_Fallback()
    {
        // Arrange
        var bookRepo = new global::Bookstore.Infrastructure.InMemory.InMemoryBookRepository();
        var authorRepo = new global::Bookstore.Infrastructure.InMemory.InMemoryAuthorRepository();
        var sut = new global::Bookstore.Application.BookCatalogService(bookRepo, authorRepo);

        // Act
        var result = sut.AddBook("Book X", 999, "Fiction", "Desc");

        // Assert
        Assert.True(result.Success);
        var added = bookRepo.GetAll().Single();
        Assert.Equal("Unknown Author", added.Author);
    }
}
