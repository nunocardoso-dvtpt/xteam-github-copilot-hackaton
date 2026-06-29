namespace Bookstore.Domain.Repositories;

public interface IAuthorRepository
{
    void Add(Author author);
    IReadOnlyList<Author> GetAll();
    Author? GetById(long id);
    bool ExistsId(long id);
}
