using Bookstore.Domain.Repositories;

namespace Bookstore.Infrastructure.InMemory;

public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> books = [];

    public void Add(Book book) => books.Add(book);

    public IReadOnlyList<Book> GetAll() => books;

    public Book? GetById(long id) => books.FirstOrDefault(book => book.Id == id);

    public bool ExistsTitle(string title) =>
        books.Any(book => book.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
}
