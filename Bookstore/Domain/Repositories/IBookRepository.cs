namespace Bookstore.Domain.Repositories;

public interface IBookRepository
{
    void Add(Book book);
    IReadOnlyList<Book> GetAll();
    Book? GetById(long id);
    bool ExistsTitle(string title);
}
