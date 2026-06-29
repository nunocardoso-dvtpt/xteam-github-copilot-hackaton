using System.Globalization;
using Bookstore.Domain.Repositories;

namespace Bookstore.Application;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository authors;

    public AuthorService(IAuthorRepository authors)
    {
        this.authors = authors;
    }

    public OperationResult AddAuthor(long id, string name, string bornDateText, string awardsText)
    {
        if (id == 0)
        {
            return new OperationResult(false, "Author ID 0 is reserved for Unknown Author.");
        }

        if (authors.ExistsId(id))
        {
            return new OperationResult(false, $"Duplicate author ID: {id}.");
        }

        if (!DateOnly.TryParseExact(bornDateText, "yyyy-MM-dd", null, DateTimeStyles.None, out var bornDate))
        {
            return new OperationResult(false, "Invalid born date. Use YYYY-MM-DD.");
        }

        var awards = awardsText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        authors.Add(new Author
        {
            Id = id,
            Name = name,
            BornDate = bornDate,
            Awards = awards
        });

        return new OperationResult(true, string.Empty);
    }

    public IReadOnlyList<Author> GetAuthors() => authors.GetAll();
}
