using Bookstore.Application;
using Bookstore.Application.Parsing;
using Tasks;

namespace Bookstore.Presentation;

public class CommandHandler : ICommandHandler
{
    private readonly IConsole console;
    private readonly ICommandParser parser;
    private readonly IBookCatalogService bookCatalogService;
    private readonly IAuthorService authorService;

    public CommandHandler(
        IConsole console,
        ICommandParser parser,
        IBookCatalogService bookCatalogService,
        IAuthorService authorService)
    {
        this.console = console;
        this.parser = parser;
        this.bookCatalogService = bookCatalogService;
        this.authorService = authorService;
    }

    public void Handle(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            console.WriteLine("Please provide a command. Type \"help\" to see available commands.");
            return;
        }

        var commandParts = commandLine.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = commandParts[0];
        var argument = commandParts.Length > 1 ? commandParts[1] : string.Empty;

        switch (command)
        {
            case "show":
                Show();
                break;
            case "add":
                Add(argument);
                break;
            case "addAuthor":
                AddAuthor(argument);
                break;
            case "showAuthors":
                ShowAuthors();
                break;
            case "discontinueBook":
                DiscontinueBook(argument);
                break;
            case "discontinueAuthor":
                DiscontinueByAuthor(argument);
                break;
            case "help":
                Help();
                break;
            default:
                Error(command);
                break;
        }
    }

    private void Show()
    {
        var books = bookCatalogService.GetBooks();
        if (books.Count == 0)
        {
            console.WriteLine("No books registered.");
            return;
        }

        foreach (var book in books)
        {
            console.WriteLine("[{0}] {1}: {2} {3}", book.Id, book.Title, book.Author, book.IsDiscontinued ? "(discontinued)" : string.Empty);
        }

        console.WriteLine();
    }

    private void Add(string commandLine)
    {
        var arguments = parser.ParseArguments(commandLine);
        if (arguments.Count != 4)
        {
            console.WriteLine("Usage: add \"<title>\" <authorId> \"<category>\" \"<description>\"");
            return;
        }

        if (!long.TryParse(arguments[1], out var authorId))
        {
            console.WriteLine("Invalid author ID: \"{0}\".", arguments[1]);
            return;
        }

        var result = bookCatalogService.AddBook(arguments[0], authorId, arguments[2], arguments[3]);
        if (!result.Success)
        {
            console.WriteLine(result.Message);
        }
    }

    private void AddAuthor(string commandLine)
    {
        var arguments = parser.ParseArguments(commandLine);
        if (arguments.Count != 4)
        {
            console.WriteLine("Usage: addAuthor <id> \"<name>\" <YYYY-MM-DD> \"<award1,award2,...>\"");
            return;
        }

        if (!long.TryParse(arguments[0], out var id))
        {
            console.WriteLine("Invalid author ID: \"{0}\".", arguments[0]);
            return;
        }

        var result = authorService.AddAuthor(id, arguments[1], arguments[2], arguments[3]);
        if (!result.Success)
        {
            console.WriteLine(result.Message);
        }
    }

    private void ShowAuthors()
    {
        foreach (var author in authorService.GetAuthors())
        {
            var awards = author.Awards.Count > 0 ? string.Join(", ", author.Awards) : "(none)";
            var bornDate = author.BornDate == default ? "N/A" : author.BornDate.ToString("yyyy-MM-dd");
            console.WriteLine("[{0}] {1} | Born: {2} | Awards: {3}", author.Id, author.Name, bornDate, awards);
        }

        console.WriteLine();
    }

    private void DiscontinueBook(string idString)
    {
        var result = bookCatalogService.DiscontinueBook(idString);
        if (!result.Success)
        {
            console.WriteLine(result.Message);
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            console.WriteLine(result.Message);
        }
    }

    private void DiscontinueByAuthor(string authorName)
    {
        var result = bookCatalogService.DiscontinueAuthor(authorName);
        if (!result.Success)
        {
            console.WriteLine(result.Message);
        }
    }

    private void Help()
    {
        console.WriteLine("Commands:");
        console.WriteLine("  show");
        console.WriteLine("  showAuthors");
        console.WriteLine("  add \"<title>\" <authorId> \"<category>\" \"<description>\"");
        console.WriteLine("  addAuthor <id> \"<name>\" <YYYY-MM-DD> \"<award1,award2,...>\"");
        console.WriteLine("  discontinueBook <book ID>");
        console.WriteLine("  discontinueAuthor <author name>");
        console.WriteLine("  help");
        console.WriteLine("  quit");
        console.WriteLine();
    }

    private void Error(string command)
    {
        console.WriteLine("I don't know what the command \"{0}\" is.", command);
    }
}
