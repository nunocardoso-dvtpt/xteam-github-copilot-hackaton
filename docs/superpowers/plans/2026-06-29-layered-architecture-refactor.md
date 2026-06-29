# Layered Architecture Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split `Program.cs` into maintainable presentation/application/domain/infrastructure layers with interface-driven data access and full regression-safe tests.

**Architecture:** Keep `Program.cs` as composition root + command loop only, move business rules into services, and abstract data access behind repository interfaces. Provide in-memory repository implementations now so behavior stays identical while enabling future DB swap by replacing infrastructure wiring only. Preserve command-level behavior and add focused service/parser/repository tests.

**Tech Stack:** .NET 10, C#, xUnit, coverlet.collector, in-memory adapters, interface-based DI via composition root.

## Global Constraints

- Add interface-driven layers while preserving current command behavior/output.
- `addAuthor`, `showAuthors`, and `add` with `<authorId>` behavior must remain consistent.
- Unknown Author fallback (`Id=0`, `Name="Unknown Author"`) must be guaranteed.
- All user-facing output stays through `IConsole`.
- Use AAA in tests and TDD red-green for each new behavior.
- Keep in-memory data access in this refactor (no database implementation).

---

## File Structure

- Create: `Bookstore\Domain\Repositories\IBookRepository.cs`
- Create: `Bookstore\Domain\Repositories\IAuthorRepository.cs`
- Create: `Bookstore\Infrastructure\InMemory\InMemoryBookRepository.cs`
- Create: `Bookstore\Infrastructure\InMemory\InMemoryAuthorRepository.cs`
- Create: `Bookstore\Application\Parsing\ICommandParser.cs`
- Create: `Bookstore\Application\Parsing\CommandParser.cs`
- Create: `Bookstore\Application\IBookCatalogService.cs`
- Create: `Bookstore\Application\BookCatalogService.cs`
- Create: `Bookstore\Application\IAuthorService.cs`
- Create: `Bookstore\Application\AuthorService.cs`
- Modify: `Bookstore\Program.cs`
- Create: `Bookstore.Tests\ParserTests.cs`
- Create: `Bookstore.Tests\RepositoryTests.cs`
- Create: `Bookstore.Tests\ServiceTests.cs`
- Modify: `Bookstore.Tests\AddCommandTests.cs`
- Modify: `Bookstore.Tests\DiscontinueCommandTests.cs`
- Modify: `Bookstore.Tests\AuthorCommandTests.cs`
- Modify: `Bookstore.Tests\HelpAndErrorCommandTests.cs`
- Modify: `README.md`

### Task 1: Introduce repository interfaces and in-memory adapters

**Files:**
- Create: `Bookstore\Domain\Repositories\IBookRepository.cs`
- Create: `Bookstore\Domain\Repositories\IAuthorRepository.cs`
- Create: `Bookstore\Infrastructure\InMemory\InMemoryBookRepository.cs`
- Create: `Bookstore\Infrastructure\InMemory\InMemoryAuthorRepository.cs`
- Test: `Bookstore.Tests\RepositoryTests.cs`

**Interfaces:**
- Consumes: `Bookstore\Book.cs`, `Bookstore\Author.cs`
- Produces:
  - `IBookRepository.Add(Book)`, `IReadOnlyList<Book> GetAll()`, `Book? GetById(long)`, `bool ExistsTitle(string)`
  - `IAuthorRepository.Add(Author)`, `IReadOnlyList<Author> GetAll()`, `Author? GetById(long)`, `bool ExistsId(long)`

- [ ] **Step 1: Write the failing tests**

Create `Bookstore.Tests\RepositoryTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class RepositoryTests
{
    [Fact]
    public void InMemoryAuthorRepository_Seeds_Unknown_Author()
    {
        // Arrange
        var repo = new global::Bookstore.Infrastructure.InMemory.InMemoryAuthorRepository();

        // Act
        var unknown = repo.GetById(0);

        // Assert
        Assert.NotNull(unknown);
        Assert.Equal("Unknown Author", unknown!.Name);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~InMemoryAuthorRepository_Seeds_Unknown_Author"`

Expected: FAIL compile error because repository classes/interfaces do not exist.

- [ ] **Step 3: Write minimal implementation**

Create `Bookstore\Domain\Repositories\IAuthorRepository.cs`:

```csharp
namespace Bookstore.Domain.Repositories;

public interface IAuthorRepository
{
    void Add(Author author);
    IReadOnlyList<Author> GetAll();
    Author? GetById(long id);
    bool ExistsId(long id);
}
```

Create `Bookstore\Infrastructure\InMemory\InMemoryAuthorRepository.cs`:

```csharp
using Bookstore.Domain.Repositories;

namespace Bookstore.Infrastructure.InMemory;

public class InMemoryAuthorRepository : IAuthorRepository
{
    private readonly List<Author> authors =
    [
        new Author { Id = 0, Name = "Unknown Author", BornDate = default, Awards = [] }
    ];

    public void Add(Author author) => authors.Add(author);
    public IReadOnlyList<Author> GetAll() => authors;
    public Author? GetById(long id) => authors.FirstOrDefault(a => a.Id == id);
    public bool ExistsId(long id) => authors.Any(a => a.Id == id);
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~InMemoryAuthorRepository_Seeds_Unknown_Author"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Domain\Repositories\IAuthorRepository.cs Bookstore\Infrastructure\InMemory\InMemoryAuthorRepository.cs Bookstore.Tests\RepositoryTests.cs
git commit -m "refactor: add repository interfaces and author adapter"
```

### Task 2: Extract parser abstraction

**Files:**
- Create: `Bookstore\Application\Parsing\ICommandParser.cs`
- Create: `Bookstore\Application\Parsing\CommandParser.cs`
- Test: `Bookstore.Tests\ParserTests.cs`

**Interfaces:**
- Consumes: none
- Produces: `IReadOnlyList<string> ParseArguments(string commandLine)`

- [ ] **Step 1: Write the failing tests**

Create `Bookstore.Tests\ParserTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class ParserTests
{
    [Fact]
    public void CommandParser_Parses_Quoted_And_Unquoted_Tokens()
    {
        // Arrange
        var parser = new global::Bookstore.Application.Parsing.CommandParser();

        // Act
        var tokens = parser.ParseArguments("\"Dune Messiah\" 10 \"Science Fiction\" \"Classic sequel\"");

        // Assert
        Assert.Equal(4, tokens.Count);
        Assert.Equal("Dune Messiah", tokens[0]);
        Assert.Equal("10", tokens[1]);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~CommandParser_Parses_Quoted_And_Unquoted_Tokens"`

Expected: FAIL compile error because parser type does not exist.

- [ ] **Step 3: Write minimal implementation**

Create `Bookstore\Application\Parsing\ICommandParser.cs`:

```csharp
namespace Bookstore.Application.Parsing;

public interface ICommandParser
{
    IReadOnlyList<string> ParseArguments(string commandLine);
}
```

Create `Bookstore\Application\Parsing\CommandParser.cs`:

```csharp
using System.Text.RegularExpressions;

namespace Bookstore.Application.Parsing;

public class CommandParser : ICommandParser
{
    public IReadOnlyList<string> ParseArguments(string commandLine)
    {
        return Regex.Matches(commandLine ?? string.Empty, "\"([^\"]*)\"|(\\S+)")
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
            .ToList();
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~CommandParser_Parses_Quoted_And_Unquoted_Tokens"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Application\Parsing\ICommandParser.cs Bookstore\Application\Parsing\CommandParser.cs Bookstore.Tests\ParserTests.cs
git commit -m "refactor: extract command parser abstraction"
```

### Task 3: Move business rules to services

**Files:**
- Create: `Bookstore\Application\IAuthorService.cs`
- Create: `Bookstore\Application\AuthorService.cs`
- Create: `Bookstore\Application\IBookCatalogService.cs`
- Create: `Bookstore\Application\BookCatalogService.cs`
- Test: `Bookstore.Tests\ServiceTests.cs`

**Interfaces:**
- Consumes: repository interfaces + parser interface
- Produces:
  - `AuthorService.AddAuthor(...)`, `AuthorService.ShowAuthors()`
  - `BookCatalogService.AddBook(...)`, `ShowBooks()`, `DiscontinueBook(...)`, `DiscontinueAuthor(...)`

- [ ] **Step 1: Write the failing tests**

Create `Bookstore.Tests\ServiceTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class ServiceTests
{
    [Fact]
    public void BookCatalogService_Uses_Unknown_Author_Fallback()
    {
        // Arrange
        var bookRepo = new global::Bookstore.Infrastructure.InMemory.InMemoryBookRepository();
        var authorRepo = new global::Bookstore.Infrastructure.InMemory.InMemoryAuthorRepository();
        var sut = new global::Bookstore.Application.BookCatalogService(bookRepo, authorRepo);

        // Act
        var result = sut.AddBook("Book X", 999, "Fiction", "Desc");

        // Assert
        Assert.True(result.Success);
        var added = bookRepo.GetAll().Single();
        Assert.Equal("Unknown Author", added.Author);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~BookCatalogService_Uses_Unknown_Author_Fallback"`

Expected: FAIL compile error because service or book repo not implemented.

- [ ] **Step 3: Write minimal implementation**

Create `Bookstore\Application\BookCatalogService.cs` with minimal method:

```csharp
using Bookstore.Domain.Repositories;

namespace Bookstore.Application;

public class BookCatalogService
{
    private readonly IBookRepository books;
    private readonly IAuthorRepository authors;

    public BookCatalogService(IBookRepository books, IAuthorRepository authors)
    {
        this.books = books;
        this.authors = authors;
    }

    public (bool Success, string Message) AddBook(string title, long authorId, string category, string description)
    {
        var author = authors.GetById(authorId) ?? authors.GetById(0)!;
        books.Add(new Book { Id = books.GetAll().Count + 1, Title = title, Author = author.Name, CategoryId = 1, Description = description, IsDiscontinued = false });
        return (true, string.Empty);
    }
}
```

Also add `InMemoryBookRepository` + `IBookRepository` minimal implementations needed by test.

- [ ] **Step 4: Run test to verify it passes**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~BookCatalogService_Uses_Unknown_Author_Fallback"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Application\ Bookstore\Domain\Repositories\IBookRepository.cs Bookstore\Infrastructure\InMemory\InMemoryBookRepository.cs Bookstore.Tests\ServiceTests.cs
git commit -m "refactor: move core book logic into services"
```

### Task 4: Refactor Program.cs to thin presentation/composition

**Files:**
- Modify: `Bookstore\Program.cs`
- Modify: `Bookstore.Tests\AddCommandTests.cs`
- Modify: `Bookstore.Tests\DiscontinueCommandTests.cs`
- Modify: `Bookstore.Tests\AuthorCommandTests.cs`
- Modify: `Bookstore.Tests\HelpAndErrorCommandTests.cs`

**Interfaces:**
- Consumes: application services + parser interfaces
- Produces: Program with routing only and service calls

- [ ] **Step 1: Write the failing integration test**

Add to `Bookstore.Tests\HelpAndErrorCommandTests.cs`:

```csharp
[Fact]
public void Refactored_Program_Help_And_ShowAuthors_Behavior_Remains_Stable()
{
    // Arrange
    var console = new ScriptedConsole(["help", "showAuthors", "quit"]);
    var sut = new global::Bookstore.Bookstore(console);

    // Act
    sut.Run();

    // Assert
    Assert.Contains(console.Output, line => line.Contains("showAuthors"));
    Assert.Contains(console.Output, line => line.Contains("[0] Unknown Author"));
}
```

- [ ] **Step 2: Run test to verify baseline**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~Refactored_Program_Help_And_ShowAuthors_Behavior_Remains_Stable"`

Expected: PASS pre-refactor (safety anchor).

- [ ] **Step 3: Implement thin Program**

Refactor `Program.cs` so command handlers delegate to services:

```csharp
private readonly IBookCatalogService bookService;
private readonly IAuthorService authorService;
private readonly ICommandParser parser;
```

In constructor:

```csharp
var authorRepo = new InMemoryAuthorRepository();
var bookRepo = new InMemoryBookRepository();
var parser = new CommandParser();
var authorService = new AuthorService(authorRepo, parser);
var bookService = new BookCatalogService(bookRepo, authorRepo, parser);
```

Handlers should call service methods and print returned messages.

- [ ] **Step 4: Run focused tests**

Run:

```powershell
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AddCommandTests|FullyQualifiedName~DiscontinueCommandTests|FullyQualifiedName~AuthorCommandTests|FullyQualifiedName~HelpAndErrorCommandTests"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Program.cs Bookstore.Tests\
git commit -m "refactor: make program a thin composition and routing layer"
```

### Task 5: Documentation + full verification

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes: final layered structure
- Produces: updated architecture section and unchanged runtime command docs

- [ ] **Step 1: Write doc mismatch check**

Run:

`rg "Application|Infrastructure|Domain|Program.cs" README.md -n`

Expected: missing or stale architecture references before update.

- [ ] **Step 2: Update README architecture section**

Add section:

```markdown
### Architecture layers
- Presentation: `Program.cs` (command loop + composition)
- Application: services and parser abstractions
- Domain: entities + repository interfaces
- Infrastructure: in-memory repository implementations
```

- [ ] **Step 3: Full verification**

Run:

```powershell
dotnet build .\Bookstore.sln
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --settings .\Bookstore.Tests\Coverage\coverage.runsettings --collect:"XPlat Code Coverage"
```

Expected:
- Build succeeds with 0 errors.
- All tests pass.
- Coverage file generated under `Bookstore.Tests\TestResults\**\coverage.cobertura.xml`.

- [ ] **Step 4: Commit**

```bash
git add README.md
git commit -m "docs: document layered architecture and extension path"
```

- [ ] **Step 5: Validate plan task closure**

Run:

`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj`

Expected: PASS and no failing tests remain.

## Self-Review Checklist (applied)

1. **Spec coverage:** Plan covers layering split, interfaces, data access abstraction, unknown-author requirement continuity, and tests.
2. **Placeholder scan:** Every task has explicit files, test commands, and implementation snippets.
3. **Type consistency:** Repository/service/parser interfaces and Program composition references use consistent names across tasks.

