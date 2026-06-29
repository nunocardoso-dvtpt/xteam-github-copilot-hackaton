namespace Bookstore.Application;

public interface IBookCatalogService
{
    OperationResult AddBook(string title, long authorId, string category, string description);
    IReadOnlyList<Book> GetBooks();
    OperationResult DiscontinueBook(string idString);
    OperationResult DiscontinueAuthor(string authorName);
}
