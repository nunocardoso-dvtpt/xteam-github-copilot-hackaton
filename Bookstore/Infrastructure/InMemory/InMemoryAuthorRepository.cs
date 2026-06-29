using Bookstore.Domain.Repositories;

namespace Bookstore.Infrastructure.InMemory;

public class InMemoryAuthorRepository : IAuthorRepository
{
    private readonly List<Author> authors =
    [
        new Author
        {
            Id = 0,
            Name = "Unknown Author",
            BornDate = default,
            Awards = []
        }
    ];

    public void Add(Author author) => authors.Add(author);

    public IReadOnlyList<Author> GetAll() => authors;

    public Author? GetById(long id) => authors.FirstOrDefault(a => a.Id == id);

    public bool ExistsId(long id) => authors.Any(a => a.Id == id);
}
