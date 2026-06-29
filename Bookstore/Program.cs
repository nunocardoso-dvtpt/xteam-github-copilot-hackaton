using Bookstore.Application;
using Bookstore.Application.Parsing;
using Bookstore.Infrastructure.InMemory;
using Bookstore.Presentation;
using Tasks;

namespace Bookstore
{
	public sealed class Bookstore
	{
		private const string QUIT = "quit";
		private readonly IConsole console;
		private readonly ICommandHandler commandHandler;

		public static void Main(string[] args)
		{
			new Bookstore(new RealConsole()).Run();
		}

		public Bookstore(IConsole console)
		{
			this.console = console;
			var parser = new CommandParser();

			var authorRepository = new InMemoryAuthorRepository();
			var bookRepository = new InMemoryBookRepository();

			var authorService = new AuthorService(authorRepository);
			var bookCatalogService = new BookCatalogService(bookRepository, authorRepository);
			commandHandler = new CommandHandler(console, parser, bookCatalogService, authorService);
		}

		public void Run()
		{
			while (true)
			{
				try
				{
					console.Write("> ");
					var command = console.ReadLine();
					if (command is null || command.Equals(QUIT, StringComparison.OrdinalIgnoreCase))
					{
						break;
					}

					commandHandler.Handle(command);
				}
				catch (Exception e)
				{
					console.WriteLine("Error: {0}", e.Message);
				}
			}
		}
	}
}
