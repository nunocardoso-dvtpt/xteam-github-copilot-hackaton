using Bookstore.Domain.Repositories;

namespace Bookstore.Application;

public class BookCatalogService : IBookCatalogService
{
    private readonly IBookRepository books;
    private readonly IAuthorRepository authors;

    private static readonly IReadOnlyDictionary<string, int> CategoryNameToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "Fiction", 1 },
        { "Romance", 2 },
        { "Science Fiction", 3 },
        { "Fantasy", 4 },
        { "Mystery", 5 },
        { "Biography", 6 }
    };

    public BookCatalogService(IBookRepository books, IAuthorRepository authors)
    {
        this.books = books;
        this.authors = authors;
    }

    public OperationResult AddBook(string title, long authorId, string category, string description)
    {
        if (books.ExistsTitle(title))
        {
            return new OperationResult(false, $"Duplicate book with the name \"{title}\".");
        }

        if (!CategoryNameToId.TryGetValue(category, out var categoryId))
        {
            return new OperationResult(false, $"Invalid category. Valid categories: {string.Join(", ", CategoryNameToId.Keys)}");
        }

        var resolvedAuthor = authors.GetById(authorId) ?? authors.GetById(0)!;
        var nextId = books.GetAll().Count == 0 ? 1 : books.GetAll().Max(b => b.Id) + 1;

        books.Add(new Book
        {
            Id = nextId,
            Title = title,
            Author = resolvedAuthor.Name,
            CategoryId = categoryId,
            Description = description,
            IsDiscontinued = false
        });

        return new OperationResult(true, string.Empty);
    }

    public IReadOnlyList<Book> GetBooks() => books.GetAll();

    public OperationResult DiscontinueBook(string idString)
    {
        if (!int.TryParse(idString, out var id))
        {
            return new OperationResult(false, $"Invalid book ID: \"{idString}\". Please provide a numeric ID.");
        }

        var identifiedBook = books.GetById(id);
        if (identifiedBook is null)
        {
            return new OperationResult(false, $"Book with ID {id} was not found.");
        }

        identifiedBook.IsDiscontinued = true;
        return new OperationResult(true, $"Discontinued: '{identifiedBook.Title}' (ID: {identifiedBook.Id})");
    }

    public OperationResult DiscontinueAuthor(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
        {
            return new OperationResult(false, "Please provide an author name.");
        }

        var foundAny = false;
        foreach (var currentBook in books.GetAll().Where(book => book.Author.Equals(authorName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            if (!currentBook.IsDiscontinued)
            {
                currentBook.IsDiscontinued = true;
                foundAny = true;
            }
        }

        if (!foundAny)
        {
            return new OperationResult(false, $"Could not find any active books by author: {authorName}");
        }

        return new OperationResult(true, string.Empty);
    }
}
