# Author Actions and Book Author Resolution Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `addAuthor` and `showAuthors` actions, introduce author ID-based resolution for `add`, and fallback unknown authors to ID `0 - Unknown Author`.

**Architecture:** Keep current in-memory model and command loop in `Bookstore\Program.cs`. Add a focused `Author` domain model and an in-memory author registry initialized with reserved ID `0`. Keep `Book.Author` string unchanged; resolve author name from `authorId` at add-time for low-risk integration.

**Tech Stack:** C# (.NET 10), xUnit, Bookstore CLI command parser, AAA + TDD.

## Global Constraints

- Add two commands: `addAuthor` and `showAuthors`.
- Author fields: `Id`, `Name`, `BornDate (YYYY-MM-DD)`, `Awards List`.
- `add` must use `authorId` and fallback to `Id=0, Name="Unknown Author"` when author does not exist.
- `show` output must include author name.
- Keep in-memory runtime model only (no DB/file persistence).
- Preserve `IConsole` for all user-facing output.
- Implement tests with strict AAA structure and TDD red-green cycle.

---

## File Structure

- Create: `Bookstore\Author.cs` (author entity)
- Modify: `Bookstore\Program.cs` (command routing, author registry, new commands, add/show behavior)
- Modify: `Bookstore\Book.cs` (no shape change expected; verify compatibility only)
- Create: `Bookstore.Tests\AuthorCommandTests.cs` (addAuthor/showAuthors tests)
- Modify: `Bookstore.Tests\AddCommandTests.cs` (update add command tests to authorId flow)
- Modify: `Bookstore.Tests\HelpAndErrorCommandTests.cs` (include new command help assertions)
- Modify: `README.md` (command contract updates + examples)

### Task 1: Add Author domain model

**Files:**
- Create: `Bookstore\Author.cs`
- Test: `Bookstore.Tests\AuthorCommandTests.cs`

**Interfaces:**
- Consumes: none
- Produces:
  - `public class Author`
  - Properties: `long Id`, `string Name`, `DateOnly BornDate`, `List<string> Awards`

- [ ] **Step 1: Write the failing test**

Create `Bookstore.Tests\AuthorCommandTests.cs` with:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class AuthorCommandTests
{
    [Fact]
    public void Author_Model_Holds_All_Required_Fields()
    {
        // Arrange
        var sut = new global::Bookstore.Author
        {
            Id = 1,
            Name = "Frank Herbert",
            BornDate = new DateOnly(1920, 10, 8),
            Awards = ["Hugo", "Nebula"]
        };

        // Act
        var awardsCount = sut.Awards.Count;

        // Assert
        Assert.Equal(1, sut.Id);
        Assert.Equal("Frank Herbert", sut.Name);
        Assert.Equal(new DateOnly(1920, 10, 8), sut.BornDate);
        Assert.Equal(2, awardsCount);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~Author_Model_Holds_All_Required_Fields"`

Expected: FAIL compile error that `Bookstore.Author` does not exist.

- [ ] **Step 3: Write minimal implementation**

Create `Bookstore\Author.cs`:

```csharp
namespace Bookstore;

public class Author
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly BornDate { get; set; }
    public List<string> Awards { get; set; } = [];
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~Author_Model_Holds_All_Required_Fields"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Author.cs Bookstore.Tests\AuthorCommandTests.cs
git commit -m "feat: add author domain model"
```

### Task 2: Add `addAuthor` and `showAuthors` commands

**Files:**
- Modify: `Bookstore\Program.cs`
- Test: `Bookstore.Tests\AuthorCommandTests.cs`

**Interfaces:**
- Consumes:
  - `Author` model
  - Existing `ParseArguments(string)` helper
- Produces:
  - `private readonly List<Author> authors`
  - `private void AddAuthor(string commandLine)`
  - `private void ShowAuthors()`
  - Reserved default author: `Id=0, Name="Unknown Author"`

- [ ] **Step 1: Write the failing tests**

Append to `Bookstore.Tests\AuthorCommandTests.cs`:

```csharp
[Fact]
public void AddAuthor_Registers_Author_And_ShowAuthors_Displays_It()
{
    // Arrange
    var console = new ScriptedConsole([
        "addAuthor 1 \"Frank Herbert\" 1920-10-08 \"Hugo,Nebula\"",
        "showAuthors",
        "quit"
    ]);
    var sut = new global::Bookstore.Bookstore(console);

    // Act
    sut.Run();

    // Assert
    Assert.Contains(console.Output, line => line.Contains("Frank Herbert"));
    Assert.Contains(console.Output, line => line.Contains("1920-10-08"));
    Assert.Contains(console.Output, line => line.Contains("Hugo") && line.Contains("Nebula"));
}

[Fact]
public void ShowAuthors_Includes_Unknown_Author()
{
    // Arrange
    var console = new ScriptedConsole(["showAuthors", "quit"]);
    var sut = new global::Bookstore.Bookstore(console);

    // Act
    sut.Run();

    // Assert
    Assert.Contains(console.Output, line => line.Contains("[0] Unknown Author"));
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AddAuthor_Registers_Author_And_ShowAuthors_Displays_It|FullyQualifiedName~ShowAuthors_Includes_Unknown_Author"`

Expected: FAIL because commands are unknown.

- [ ] **Step 3: Write minimal implementation**

In `Bookstore\Program.cs`, add:

```csharp
private readonly List<Author> authors = [new Author
{
    Id = 0,
    Name = "Unknown Author",
    BornDate = default,
    Awards = []
}];
```

Extend switch:

```csharp
case "addAuthor":
    AddAuthor(argument);
    break;
case "showAuthors":
    ShowAuthors();
    break;
```

Add methods:

```csharp
private void AddAuthor(string commandLine)
{
    var args = ParseArguments(commandLine);
    if (args.Count != 4)
    {
        console.WriteLine("Usage: addAuthor <id> \"<name>\" <YYYY-MM-DD> \"<award1,award2,...>\"");
        return;
    }

    if (!long.TryParse(args[0], out var id))
    {
        console.WriteLine("Invalid author ID: \"{0}\".", args[0]);
        return;
    }

    if (id == 0)
    {
        console.WriteLine("Author ID 0 is reserved for Unknown Author.");
        return;
    }

    if (authors.Any(a => a.Id == id))
    {
        console.WriteLine("Duplicate author ID: {0}.", id);
        return;
    }

    if (!DateOnly.TryParseExact(args[2], "yyyy-MM-dd", out var bornDate))
    {
        console.WriteLine("Invalid born date. Use YYYY-MM-DD.");
        return;
    }

    var awards = args[3].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    authors.Add(new Author { Id = id, Name = args[1], BornDate = bornDate, Awards = awards });
}

private void ShowAuthors()
{
    foreach (var author in authors)
    {
        var awards = author.Awards.Count > 0 ? string.Join(", ", author.Awards) : "(none)";
        var bornDate = author.BornDate == default ? "N/A" : author.BornDate.ToString("yyyy-MM-dd");
        console.WriteLine("[{0}] {1} | Born: {2} | Awards: {3}", author.Id, author.Name, bornDate, awards);
    }
    console.WriteLine();
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AuthorCommandTests"`

Expected: PASS for newly added author command tests.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Program.cs Bookstore.Tests\AuthorCommandTests.cs
git commit -m "feat: add addAuthor and showAuthors commands"
```

### Task 3: Update `add` command to resolve author by ID with fallback

**Files:**
- Modify: `Bookstore\Program.cs`
- Modify: `Bookstore.Tests\AddCommandTests.cs`

**Interfaces:**
- Consumes:
  - `authors` list with reserved ID `0`
- Produces:
  - Updated add contract: `add "<title>" <authorId> "<category>" "<description>"`
  - Fallback behavior to Unknown Author for missing author IDs

- [ ] **Step 1: Write the failing tests**

Update/add tests in `Bookstore.Tests\AddCommandTests.cs`:

```csharp
[Fact]
public void Add_With_Known_AuthorId_Shows_Resolved_Author_Name()
{
    // Arrange
    var console = new ScriptedConsole([
        "addAuthor 10 \"Frank Herbert\" 1920-10-08 \"Hugo\"",
        "add \"Dune\" 10 \"Science Fiction\" \"Classic\"",
        "show",
        "quit"
    ]);
    var sut = new global::Bookstore.Bookstore(console);

    // Act
    sut.Run();

    // Assert
    Assert.Contains(console.Output, line => line.Contains("Dune") && line.Contains("Frank Herbert"));
}

[Fact]
public void Add_With_Unknown_AuthorId_Falls_Back_To_Unknown_Author()
{
    // Arrange
    var console = new ScriptedConsole([
        "add \"Book X\" 999 \"Fiction\" \"Desc\"",
        "show",
        "quit"
    ]);
    var sut = new global::Bookstore.Bookstore(console);

    // Act
    sut.Run();

    // Assert
    Assert.Contains(console.Output, line => line.Contains("Book X") && line.Contains("Unknown Author"));
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~Add_With_Known_AuthorId_Shows_Resolved_Author_Name|FullyQualifiedName~Add_With_Unknown_AuthorId_Falls_Back_To_Unknown_Author"`

Expected: FAIL because current `add` still expects author name string.

- [ ] **Step 3: Write minimal implementation**

In `Bookstore\Program.cs`, update `Add`/`AddBook`:

```csharp
private void Add(string commandLine)
{
    var arguments = ParseArguments(commandLine);
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

    AddBook(arguments[0], authorId, arguments[2], arguments[3]);
}

private void AddBook(string title, long authorId, string category, string description)
{
    // existing duplicate/category checks remain
    var resolvedAuthor = authors.FirstOrDefault(a => a.Id == authorId) ?? authors.First(a => a.Id == 0);

    books.Add(new Book
    {
        Id = NextId(),
        Title = title,
        CategoryId = categoryId,
        Description = description,
        Author = resolvedAuthor.Name,
        IsDiscontinued = false
    });
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AddCommandTests"`

Expected: PASS for updated add command tests.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Program.cs Bookstore.Tests\AddCommandTests.cs
git commit -m "feat: resolve book authors by authorId with fallback"
```

### Task 4: Validation edge cases and help contract updates

**Files:**
- Modify: `Bookstore\Program.cs`
- Modify: `Bookstore.Tests\AuthorCommandTests.cs`
- Modify: `Bookstore.Tests\HelpAndErrorCommandTests.cs`

**Interfaces:**
- Consumes:
  - `AddAuthor`/`Add` handlers
- Produces:
  - Stable validation/error copy for invalid author ID/date/duplicates
  - Help includes new commands and updated add signature

- [ ] **Step 1: Write the failing tests**

Add to `Bookstore.Tests\AuthorCommandTests.cs`:

```csharp
[Fact]
public void AddAuthor_Rejects_Invalid_Date_Format()
{
    // Arrange
    var console = new ScriptedConsole([
        "addAuthor 3 \"Author\" 1920/10/08 \"Award\"",
        "quit"
    ]);
    var sut = new global::Bookstore.Bookstore(console);

    // Act
    sut.Run();

    // Assert
    Assert.Contains(console.Output, line => line.Contains("Invalid born date"));
}
```

Update `Bookstore.Tests\HelpAndErrorCommandTests.cs` assertions to include:

```csharp
Assert.Contains(console.Output, line => line.Contains("add \"<title>\" <authorId> \"<category>\" \"<description>\""));
Assert.Contains(console.Output, line => line.Contains("addAuthor <id> \"<name>\" <YYYY-MM-DD> \"<award1,award2,...>\""));
Assert.Contains(console.Output, line => line.Contains("showAuthors"));
```

- [ ] **Step 2: Run tests to verify they fail**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AddAuthor_Rejects_Invalid_Date_Format|FullyQualifiedName~Help_Prints_Supported_Commands"`

Expected: FAIL until help/output contracts are updated.

- [ ] **Step 3: Write minimal implementation**

In `Help()` update command list:

```csharp
console.WriteLine("  show");
console.WriteLine("  showAuthors");
console.WriteLine("  add \"<title>\" <authorId> \"<category>\" \"<description>\"");
console.WriteLine("  addAuthor <id> \"<name>\" <YYYY-MM-DD> \"<award1,award2,...>\"");
console.WriteLine("  discontinueBook <book ID>");
console.WriteLine("  discontinueAuthor <author name>");
console.WriteLine("  help");
console.WriteLine("  quit");
```

Ensure validation copy matches test assertions.

- [ ] **Step 4: Run tests to verify they pass**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AuthorCommandTests|FullyQualifiedName~HelpAndErrorCommandTests"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore\Program.cs Bookstore.Tests\AuthorCommandTests.cs Bookstore.Tests\HelpAndErrorCommandTests.cs
git commit -m "test: cover author validation and help contract"
```

### Task 5: Documentation and full verification

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes:
  - Final command contracts from `Program.cs`
- Produces:
  - Updated command documentation and examples for author workflow

- [ ] **Step 1: Write failing doc-check test (manual checklist)**

Create a manual checklist (in PR notes, not code):

```text
- README lists add command with <authorId>
- README lists addAuthor command format
- README lists showAuthors command
- README includes at least one valid workflow example
```

- [ ] **Step 2: Run verification to confirm mismatch before update**

Run:  
`rg "addAuthor|showAuthors|<authorId>" README.md -n`

Expected: Missing/incomplete matches before update.

- [ ] **Step 3: Write minimal implementation**

Update README runtime command section to:

```markdown
- `show`
- `showAuthors`
- `add "<title>" <authorId> "<category>" "<description>"`
- `addAuthor <id> "<name>" <YYYY-MM-DD> "<award1,award2,...>"`
- `discontinueBook <id>`
- `discontinueAuthor <author>`
- `help`
- `quit`
```

Add example:

```powershell
addAuthor 10 "Frank Herbert" 1920-10-08 "Hugo,Nebula"
add "Dune" 10 "Science Fiction" "Classic novel"
show
showAuthors
```

- [ ] **Step 4: Run full verification**

Run:

```powershell
dotnet build .\Bookstore.sln
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --settings .\Bookstore.Tests\Coverage\coverage.runsettings --collect:"XPlat Code Coverage"
```

Expected:
- Build succeeds with 0 errors.
- All tests pass.
- Coverage file generated under `Bookstore.Tests\TestResults\**\coverage.cobertura.xml`.

- [ ] **Step 5: Commit**

```bash
git add README.md
git commit -m "docs: add author command usage examples"
```

## Self-Review Checklist (applied)

1. **Spec coverage:** Plan covers new actions, author model, fallback unknown author, and show output author name.
2. **Placeholder scan:** No TODO/TBD placeholders; each task has concrete files, code, and commands.
3. **Type consistency:** `Author.Id`/`Book.Author` and `add` signature with `authorId` are consistent across tasks/tests/docs.

