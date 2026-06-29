namespace Bookstore.Application;

public interface IAuthorService
{
    OperationResult AddAuthor(long id, string name, string bornDateText, string awardsText);
    IReadOnlyList<Author> GetAuthors();
}
